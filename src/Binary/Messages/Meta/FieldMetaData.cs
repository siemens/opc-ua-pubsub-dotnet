// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Binary.Messages.Meta
{
    public class FieldMetaData : ICodable<FieldMetaData>
    {
        public FieldMetaData( EncodingOptions options )
        {
            Options         = options;
            Name            = new String();
            Description     = new LocalizedText();
            Flags           = new DataSetFieldFlags( options );
            Type            = BuiltinType.ExtensionObject;
            ValueRank       = -1;
            MaxStringLength = 0;
        }

        public uint[]             ArrayDimension  { get; set; }
        public NodeID             DataType        { get; set; }
        public LocalizedText      Description     { get; set; }
        public Guid               FieldID         { get; set; }
        public DataSetFieldFlags  Flags           { get; set; }
        public int                Index           { get; set; }
        public uint               MaxStringLength { get; set; }
        public String             Name            { get; set; }
        public List<KeyValuePair> Properties      { get; set; }
        [JsonConverter( typeof(StringEnumConverter) )]
        public BuiltinType Type { get; set; }
        public int ValueRank { get;    set; }

        public void Encode( Stream outputStream, bool withHeader = true)
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }

            // 1. Name
            Name.Encode( outputStream );

            // 2. Description
            Description.Encode( outputStream );

            // 3. Field Flags
            Flags.Encode( outputStream );

            // 4. Built-in Type
            outputStream.WriteByte( (byte)Type );

            // 5. DataType
            DataType.Encode( outputStream );

            // 6. Value Rank
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( ValueRank ) );

            // 7. Array Dimension
            SimpleArray<uint>.Encode( outputStream, ArrayDimension );

            // 8. MaxStringLength
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( MaxStringLength ) );

            // 9. FieldID
            BaseType.WriteToStream( outputStream, FieldID.ToByteArray() );

            // 10. Properties
            EncodeProperties( outputStream );
        }

        public EncodingOptions Options { get; }

        public static FieldMetaData Decode( Stream inputStream, EncodingOptions options )
        {
            FieldMetaData instance = new FieldMetaData( options );

            // 1. Name
            instance.Name = String.Decode( inputStream );

            // 2. Description
            instance.Description = LocalizedText.Decode( inputStream );

            // 3. Field Flags
            instance.Flags = DataSetFieldFlags.Decode( inputStream, options );

            // 4. Built-in Type
            instance.Type = (BuiltinType)inputStream.ReadByte();

            // 5. DataType
            instance.DataType = NodeID.Decode( inputStream );

            // 6. Value Rank
            int? valueRank = BaseType.ReadInt32( inputStream );
            if ( valueRank.HasValue )
            {
                instance.ValueRank = valueRank.Value;
            }

            // 7. Array Dimension
            instance.ArrayDimension = SimpleArray<uint>.Decode( inputStream, BaseType.ReadUInt32 );

            // 8. Size
            uint? readUInt32 = BaseType.ReadUInt32( inputStream );
            if ( readUInt32 != null )
            {
                instance.MaxStringLength = readUInt32.Value;
            }

            // 9. Field ID
            byte[] guidAsByte = Common.ReadBytes( inputStream, 16 );
            if ( guidAsByte != null && guidAsByte.Length == 16 )
            {
                instance.FieldID = new Guid( guidAsByte );
            }

            // 10. Properties
            instance.Properties = ParseProperties( inputStream );
            return instance;
        }

        #region Overrides of Object

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if ( Properties != null )
            {
                for ( int i = 0; i < Properties.Count; i++ )
                {
                    if ( i > 0 )
                    {
                        sb.Append( "; " );
                    }
                    KeyValuePair pair = Properties[i];
                    if ( pair != null )
                    {
                        if ( pair.Name != null ) { }
                        else
                        {
                            sb.Append( "NULL=" );
                        }
                        if ( pair.Value != null )
                        {
                            sb.Append( $"{pair.Value}" );
                        }
                        else
                        {
                            sb.Append( "NULL" );
                        }
                    }
                    else
                    {
                        sb.Append( "NULL" );
                    }
                }
            }
            return
                    $"{Name,-50} | {Description,-30} | {Flags,25} | {Type,20} | {DataType,15} | {ValueRank,12} | {ArrayDimension,10} | {MaxStringLength,5} | {FieldID,38} | {sb,-20}";
        }

        #endregion

        private void EncodeProperties( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            int length = -1;
            if ( Properties != null )
            {
                length = Properties.Count;
            }
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( length ) );
            for ( int i = 0; i < length; i++ )
            {
                // ReSharper disable once PossibleNullReferenceException
                KeyValuePair item = Properties[i];
                item.Encode( outputStream );
            }
        }

        private static List<KeyValuePair> ParseProperties( Stream inputStream )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return null;
            }
            int? arrayLength = BaseType.ReadInt32( inputStream );
            if ( !arrayLength.HasValue )
            {
                return null;
            }
            if ( arrayLength.Value < 0 )
            {
                return null;
            }
            List<KeyValuePair> properties = new List<KeyValuePair>( arrayLength.Value );
            for ( int i = 0; i < arrayLength.Value; i++ )
            {
                KeyValuePair item = KeyValuePair.Decode( inputStream );
                properties.Add( item );
            }
            return properties;
        }
    }
}