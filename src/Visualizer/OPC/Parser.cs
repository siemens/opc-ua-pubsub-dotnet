// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using opc.ua.pubsub.dotnet.binary.DataPoints;
using opc.ua.pubsub.dotnet.binary.Decode;
using opc.ua.pubsub.dotnet.binary.Header;
using opc.ua.pubsub.dotnet.binary.Messages;
using opc.ua.pubsub.dotnet.binary.Messages.Chunk;
using opc.ua.pubsub.dotnet.binary.Messages.Delta;
using opc.ua.pubsub.dotnet.binary.Messages.Key;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using opc.ua.pubsub.dotnet.binary.Messages.Meta.Structure;
using log4net;
using opc.ua.pubsub.dotnet.client.Interfaces;
using opc.ua.pubsub.dotnet.visualizer.UI;
using File = opc.ua.pubsub.dotnet.binary.DataPoints.File;

namespace opc.ua.pubsub.dotnet.visualizer.OPC
{
    public class Parser
    {
        private static readonly ILog Logger = LogManager.GetLogger( MethodBase.GetCurrentMethod()
                                                                              .DeclaringType
                                                                  );
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<ushort, BindingSource>> m_Bindings;
        private readonly BlockingCollection<Publisher>                                             m_PublisherQueue;
        private readonly BlockingCollection<DataPointCollection>                                   m_ValueQueue;
        private readonly VisualizerForm                                                            m_VisualizerForm;

        public Parser( BlockingCollection<Publisher>                                             publisherQueue,
                       BlockingCollection<DataPointCollection>                                   valueQueue,
                       VisualizerForm                                                            form,
                       ConcurrentDictionary<string, ConcurrentDictionary<ushort, BindingSource>> bindings
                )
        {
            m_PublisherQueue = publisherQueue;
            m_ValueQueue     = valueQueue;
            m_VisualizerForm = form;
            m_Bindings       = bindings;
        }

        public void ClientOnFileReceived( object sender, FileReceivedEventArgs eventArgs )
        {
            if ( eventArgs.File == null )
            {
                return;
            }
            Publisher publisher = new Publisher
                                  {
                                          PublisherID = eventArgs.PublisherId
                                  };
            DataPointCollection dp = new DataPointCollection
                                     {
                                             PublisherID     = eventArgs.PublisherId,
                                             DataSetWriterID = 2000
                                     };
            dp.Values = new List<DataPointValue>();
            dp.Values.Add( eventArgs.File );
            m_ValueQueue.Add( dp );
            publisher.DataSetWriterID = 2000;
            publisher.NumberOfDeltaMessages++;
            m_PublisherQueue.Add( publisher );
        }

        public void OnMessageDecoded( object sender, MessageDecodedEventArgs eventArgs )
        {
            NetworkMessage parsedMessage = eventArgs.Message;
            if ( parsedMessage == null
              || ( parsedMessage.NetworkMessageHeader.ExtendedFlags2.MessageType != MessageType.DataSetMessage
                && parsedMessage.NetworkMessageHeader.ExtendedFlags2.MessageType != MessageType.DiscoveryResponse ) )
            {
                return;
            }
            if ( parsedMessage.NetworkMessageHeader == null )
            {
                return;
            }
            string publisherID = parsedMessage.NetworkMessageHeader.PublisherID.Value;
            Publisher publisher = new Publisher
                                  {
                                          PublisherID = publisherID
                                  };
            if ( parsedMessage is ChunkedMessage chunkMessage )
            {
                publisher.DataSetWriterID = chunkMessage.PayloadHeader.DataSetWriterID;
                publisher.NumberOfChunkedMessage++;
            }
            if ( parsedMessage is DataFrame dataMessage )
            {
                publisher.DataSetWriterID = dataMessage.PayloadHeader.DataSetWriterID[0];
                if ( dataMessage.Flags2?.DataSetMessageType == DataSetMessageTypeEnum.KeepAlive )
                {
                    publisher.NumberOfKeepAliveMessages++;
                }
                else
                {
                    publisher.Major = dataMessage.ConfigurationVersion.Major;
                    publisher.Minor = dataMessage.ConfigurationVersion.Minor;
                }
            }
            if ( parsedMessage is MetaFrame metaMessage )
            {
                InitDetailsFromMeta( metaMessage );
                publisher.Major           = metaMessage.ConfigurationVersion.Major;
                publisher.Minor           = metaMessage.ConfigurationVersion.Minor;
                publisher.DataSetWriterID = metaMessage.DataSetWriterID;
                publisher.NumberOfMetaMessages++;
            }
            if ( parsedMessage is KeyFrame keyMessage )
            {
                DataPointCollection dp = new DataPointCollection
                                         {
                                                 PublisherID     = publisherID,
                                                 Values          = keyMessage.Items,
                                                 DataSetWriterID = keyMessage.PayloadHeader.DataSetWriterID[0],
                                                 Timestamp       = keyMessage.Timestamp.ToFileTime()
                                         };
                m_ValueQueue.Add( dp );
                publisher.DataSetWriterID = keyMessage.PayloadHeader.DataSetWriterID[0];
                publisher.NumberOfKeyMessages++;
                publisher.Timestamp = keyMessage.Timestamp;
            }
            if ( parsedMessage is DeltaFrame deltaMessage )
            {
                DataPointCollection dp = new DataPointCollection
                                         {
                                                 PublisherID     = publisherID,
                                                 Values          = deltaMessage.Items,
                                                 DataSetWriterID = deltaMessage.PayloadHeader.DataSetWriterID[0],
                                                 Timestamp       = deltaMessage.Timestamp.ToFileTime()
                                         };
                m_ValueQueue.Add( dp );
                publisher.DataSetWriterID = deltaMessage.PayloadHeader.DataSetWriterID[0];
                publisher.NumberOfDeltaMessages++;
                publisher.Timestamp = deltaMessage.Timestamp;
            }
            m_PublisherQueue.Add( publisher );
        }

        private void InitDetailsFromMeta( MetaFrame metaMessage )
        {
            if ( m_VisualizerForm.InvokeRequired )
            {
                m_VisualizerForm.Invoke( new Action<MetaFrame>( InitDetailsFromMeta ), metaMessage );
            }
            else
            {
                try
                {
                    string publisherID = metaMessage.NetworkMessageHeader.PublisherID.Value;
                    ushort writerID    = metaMessage.DataSetWriterID;
                    ConcurrentDictionary<ushort, BindingSource> writerDictionary =
                            m_Bindings.GetOrAdd( publisherID, pubID => new ConcurrentDictionary<ushort, BindingSource>() );
                    BindingSource bs = writerDictionary.GetOrAdd( writerID, s => new BindingSource() );
                    bs.Clear();
                    m_VisualizerForm.ResetGroupedTypeIndex( publisherID, writerID );

                    if ( metaMessage.FieldMetaDataList != null )
                    {
                        for ( int i = 0; i < metaMessage.FieldMetaDataList.Count; i++ )
                        {
                            FieldMetaData fieldMetaData = metaMessage.FieldMetaDataList[i];
                            bool isOldEnum = false,
                                 isGroupedDataType = false;
                            if ( metaMessage.StructureDataTypes.TryGetValue( fieldMetaData.DataType, out StructureDescription structDesc ) )
                            {
                                if ( metaMessage.EnumDataTypes != null
                                  && structDesc.Name.Name.ToString()
                                               .Contains( "EnumValue" ) )
                                {
                                    isOldEnum = true;
                                }
                            }
                            if ( !isOldEnum )
                            {
                                isGroupedDataType = ProcessValueFactory.GetNodeIDType( fieldMetaData.DataType ) == NodeIDType.GroupDataTypeTimeSeries;
                            }
                            DataPointBase dp = null;
                            if ( fieldMetaData.DataType == File.PreDefinedNodeID )
                            {
                                dp = new FileDataPoint();
                            }
                            else
                            {
                                dp = new ProcessDataPoint();
                            }
                            dp.Index = fieldMetaData.Index;
                            dp.Name = dp.GetType()
                                        .Name
                                   == "FileDataPoint"
                                              ? Path.GetFileName( fieldMetaData.Name.Value )
                                              : fieldMetaData.Name.Value;
                            bs.Add( dp );
                            if ( isGroupedDataType )
                            {
                                m_VisualizerForm.AddGroupDataTypeIndex( publisherID, writerID, bs.Count - 1 );
                                List<StructureField> fields = metaMessage.StructureDataTypes[fieldMetaData.DataType]
                                                                         .Fields;
                                int count = fields.Count;
                                for ( int k = 0; k < count; k++ )
                                {
                                    string name = fields[k]
                                                 .Name.Value;
                                    if ( name != "_time" && name.Contains( "_qc" ) == false )
                                    {
                                        ProcessDataPoint dp1 = new ProcessDataPoint();
                                        dp1.Index = fieldMetaData.Index;
                                        dp1.Name = fields[k]
                                                  .Name.Value;
                                        bs.Add( dp1 );
                                    }
                                }
                            }
                        }
                    }
                }
                catch ( Exception e )
                {
                    Console.WriteLine( e );
                    if ( Debugger.IsAttached )
                    {
                        Debugger.Break();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}