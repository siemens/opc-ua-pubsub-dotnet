// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Binary.DataPoints;
using Binary.Messages.Meta;
using Binary.Messages.Meta.Structure;
using log4net;
using File = Binary.DataPoints.File;

namespace Binary.Messages
{
    public class DataFrame : NetworkMessage
    {
        private static readonly ILog Logger = LogManager.GetLogger( MethodBase.GetCurrentMethod()
                                                                              .DeclaringType
                                                                  );
        public DataFrame() : this( new EncodingOptions() ) { }

        public DataFrame( EncodingOptions options )
        {
            Options       = options;
            Flags1        = new DataSetFlags1();
            Flags2        = new DataSetFlags2();
            PayloadHeader = new DataSetPayloadHeader();
        }

        public DataFrame( DataFrame dataFrame )
        {
            MetaFrame            = dataFrame.MetaFrame;
            NetworkMessageHeader = dataFrame.NetworkMessageHeader;
            PayloadHeader        = dataFrame.PayloadHeader;
            Flags1               = dataFrame.Flags1;
            Flags2               = dataFrame.Flags2;
            ConfigurationVersion = dataFrame.ConfigurationVersion;
        }

        public ConfigurationVersion ConfigurationVersion         { get; set; }
        public ushort               DataSetMessageSequenceNumber { get; set; }
        public DataSetFlags1        Flags1                       { get; set; }
        public DataSetFlags2        Flags2                       { get; set; }
        public List<DataPointValue> Items                        { get; set; }
        public MetaFrame            MetaFrame                    { get; set; }
        public DataSetPayloadHeader PayloadHeader                { get; set; }
        public DateTime             Timestamp                    { get; set; }

        public static DataFrame Decode( Stream inputStream )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return null;
            }
            DataFrame instance = new DataFrame();
            instance.PayloadHeader = DataSetPayloadHeader.Decode( inputStream );
            bool chunkResult = DecodeChunk( inputStream, ref instance );
            if ( chunkResult )
            {
                return instance;
            }
            return null;
        }

        public override void Encode( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }

            // 1. Network Message Header
            NetworkMessageHeader.Encode( outputStream );

            // 2. DataSet Payload Header
            PayloadHeader.Encode( outputStream );
            EncodeChunk( outputStream );
        }

        /// <summary>
        ///     Encodes just the common part of the DataSet Message (e.g. Flags 1 & Flags 2,
        ///     Configuration Version and Message Sequence Number.
        /// </summary>
        /// <param name="outputStream"></param>
        public virtual void EncodeChunk( Stream outputStream )
        {
            // 3. DataSetFlags1
            outputStream.WriteByte( Flags1.RawValue );

            // 4. DataSetFlags2
            if ( Flags1.Flags1.HasFlag( DataSetFlags1Enum.DataSetFlags2Enabled ) )
            {
                outputStream.WriteByte( Flags2.RawValue );
            }

            // 5. Message Sequence Number
            if ( Flags1.Flags1.HasFlag( DataSetFlags1Enum.DataSetSequenceNumberEnabled ) )
            {
                BaseType.WriteToStream( outputStream, BitConverter.GetBytes( DataSetMessageSequenceNumber ) );
            }

            // evaluate the following options only if DataSetFlags2 field is provided
            if ( Flags1.Flags1.HasFlag( DataSetFlags1Enum.DataSetFlags2Enabled ) )
            {
                // Timestamp
                if ( Flags2.Flags2.HasFlag( DataSetFlags2Enum.TimeStampEnabled ) )
                {
                    BaseType.WriteToStream( outputStream, BitConverter.GetBytes( Convert.ToInt64( Timestamp.ToFileTimeUtc() ) ) );
                }
            }

            // 6. Configuration Version
            if ( Flags1.Flags1.HasFlag( DataSetFlags1Enum.ConfigurationVersionMajorVersion )
              || Flags1.Flags1.HasFlag( DataSetFlags1Enum.ConfigurationVersionMinorVersion ) )
            {
                // we always send both, minor and major version; ensure that both
                // bits are set in DataSetFlags1
                ConfigurationVersion.Encode( outputStream );
            }
        }

        public static bool DecodeChunk( Stream inputStream, ref DataFrame instance )
        {
            // 3. DataSetFlags1
            instance.Flags1.RawValue = (byte)inputStream.ReadByte();

            // 4. DataSetFlags2
            if ( instance.Flags1.Flags1.HasFlag( DataSetFlags1Enum.DataSetFlags2Enabled ) )
            {
                instance.Flags2.RawValue = (byte)inputStream.ReadByte();
            }

            // 5. Message Sequence Number
            if ( instance.Flags1.Flags1.HasFlag( DataSetFlags1Enum.DataSetSequenceNumberEnabled ) )
            {
                ushort? sequence = BaseType.ReadUInt16( inputStream );
                if ( sequence != null )
                {
                    instance.DataSetMessageSequenceNumber = sequence.Value;
                }
                else
                {
                    // ToDo: Log error
                    return false;
                }
            }

            // evaluate the following options only if DataSetFlags2 field is provided
            if ( instance.Flags1.Flags1.HasFlag( DataSetFlags1Enum.DataSetFlags2Enabled ) )
            {
                // Timestamp
                if ( instance.Flags2.Flags2.HasFlag( DataSetFlags2Enum.TimeStampEnabled ) )
                {
                    long? timestamp = BaseType.ReadInt64( inputStream );
                    if ( timestamp != null )
                    {
                        instance.Timestamp = DateTime.FromFileTimeUtc( timestamp.Value );

                        //instance.Timestamp = DateTime.FromBinary(timestamp.Value);
                    }
                    else
                    {
                        // ToDo: Log error
                        return false;
                    }
                }
            }

            // 6. Major & Minor
            //
            // we always expect both, minor and major version; ensure that both
            // bits are set in DataSetFlags1
            //
            if ( instance.Flags1.Flags1.HasFlag( DataSetFlags1Enum.ConfigurationVersionMajorVersion ) )
            {
                if ( instance.Flags1.Flags1.HasFlag( DataSetFlags1Enum.ConfigurationVersionMinorVersion ) )
                {
                    // read minor and major version, must both exist
                    instance.ConfigurationVersion = ConfigurationVersion.Decode( inputStream );
                }
                else
                {
                    throw new Exception( "DataSetMessageHeader: Minor Configuration Version not present." );
                }
            }
            else
            {
                if ( instance.Flags1.Flags1.HasFlag( DataSetFlags1Enum.ConfigurationVersionMinorVersion ) )
                {
                    throw new Exception( "DataSetMessageHeader: Major Configuration Version not present." );
                }
            }
            return true;
        }

        protected static DataPointValue ParseDataPoint( Stream inputStream, MetaFrame meta, ushort index )
        {
            if ( index > meta.FieldMetaDataList.Count - 1 )
            {
                throw new Exception( "Decode DeltaFrame: Could not parse field as corresponding meta data is not present" );
            }

            // Value
            FieldMetaData metaData = meta.FieldMetaDataList[index];
            if ( metaData.Index != index )
            {
                Logger.Error( $"Index does not match: FieldMetaData.Index {metaData.Index} != {index} read from Stream." );
            }
            if ( Logger.IsDebugEnabled )
            {
                Logger.Debug( $"Decoding item at position {index} in Meta Frame. Using FieldMetaData:" );
                Logger.Debug( metaData );
            }
            DataPointValue item = null;
            if ( metaData.DataType == File.PreDefinedNodeID )
            {
                item = new File();
                item.Decode( inputStream );
            }
            else
            {
                item            = ProcessValueFactory.CreateValue( metaData.DataType );
                item.Properties = metaData.Properties;
                if ( meta.StructureDataTypes.TryGetValue( metaData.DataType, out StructureDescription desc ) )
                {
                    if ( meta.EnumDataTypes != null
                      && desc.Name.Name.ToString()
                             .Contains( "EnumValue" ) )
                    {
                        string enumName = desc.Name.Name.Value
                                              .Substring( 10 );
                        item.EnumDescription = meta.EnumDataTypes.First( s => s.Value.Name.Name.Value == enumName )
                                                   .Value;
                    }
                    item.Decode( inputStream );
                }
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if ( item == null )
            {
                Logger.Error( $"Unable to decode value {metaData.DataType} at position {index} in message." );
                return null;
            }
            item.Name       = metaData.Name.Value;
            item.Properties = metaData.Properties;
            item.Index      = index;
            return item;
        }

        protected static void WriteSingleDataPoint( Stream outputStream, DataPointValue dpv )
        {
            switch ( dpv )
            {
                case ProcessDataPointValue pdv:
                {
                    pdv.Encode( outputStream );
                    break;
                }

                case File file:
                    file.Encode( outputStream );
                    break;

                case null:
                    throw new NullReferenceException( "Empty DataPointValue." );

                default:
                    throw new Exception( "Unsupported DataType for encoding." );
            }
        }

        internal static StructureDescription GetStructureDescription( MetaFrame meta, NodeID nodeID )
        {
            foreach ( KeyValuePair<NodeID, StructureDescription> desc in meta.StructureDataTypes )
            {
                if ( desc.Key == nodeID )
                {
                    return desc.Value;
                }
            }
            return null;
        }
    }
}