// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using opc.ua.pubsub.dotnet.binary.Messages.Meta.Structure;
using Newtonsoft.Json;

namespace opc.ua.pubsub.dotnet.binary.Messages.Meta
{
    public class MetaFrame : NetworkMessage, ICodable<MetaFrame>
    {
        public MetaFrame() : this( new EncodingOptions() ) { }

        public MetaFrame( EncodingOptions options )
        {
            Name                 = new String();
            Description          = new LocalizedText();
            ConfigurationVersion = new ConfigurationVersion();
            DiscoveryResponseHeader = new DiscoveryResponseHeader
                                      {
                                              ResponseType = 2
                                      };
        }

        public ConfigurationVersion                ConfigurationVersion    { get; set; }
        public Guid                                DataSetClassID          { get; set; }
        public ushort                              DataSetWriterID         { get; set; }
        public LocalizedText                       Description             { get; set; }
        public DiscoveryResponseHeader             DiscoveryResponseHeader { get; set; }
        public Dictionary<NodeID, EnumDescription> EnumDataTypes           { get; set; }
        public List<FieldMetaData>                 FieldMetaDataList       { get; set; }
        public String                              Name                    { get; set; }
        public List<String>                        Namespaces              { get; set; }
        public ushort                              SequenceNumber          { get; set; }
        public uint                                StatusCode              { get; set; }
        [JsonConverter( typeof(StructureDataTypeDictionaryJSONConverter) )]
        public Dictionary<NodeID, StructureDescription> StructureDataTypes { get; set; }

        public override void Encode( Stream outputStream, bool withHeader = true )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }

            if ( withHeader )
            {
                NetworkMessageHeader.Encode( outputStream );
            }

            EncodeChunk( outputStream );
        }

        public static MetaFrame Decode( Stream inputStream, EncodingOptions options )
        {
            MetaFrame instance = new MetaFrame();
            if ( inputStream == null )
            {
                return null;
            }
            instance.DiscoveryResponseHeader = DiscoveryResponseHeader.Decode( inputStream );
            bool chunkResult = DeocdeChunk( inputStream, options, ref instance );
            if ( chunkResult )
            {
                return instance;
            }
            return null;
        }

        /// <summary>
        ///     Encodes just the DataSetMetaData message to the given Stream.
        ///     NetworkMessage Header and DiscoveryResponse are not encoded.
        /// </summary>
        /// <param name="outputStream"></param>
        public void EncodeChunk( Stream outputStream )
        {
            DiscoveryResponseHeader.Encode( outputStream );

            // DataSetWriterID
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( DataSetWriterID ) );
            EncodeDataTypeSchemaHeaderStructure( outputStream );

            // Name
            Name.Encode( outputStream );

            // Description
            Description.Encode( outputStream );

            // Fields
            int fieldLength = -1;
            if ( FieldMetaDataList != null )
            {
                fieldLength = FieldMetaDataList.Count;
            }
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( fieldLength ) );
            for ( int i = 0; i < fieldLength; i++ )
            {
                // ReSharper disable once PossibleNullReferenceException
                FieldMetaData field = FieldMetaDataList[i] ?? new FieldMetaData( Options );
                field.Encode( outputStream );
            }

            // DataSet Class ID
            BaseType.WriteToStream( outputStream, DataSetClassID.ToByteArray() );

            // Version
            ConfigurationVersion.Encode( outputStream );

            // Status Code
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( StatusCode ) );
        }

        #region Overrides of Object

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine( "-----------------------------" );
            sb.AppendLine( "\tMeta Message" );
            sb.AppendLine( "-----------------------------" );
            sb.AppendLine( System.Globalization.CultureInfo.InvariantCulture, $"Name:\t\t\t{Name}" );
            sb.AppendLine( System.Globalization.CultureInfo.InvariantCulture, $"Description:\t\t{Description}" );
            int fieldLength = -1;
            if ( FieldMetaDataList != null )
            {
                fieldLength = FieldMetaDataList.Count;
            }
            sb.AppendLine( System.Globalization.CultureInfo.InvariantCulture, $"FieldMetaData:\t\t{fieldLength}" );
            sb.AppendLine( "--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------"
                         );
            sb.AppendLine( System.Globalization.CultureInfo.InvariantCulture, $"{"Index",10} | {"Name",-50} | {"Description",-30} | {"Flags",-25} | {"Type",-20} | {"DataType",-15} | {"ValueRank",-12} | {"ArrayDim",-10} | {"Size",-5} | {"FieldID",-38} | {"KeyValuePair",20}"
                         );
            sb.AppendLine( "--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------"
                         );
            for ( int i = 0; i < fieldLength; i++ )
            {
                FieldMetaData item = FieldMetaDataList[i];
                sb.AppendLine( System.Globalization.CultureInfo.InvariantCulture, $"{i,10} | {item}" );
            }
            int nameSpaceLength = -1;
            if ( Namespaces != null )
            {
                nameSpaceLength = Namespaces.Count;
            }
            sb.AppendLine( System.Globalization.CultureInfo.InvariantCulture, $"Namespaces:\t\t{nameSpaceLength}" );
            if ( Namespaces != null )
            {
                for ( int i = 0; i < nameSpaceLength; i++ )
                {
                    sb.AppendLine( System.Globalization.CultureInfo.InvariantCulture, $"Namespace [{i}]: {Namespaces[i]?.ToString() ?? "null"}" );
                }
            }
            int structureLength = -1;
            if ( StructureDataTypes != null )
            {
                structureLength = StructureDataTypes.Count;
            }
            sb.AppendLine( System.Globalization.CultureInfo.InvariantCulture, $"StructureDataTypes:\t\t{structureLength}" );
            if ( StructureDataTypes != null )
            {
                List<StructureDescription> structureDescriptions = StructureDataTypes.Values.ToList();
                for ( int i = 0; i < structureLength; i++ )
                {
                    sb.AppendLine( System.Globalization.CultureInfo.InvariantCulture, $"StructureDescription [{i}]:" );
                    sb.AppendLine( structureDescriptions[i]
                                         ?.ToConsoleString()
                                ?? "null"
                                 );
                }
            }
            if ( EnumDataTypes != null )
            {
                sb.AppendLine( System.Globalization.CultureInfo.InvariantCulture, $"EnumDataTypes:\t\t{EnumDataTypes.Count}" );
                foreach ( KeyValuePair<NodeID, EnumDescription> pair in EnumDataTypes )
                {
                    sb.AppendLine( System.Globalization.CultureInfo.InvariantCulture, $"EnumDescription [{pair.Key}]:" );
                    sb.AppendLine( System.Globalization.CultureInfo.InvariantCulture, $"{pair.Value}" );
                }
            }
            sb.AppendLine( System.Globalization.CultureInfo.InvariantCulture, $"ConfigurationVersion:\t{ConfigurationVersion}" );
            sb.AppendLine();
            sb.AppendLine();
            return sb.ToString();
        }

        #endregion

        /// <summary>
        ///     Decodes just the DataSetMetaData Message omitting the DiscoveryResponse Header.
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        protected static bool DeocdeChunk( Stream inputStream, EncodingOptions options, ref MetaFrame instance )
        {
            instance.DataSetWriterID = BaseType.ReadUInt16( inputStream )
                                               .Value;
            instance.Namespaces         = ParseNamespaceArray( inputStream );
            instance.StructureDataTypes = ParseStructureDescriptions( inputStream );

            // if (instance.StructureDataTypes != null)
            //    DataPointsManager.UpdateStructureDescription(instance.StructureDataTypes);
            instance.EnumDataTypes = ParseEnumDescriptions( inputStream );

            // Simple Data Types are currently not supported
            int? arrayLength = BaseType.ReadInt32( inputStream );
            if ( arrayLength > 0 )
            {
                // Simple Data Types are not supported
                // ToDo: Log error
                return false;
            }
            instance.Name              = String.Decode( inputStream );
            instance.Description       = LocalizedText.Decode( inputStream );
            instance.FieldMetaDataList = ParseFieldMetaDataArray( inputStream, options );
            byte[] guidAsByte = Common.ReadBytes( inputStream, 16 );
            if ( guidAsByte != null && guidAsByte.Length == 16 )
            {
                instance.DataSetClassID = new Guid( guidAsByte );
            }
            instance.ConfigurationVersion = ConfigurationVersion.Decode( inputStream );
            uint? readValue = BaseType.ReadUInt32( inputStream );
            if ( readValue != null )
            {
                instance.StatusCode = readValue.Value;
            }
            else
            {
                return false;
            }
            return true;
        }

        private void EncodeDataTypeSchemaHeaderStructure( Stream outputStream )
        {
            // Namespaces
            int nameSpaceLength = -1;
            if ( Namespaces != null )
            {
                nameSpaceLength = Namespaces.Count;
            }
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( nameSpaceLength ) );
            for ( int i = 0; i < nameSpaceLength; i++ )
            {
                String nameSpace = Namespaces[i] ?? new String();
                nameSpace.Encode( outputStream );
            }

            // Structures
            int structureLength = -1;
            if ( StructureDataTypes != null )
            {
                structureLength = StructureDataTypes.Count;
            }
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( structureLength ) );
            if ( StructureDataTypes != null )
            {
                List<StructureDescription> structureDescriptions = StructureDataTypes.Values.ToList();
                for ( int i = 0; i < structureLength; i++ )
                {
                    StructureDescription desc = structureDescriptions[i] ?? new StructureDescription();
                    desc.Encode( outputStream );
                }
            }

            // Enums
            int enumLength = -1;
            if ( EnumDataTypes != null )
            {
                enumLength = EnumDataTypes.Count;
            }
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( enumLength ) );
            if ( EnumDataTypes != null )
            {
                List<EnumDescription> enumDescriptions = EnumDataTypes.Values.ToList();
                for ( int i = 0; i < enumLength; i++ )
                {
                    EnumDescription desc = enumDescriptions[i] ?? new EnumDescription();
                    desc.Encode( outputStream );
                }
            }

            // Simple Data Types
            int simpleTypesArraySize = -1;
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( simpleTypesArraySize ) );
        }

        private static Dictionary<NodeID, EnumDescription> ParseEnumDescriptions( Stream inputStream )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return null;
            }
            int? length = BaseType.ReadInt32( inputStream );
            if ( !length.HasValue )
            {
                return null;
            }
            if ( length < 0 )
            {
                return null;
            }
            Dictionary<NodeID, EnumDescription> dictionary = new Dictionary<NodeID, EnumDescription>( length.Value );
            for ( int i = 0; i < length; i++ )
            {
                EnumDescription desc = EnumDescription.Decode( inputStream );
                dictionary[desc.DataTypeID] = desc;
            }
            return dictionary;
        }

        private static List<FieldMetaData> ParseFieldMetaDataArray( Stream inputStream, EncodingOptions options )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return null;
            }
            int? arraySize = BaseType.ReadInt32( inputStream );
            if ( !arraySize.HasValue )
            {
                return null;
            }

            // check whether FieldMetaData exist
            if ( arraySize.Value <= 0 )
            {
                // no FieldMetaData
                return null;
            }

            List<FieldMetaData> fieldMetaDataList = new List<FieldMetaData>( arraySize.Value );
            for ( int i = 0; i < arraySize.Value; i++ )
            {
                FieldMetaData item = FieldMetaData.Decode( inputStream, options );
                item.Index = i;
                fieldMetaDataList.Add( item );
            }
            return fieldMetaDataList;
        }

        private static List<String> ParseNamespaceArray( Stream inputStream )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return null;
            }
            int? length = BaseType.ReadInt32( inputStream );
            if ( !length.HasValue )
            {
                return null;
            }

            // check whether namespaces exist
            if ( length.Value <= 0 )
            {
                // no namespaces
                return null;
            }

            List<String> resultList = new List<String>( length.Value );
            for ( int i = 0; i < length.Value; i++ )
            {
                resultList.Add( String.Decode( inputStream ) );
            }
            return resultList;
        }

        private static Dictionary<NodeID, StructureDescription> ParseStructureDescriptions( Stream inputStream )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return null;
            }
            int? length = BaseType.ReadInt32( inputStream );
            if ( !length.HasValue )
            {
                return null;
            }

            // check whether StructureDescriptions exist
            if ( length.Value < 0 )
            {
                // no StructureDescriptions
                return null;
            }

            Dictionary<NodeID, StructureDescription> dictionary = new Dictionary<NodeID, StructureDescription>( length.Value );
            for ( int i = 0; i < length.Value; i++ )
            {
                StructureDescription desc = StructureDescription.Decode( inputStream );
                dictionary[desc.DataTypeId] = desc;
            }
            return dictionary;
        }
    }
}