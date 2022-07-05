// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace opc.ua.pubsub.dotnet.binary
{
    public class Variant
    {
        public byte          EncodingMask            { get; set; }
        public bool          IsArrayDimensionEncoded { get; set; }
        public bool          IsArrayEncoded          { get; set; }
        public FieldMetaData MetaData                { get; set; }
        [JsonConverter( typeof(StringEnumConverter) )]
        public BuiltinType Type { get; set; }
        public object Value { get;     set; }

        public static Variant Decode( Stream inputStream )
        {
            Variant instance = new Variant();
            instance.EncodingMask = (byte)inputStream.ReadByte();

            // Error: Dimension flag is set, but there's no array flag.
            instance.IsArrayDimensionEncoded = ( instance.EncodingMask & ( 1 << 6 ) ) != 0;
            instance.IsArrayEncoded          = ( instance.EncodingMask & ( 1 << 7 ) ) != 0;
            byte type = (byte)( instance.EncodingMask & ~( 1 << 6 ) );
            type          &= (byte)( type & ~( 1 << 7 ) );
            instance.Type =  (BuiltinType)type;

            // ToDo: Workaround
            if ( instance.IsArrayEncoded || instance.IsArrayDimensionEncoded )
            {
                instance.Value = ReadArray( inputStream, instance.Type );
            }
            else
            {
                instance.Value = ReadValue( inputStream, instance.Type );
            }
            return instance;
        }

        private static object ReadArray( Stream inputStream, BuiltinType type )
        {
            uint? arrayLength = BaseType.ReadUInt32( inputStream );
            if ( !arrayLength.HasValue )
            {
                return null;
            }
            switch ( type )
            {
                case BuiltinType.Variant:
                    return ReadVariantArray( inputStream, arrayLength.Value );

                case BuiltinType.Byte:
                    return new List<byte>( SimpleArray<byte>.Decode( inputStream, BaseType.ReadByte ) );

                case BuiltinType.UInt16:
                    return new List<ushort>( SimpleArray<ushort>.Decode( inputStream, BaseType.ReadUInt16 ) );

                case BuiltinType.Int16:
                    return new List<short>( SimpleArray<short>.Decode( inputStream, BaseType.ReadInt16 ) );

                case BuiltinType.UInt32:
                    return new List<uint>( SimpleArray<uint>.Decode( inputStream, BaseType.ReadUInt32 ) );

                case BuiltinType.Int32:
                    return new List<int>( SimpleArray<int>.Decode( inputStream, BaseType.ReadInt32 ) );

                case BuiltinType.UInt64:
                    return new List<ulong>( SimpleArray<ulong>.Decode( inputStream, BaseType.ReadUInt64 ) );

                case BuiltinType.Int64:
                    return new List<long>( SimpleArray<long>.Decode( inputStream, BaseType.ReadInt64 ) );

                case BuiltinType.Float:
                    return new List<float>( SimpleArray<float>.Decode( inputStream, BaseType.ReadFloat ) );

                case BuiltinType.Double:
                    return new List<double>( SimpleArray<double>.Decode( inputStream, BaseType.ReadDouble ) );

                default:
                    Console.WriteLine( $"Not supported BaseType in Variant Array: {type}" );
                    return null;
            }
        }

        private static object ReadValue( Stream inputStream, BuiltinType type )
        {
            switch ( type )
            {
                case BuiltinType.Byte:
                    return (byte)inputStream.ReadByte();

                case BuiltinType.Boolean:
                    int value = inputStream.ReadByte();
                    if ( value == 0 )
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }

                case BuiltinType.UInt16:
                    ushort? readValue = BaseType.ReadUInt16( inputStream );
                    if ( !readValue.HasValue )
                    {
                        return null;
                    }
                    return readValue.Value;

                case BuiltinType.Int16:
                    short? readInt16 = BaseType.ReadInt16( inputStream );
                    if ( !readInt16.HasValue )
                    {
                        return null;
                    }
                    return readInt16.Value;

                case BuiltinType.UInt32:
                    uint? readUInt32 = BaseType.ReadUInt32( inputStream );
                    if ( !readUInt32.HasValue )
                    {
                        return null;
                    }
                    return readUInt32.Value;

                case BuiltinType.Int32:
                    int? readInt32 = BaseType.ReadInt32( inputStream );
                    if ( !readInt32.HasValue )
                    {
                        return null;
                    }
                    return readInt32.Value;

                case BuiltinType.UInt64:
                    ulong? readUInt64 = BaseType.ReadUInt64( inputStream );
                    if ( !readUInt64.HasValue )
                    {
                        return null;
                    }
                    return readUInt64.Value;

                case BuiltinType.Int64:
                    long? readInt64 = BaseType.ReadInt64( inputStream );
                    if ( !readInt64.HasValue )
                    {
                        return null;
                    }
                    return readInt64.Value;

                case BuiltinType.DateTime:
                    long? readDate = BaseType.ReadInt64( inputStream );
                    if ( !readDate.HasValue )
                    {
                        return null;
                    }
                    if ( readDate.Value > DateTime.MaxValue.Ticks )
                    {
                        return DateTime.MaxValue;
                    }
                    if ( readDate.Value < DateTime.MinValue.Ticks )
                    {
                        return DateTime.MinValue;
                    }
                    return DateTime.FromFileTimeUtc( readDate.Value );

                case BuiltinType.Float:
                    float? readFloat = BaseType.ReadFloat( inputStream );
                    if ( !readFloat.HasValue )
                    {
                        return null;
                    }
                    return readFloat.Value;

                case BuiltinType.Double:
                    double? readDouble = BaseType.ReadDouble( inputStream );
                    if ( !readDouble.HasValue )
                    {
                        return null;
                    }
                    return readDouble.Value;

                case BuiltinType.String:
                    string parsed;
                    if ( !String.Decode( inputStream, out parsed ) )
                    {
                        return null;
                    }
                    return parsed;

                default:
                    Console.WriteLine( $"Not supported BaseType in Variant: {type}" );
                    return null;
            }
        }

        private static List<Variant> ReadVariantArray( Stream inputStream, uint numberOfElements )
        {
            List<Variant> array = new List<Variant>();
            if ( inputStream == null || !inputStream.CanRead )
            {
                return array;
            }
            for ( int i = 0; i < numberOfElements; i++ )
            {
                Variant item = Decode( inputStream );
                array.Add( item );
            }
            return array;
        }

        #region Overrides of Object

        public override string ToString()
        {
            List<Variant> list = Value as List<Variant>;
            if ( list != null )
            {
                return ToString( list );
            }
            return $"\t{Type}: {Value}";
        }

        private string ToString( List<Variant> list )
        {
            StringBuilder sb = new StringBuilder();
            foreach ( Variant item in list )
            {
                sb.AppendLine( System.Globalization.CultureInfo.InvariantCulture, $"\t{item}" );
            }
            return sb.ToString();
        }

        #endregion
    }
}