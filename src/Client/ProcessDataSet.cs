// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using opc.ua.pubsub.dotnet.binary;
using opc.ua.pubsub.dotnet.binary.DataPoints;
using opc.ua.pubsub.dotnet.binary.Header;
using opc.ua.pubsub.dotnet.binary.Messages;
using opc.ua.pubsub.dotnet.binary.Messages.Chunk;
using opc.ua.pubsub.dotnet.binary.Messages.Delta;
using opc.ua.pubsub.dotnet.binary.Messages.KeepAlive;
using opc.ua.pubsub.dotnet.binary.Messages.Key;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using opc.ua.pubsub.dotnet.binary.Messages.Meta.Structure;
using String = opc.ua.pubsub.dotnet.binary.String;

[assembly: InternalsVisibleTo( "Client" )]

namespace opc.ua.pubsub.dotnet.client
{
    public class ProcessDataSet
    {
        public enum DataSetType
        {
            TimeSeries          = 0,
            Event               = 1,
            TimeSeriesEventFile = 2
        }

        private readonly string                             m_Name;
        private readonly Dictionary<string, DataPointEntry> m_ProcessValues;
        private readonly ushort                             m_WriterId;
        private          DataSetType                        m_DataSetType;
        private          MetaFrame                          m_MetaFrame;

        public ProcessDataSet( string publisherId, string name, ushort writerId, DataSetType dataSetType )
        {
            PublisherId     = publisherId;
            m_ProcessValues = new Dictionary<string, DataPointEntry>();
            m_Name          = name;
            m_WriterId      = writerId;
            m_MetaFrame     = null;
            SetDataSetType( dataSetType );
            Description = new LocalizedText();
        }

        public LocalizedText Description { get; set; }

        public bool MetaDataUpToDate
        {
            get
            {
                return m_MetaFrame != null;
            }
        }

        public int ProcessValueCount
        {
            get
            {
                return m_ProcessValues.Count;
            }
        }

        public string PublisherId { get; internal set; }

        public void AddDataPoint( ProcessDataPointValue dataPoint )
        {
            if ( !m_ProcessValues.ContainsKey( dataPoint.Name ) )
            {
                DataPointEntry entry = new DataPointEntry
                                       {
                                               DataPoint  = dataPoint,
                                               IsModified = true
                                       };
                m_ProcessValues.Add( dataPoint.Name, entry );
                m_MetaFrame = null;
            }
        }

        public DataFrame GenerateDateFrame()
        {
            CreateMeta( m_Name, new EncodingOptions(), 1 );
            return GetKeyFrame( 2 );
        }

        public IList<ProcessDataPointValue> GetAllDataPointValues()
        {
            return m_ProcessValues.Values.Select( v => v.DataPoint )
                                  .ToList();
        }

        public List<byte[]> GetChunkedKeyFrame( uint chunkSize, ushort sequenceNumber )
        {
            KeyFrame key = GetKeyFrame( sequenceNumber );
            return GetChunkedFrame( chunkSize, key );
        }

        public List<byte[]> GetChunkedMetaFrame( uint chunkSize, EncodingOptions options, ushort sequenceNumber )
        {
            byte[]       rawMessage = GetEncodedMetaFrame( options, sequenceNumber, false );
            List<byte[]> rawChunks  = new List<byte[]>();
            if ( chunkSize == 0 )
            {
                using ( MemoryStream completeOutputStream = new MemoryStream() )
                {
                    m_MetaFrame.NetworkMessageHeader.Encode( completeOutputStream );
                    completeOutputStream.Write( rawMessage, 0, rawMessage.Length );
                    rawChunks.Add( completeOutputStream.ToArray() );
                }
            }
            else
            {
                for ( uint i = 0; i < rawMessage.LongLength; i += chunkSize )
                {
                    NetworkMessageHeader networkHeader  = GetChunkedNetworkHeader( true );
                    ChunkedMessage       chunkedMessage = new ChunkedMessage();
                    chunkedMessage.PayloadHeader = new ChunkedPayloadHeader();

                    //chunkedMessage.PayloadHeader.DataSetWriterID = m_MetaFrame.DataSetWriterID;
                    chunkedMessage.NetworkMessageHeader  = networkHeader;
                    chunkedMessage.TotalSize             = (uint)rawMessage.LongLength;
                    chunkedMessage.ChunkOffset           = i;
                    chunkedMessage.MessageSequenceNumber = m_MetaFrame.SequenceNumber;

                    // Check if can copy a "full" chunk or just the remaining elements of the array.
                    long length = Math.Min( chunkSize, rawMessage.LongLength - chunkedMessage.ChunkOffset );
                    chunkedMessage.ChunkData = new byte[length];
                    Array.Copy( rawMessage, i, chunkedMessage.ChunkData, 0, length );
                    using ( MemoryStream stream = new MemoryStream() )
                    {
                        chunkedMessage.Encode( stream );
                        rawChunks.Add( stream.ToArray() );
                    }
                }
            }
            return rawChunks;
        }

        public ProcessDataPointValue GetDataPointValue( string name )
        {
            if ( !m_ProcessValues.ContainsKey( name ) )
            {
                return null;
            }
            return m_ProcessValues[name]
                   .DataPoint;
        }

        public DataSetType GetDataSetType()
        {
            return m_DataSetType;
        }

        public byte[] GetEncodedDeltaFrame( ushort sequenceNumber )
        {
            DeltaFrame delta = new DeltaFrame();
            delta.ConfigurationVersion = m_MetaFrame.ConfigurationVersion;
            delta.MetaFrame            = m_MetaFrame;
            delta.NetworkMessageHeader = new NetworkMessageHeader
                                         {
                                                 PublisherID     = new String( PublisherId ),
                                                 VersionAndFlags = 0xD1,
                                                 ExtendedFlags1 = new ExtendedFlags1
                                                                  {
                                                                          RawValue = 0x04
                                                                  }
                                         };
            delta.Flags1 = new DataSetFlags1
                           {
                                   RawValue = 0xEB
                           };
            delta.Flags2 = new DataSetFlags2
                           {
                                   RawValue = 0x11
                           };
            delta.FieldIndexList = new List<ushort>();
            delta.PayloadHeader = new DataSetPayloadHeader
                                  {
                                          Count           = 1,
                                          DataSetWriterID = new[] { m_MetaFrame.DataSetWriterID }
                                  };
            delta.Items          = new List<DataPointValue>();
            delta.FieldIndexList = new List<ushort>();
            for ( int i = 0; i < m_ProcessValues.Values.Count; i++ )
            {
                if ( m_ProcessValues.Values.ElementAt( i )
                                    .IsModified )
                {
                    DataPointValue dataPoint = m_ProcessValues.Values.ElementAt( i )
                                                              .DataPoint;
                    dataPoint.Index = i;
                    delta.Items.Add( dataPoint );
                    m_ProcessValues.Values.ElementAt( i )
                                   .IsModified = false;
                    delta.FieldIndexList.Add( (ushort)i );
                }
            }
            delta.FieldCount                   = (ushort)delta.Items.Count;
            delta.DataSetMessageSequenceNumber = sequenceNumber;
            delta.Timestamp                    = DateTime.Now;
            using ( MemoryStream outputStream = new MemoryStream() )
            {
                delta.Encode( outputStream );
                return outputStream.ToArray();
            }
        }

        public byte[] GetEncodedKeepAliveMessage( ushort sequenceNumber )
        {
            KeepAliveFrame keepAliveMessage = CreateKeepAliveFrame( sequenceNumber );
            if ( keepAliveMessage == null )
            {
                return null;
            }
            byte[] rawData;
            using ( MemoryStream stream = new MemoryStream() )
            {
                keepAliveMessage.Encode( stream );
                rawData = stream.ToArray();
                return rawData;
            }
        }

        public byte[] GetEncodedKeyFrame( ushort sequenceNumber )
        {
            KeyFrame key = GetKeyFrame( sequenceNumber );
            using ( MemoryStream outputStream = new MemoryStream() )
            {
                key.Encode( outputStream );
                return outputStream.ToArray();
            }
        }

        public byte[] GetEncodedMetaFrame( EncodingOptions options, ushort sequenceNumber, bool withHeader )
        {
            if ( m_MetaFrame == null )
            {
                CreateMeta( m_Name, options, sequenceNumber );
            }
            byte[] rawMessage = null;
            using ( MemoryStream outputStream = new MemoryStream() )
            {
                m_MetaFrame.Encode( outputStream, withHeader );
                rawMessage = outputStream.ToArray();
            }
            return rawMessage;
        }

        public ushort GetWriterId()
        {
            return m_WriterId;
        }

        public void SetDataPointModified( string name )
        {
            m_ProcessValues[name]
                   .IsModified = true;
        }

        public void UpdateDataPoint( ProcessDataPointValue dataPoint )
        {
            DataPointValue baseValue = GetDataPointValue( dataPoint.Name );
            if ( baseValue == null )
            {
                AddDataPoint( dataPoint );
            }
            else
            {
                ( baseValue as ProcessDataPointValue )?.Update( dataPoint );
                SetDataPointModified( baseValue.Name );
            }
        }

        protected void AddDataPointMeta( DataPointValue dataPoint )
        {
            if ( m_MetaFrame.FieldMetaDataList == null )
            {
                m_MetaFrame.FieldMetaDataList = new List<FieldMetaData>();
            }
            if ( m_MetaFrame.StructureDataTypes == null )
            {
                m_MetaFrame.StructureDataTypes = new Dictionary<NodeID, StructureDescription>();
            }
            if ( m_MetaFrame.EnumDataTypes == null )
            {
                m_MetaFrame.EnumDataTypes = new Dictionary<NodeID, EnumDescription>();
            }
            if ( m_MetaFrame.Namespaces == null )
            {
                m_MetaFrame.Namespaces = new List<String>( 3 );
                m_MetaFrame.Namespaces.Add( new String() );
                m_MetaFrame.Namespaces.Add( new String( "http://siemens.com/energy/schema/opcua/ps/v2" ) );
                m_MetaFrame.Namespaces.Add( new String( "https://mindsphere.io/OPCUAPubSub/v3" ) );
            }
            int           index = m_MetaFrame.FieldMetaDataList.FindIndex( x => x.DataType == dataPoint.NodeID );
            FieldMetaData field = m_MetaFrame.FieldMetaDataList[index];

            //string        datatype = ProcessValueFactory.ConvertNodeIDSToString( dataPoint.NodeID );

            //if ( datatype == "EnumValue" )
            //{
            //    StructureDescription desc = IntegerValue.IntegerValueStructureDescription;
            //    desc.DataTypeId                                = dataPoint.NodeID;
            //    desc.Name.Name.Value                           = "EnumValue_" + dataPoint.EnumDescription.Name.Name.Value;
            //    m_MetaFrame.StructureDataTypes[field.DataType] = desc;

            //    //DataPointsManager.StructureDescriptions[field.DataType] = desc;
            //    EnumDescription enumDescription = dataPoint.EnumDescription;
            //    /*
            //    * This is the fix for defect 
            //    * #15 - StructureDataType has same NodeId value with EnumDataType
            //    * Need to validate this fix  to figure out any other better fix available.
            //    */
            //    NodeID Id = new NodeID
            //                {
            //                        Namespace = 1,
            //                        Value     = (ushort) ( 100 + m_MetaFrame.EnumDataTypes.Count )
            //                };
            //    enumDescription.DataTypeID = Id;

            //    if ( !m_MetaFrame.EnumDataTypes.ContainsKey( enumDescription.DataTypeID ) )
            //    {
            //        m_MetaFrame.EnumDataTypes.Add( enumDescription.DataTypeID, enumDescription );
            //    }
            //}
            //else
            //{
            m_MetaFrame.StructureDataTypes[field.DataType] = dataPoint.StructureDescription;

            //}
        }

        protected void CreateMeta( string name, EncodingOptions options, ushort sequenceNumber )
        {
            m_MetaFrame = new MetaFrame( options );
            ExtendedFlags1 extendedFlags1 = new ExtendedFlags1
                                            {
                                                    RawValue = 0x84
                                            };
            ExtendedFlags2 extendedFlags2 = new ExtendedFlags2
                                            {
                                                    RawValue = 8
                                            };
            NetworkMessageHeader networkMessageHeader = new NetworkMessageHeader
                                                        {
                                                                VersionAndFlags = 0xD1,
                                                                ExtendedFlags1  = extendedFlags1,
                                                                ExtendedFlags2  = extendedFlags2,
                                                                PublisherID     = new String( PublisherId )
                                                        };
            m_MetaFrame.NetworkMessageHeader = networkMessageHeader;
            m_MetaFrame.SequenceNumber       = sequenceNumber;
            DateTime time = DateTime.UtcNow;
            m_MetaFrame.ConfigurationVersion.Minor = (uint)( time.Ticks & uint.MaxValue );
            m_MetaFrame.ConfigurationVersion.Major = (uint)( time.Ticks >> 32 );
            m_MetaFrame.Name                       = new String( name );
            m_MetaFrame.DataSetWriterID            = m_WriterId;
            m_MetaFrame.Description                = Description;
            foreach ( DataPointEntry entry in m_ProcessValues.Values )
            {
                CreateFieldMetaDataList( entry, m_MetaFrame, options );
                AddDataPointMeta( entry.DataPoint );
            }
        }

        protected NetworkMessageHeader GetChunkedNetworkHeader( bool isMetaMessage = false )
        {
            byte extendedFlags2 = 0x01;
            if ( isMetaMessage )
            {
                extendedFlags2 = 0x09;
            }
            NetworkMessageHeader networkHeader = new NetworkMessageHeader();
            networkHeader.PublisherID     = new String( PublisherId );
            networkHeader.VersionAndFlags = 0xD1;
            networkHeader.ExtendedFlags1 = new ExtendedFlags1
                                           {
                                                   RawValue = 0x84
                                           };
            networkHeader.ExtendedFlags2 = new ExtendedFlags2
                                           {
                                                   RawValue = extendedFlags2
                                           };
            return networkHeader;
        }

        protected KeyFrame GetKeyFrame( ushort sequenceNumber )
        {
            KeyFrame key = new KeyFrame();
            key.ConfigurationVersion = m_MetaFrame.ConfigurationVersion;
            key.NetworkMessageHeader = new NetworkMessageHeader
                                       {
                                               PublisherID     = new String( PublisherId ),
                                               VersionAndFlags = 0xD1,
                                               ExtendedFlags1 = new ExtendedFlags1
                                                                {
                                                                        RawValue = 0x04
                                                                }
                                       };
            key.Flags1.RawValue = 0xEB;
            key.Flags2.RawValue = 0x10;
            key.MetaFrame       = m_MetaFrame;
            key.Items           = new List<DataPointValue>();
            for ( int i = 0; i < m_ProcessValues.Values.Count; i++ )
            {
                DataPointValue dataPoint = m_ProcessValues.Values.ElementAt( i )
                                                          .DataPoint;
                dataPoint.Index = i;
                key.Items.Add( dataPoint );
                m_ProcessValues.Values.ElementAt( i )
                               .IsModified = false;
            }
            key.PayloadHeader = new DataSetPayloadHeader
                                {
                                        Count           = 1,
                                        DataSetWriterID = new[] { m_MetaFrame.DataSetWriterID }
                                };
            key.DataSetMessageSequenceNumber = sequenceNumber;
            key.Timestamp                    = DateTime.Now;
            return key;
        }

        private static void CreateFieldMetaDataList( DataPointEntry entry, MetaFrame m_MetaFrame, EncodingOptions options )
        {
            if ( m_MetaFrame.FieldMetaDataList == null )
            {
                m_MetaFrame.FieldMetaDataList = new List<FieldMetaData>();
            }
            FieldMetaData fieldMetaData = new FieldMetaData ( options )
                                          {
                                                  Index    = entry.DataPoint.Index,
                                                  Name     = new String( entry.DataPoint.Name ),
                                                  Flags    = new DataSetFieldFlags( options ),
                                                  DataType = entry.DataPoint.NodeID,
                                                  FieldID  = entry.DataPoint.FieldID
                                          };

            // all data points have a "Unit" and a "Prefix" key/value pair in the Properties
            // list of the FieldMetaData; if it is null here, then create an empty entry

            // create Properties object and add key/value pair for "Prefix"
            if ( entry.DataPoint.Prefix == null )
            {
                fieldMetaData.Properties = new List<KeyValuePair>
                                           {
                                                   new KeyValuePair( "Prefix", new String( "" ) )
                                           };
            }
            else
            {
                fieldMetaData.Properties = new List<KeyValuePair>
                                           {
                                                   new KeyValuePair( "Prefix", new String( entry.DataPoint.Prefix ) )
                                           };
            }

            // add key/value pair for "Unit"
            if ( entry.DataPoint.Prefix == null )
            {
                fieldMetaData.Properties.Add( new KeyValuePair( "Unit", new String( "" ) ) );
            }
            else
            {
                fieldMetaData.Properties.Add( new KeyValuePair( "Unit", new String( entry.DataPoint.Unit ) ) );
            }

            // add the FieldMetaData for the given data object to the list of all FieldMetaData
            m_MetaFrame.FieldMetaDataList.Add( fieldMetaData );
        }

        private KeepAliveFrame CreateKeepAliveFrame( ushort sequenceNumber )
        {
            KeepAliveFrame keepAliveMessage = new KeepAliveFrame();
            keepAliveMessage.ConfigurationVersion                 = m_MetaFrame.ConfigurationVersion;
            keepAliveMessage.NetworkMessageHeader                 = new NetworkMessageHeader();
            keepAliveMessage.NetworkMessageHeader.PublisherID     = new String( PublisherId );
            keepAliveMessage.NetworkMessageHeader.VersionAndFlags = 0xD1;
            keepAliveMessage.PayloadHeader = new DataSetPayloadHeader
                                             {
                                                     Count           = 1,
                                                     DataSetWriterID = new[] { m_WriterId }
                                             };
            keepAliveMessage.Flags1                       = new DataSetFlags1();
            keepAliveMessage.Flags1.RawValue              = 0x8B;
            keepAliveMessage.Flags2                       = new DataSetFlags2();
            keepAliveMessage.Flags2.RawValue              = 0x03;
            keepAliveMessage.DataSetMessageSequenceNumber = sequenceNumber;
            return keepAliveMessage;
        }

        private List<byte[]> GetChunkedFrame( uint chunkSize, DataFrame dataFrame )
        {
            byte[] rawData;
            using ( MemoryStream stream = new MemoryStream() )
            {
                dataFrame.EncodeChunk( stream );
                rawData = stream.ToArray();
            }
            List<byte[]> rawChunks = new List<byte[]>();
            if ( chunkSize == 0 )
            {
                using ( MemoryStream completeOutputStream = new MemoryStream() )
                {
                    dataFrame.NetworkMessageHeader.Encode( completeOutputStream );
                    dataFrame.PayloadHeader.Encode( completeOutputStream );
                    completeOutputStream.Write( rawData, 0, rawData.Length );
                    rawChunks.Add( completeOutputStream.ToArray() );
                }
            }
            else
            {
                for ( uint i = 0; i < rawData.LongLength; i += chunkSize )
                {
                    NetworkMessageHeader networkHeader  = GetChunkedNetworkHeader();
                    ChunkedMessage       chunkedMessage = new ChunkedMessage();
                    chunkedMessage.PayloadHeader                 = new ChunkedPayloadHeader();
                    chunkedMessage.PayloadHeader.DataSetWriterID = dataFrame.PayloadHeader.DataSetWriterID[0];
                    chunkedMessage.NetworkMessageHeader          = networkHeader;
                    chunkedMessage.TotalSize                     = (uint)rawData.LongLength;
                    chunkedMessage.ChunkOffset                   = i;
                    chunkedMessage.MessageSequenceNumber         = dataFrame.DataSetMessageSequenceNumber;

                    // Check if can copy a "full" chunk or just the remaining elements of the array.
                    long length = Math.Min( chunkSize, rawData.LongLength - chunkedMessage.ChunkOffset );
                    chunkedMessage.ChunkData = new byte[length];
                    Array.Copy( rawData, i, chunkedMessage.ChunkData, 0, length );
                    using ( MemoryStream stream = new MemoryStream() )
                    {
                        chunkedMessage.Encode( stream );
                        rawChunks.Add( stream.ToArray() );
                    }
                }
            }
            return rawChunks;
        }

        private void SetDataSetType( DataSetType value )
        {
            m_DataSetType = value;
        }

        protected class DataPointEntry
        {
            public ProcessDataPointValue DataPoint  { get; set; }
            public bool                  IsModified { get; set; }
        }
    }
}