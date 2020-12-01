// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Binary.Messages.Meta.Structure
{
    public class EnumDescription : IEquatable<EnumDescription>
    {
        public EnumDescription()
        {
            Type = BuiltinType.Int32;
        }

        public NodeID          DataTypeID { get; set; }
        public List<EnumField> Fields     { get; set; }
        public QualifiedName   Name       { get; set; }
        [JsonConverter( typeof(StringEnumConverter) )]
        public BuiltinType Type { get; set; }

        public bool Equals( EnumDescription other )
        {
            if ( ReferenceEquals( null, other ) )
            {
                return false;
            }
            if ( ReferenceEquals( this, other ) )
            {
                return true;
            }
            return Equals( DataTypeID, other.DataTypeID ) && Equals( Name, other.Name ) && Equals( Fields, other.Fields ) && Type == other.Type;
        }

        public static EnumDescription Decode( Stream inputStream )
        {
            if ( inputStream == null )
            {
                return null;
            }
            EnumDescription instance = new EnumDescription();
            instance.DataTypeID = NodeID.Decode( inputStream );
            instance.Name       = QualifiedName.Decode( inputStream );
            int? arraySize = BaseType.ReadInt32( inputStream );
            if ( !arraySize.HasValue )
            {
                return null;
            }
            if ( arraySize.Value < 0 )
            {
                return instance;
            }
            instance.Fields = new List<EnumField>( arraySize.Value + 1 );
            for ( int i = 0; i < arraySize.Value; i++ )
            {
                instance.Fields.Add( EnumField.Decode( inputStream ) );
            }
            instance.Type = (BuiltinType)inputStream.ReadByte();
            return instance;
        }

        public void Encode( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            DataTypeID.Encode( outputStream );
            Name.Encode( outputStream );
            int fieldLength = -1;
            if ( Fields != null )
            {
                fieldLength = Fields.Count;
            }
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( fieldLength ) );
            for ( int i = 0; i < fieldLength; i++ )
            {
                // ReSharper disable once PossibleNullReferenceException
                Fields[i]
                       .Encode( outputStream );
            }
            outputStream.WriteByte( (byte)Type );
        }

        public override bool Equals( object obj )
        {
            if ( ReferenceEquals( null, obj ) )
            {
                return false;
            }
            if ( ReferenceEquals( this, obj ) )
            {
                return true;
            }
            if ( obj.GetType() != GetType() )
            {
                return false;
            }
            return Equals( (EnumDescription)obj );
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = DataTypeID                != null ? DataTypeID.GetHashCode() : 0;
                hashCode = ( hashCode * 397 ) ^ ( Name   != null ? Name.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ ( Fields != null ? Fields.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ (int)Type;
                return hashCode;
            }
        }

        public static bool operator ==( EnumDescription left, EnumDescription right )
        {
            return Equals( left, right );
        }

        public static bool operator !=( EnumDescription left, EnumDescription right )
        {
            return !Equals( left, right );
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat( CultureInfo.InvariantCulture, "{0, -30} {1}{2}", "DataType:", DataTypeID, Environment.NewLine, CultureInfo.InvariantCulture );
            sb.AppendFormat( CultureInfo.InvariantCulture, "{0, -30} {1}{2}", "Name:",     Name,       Environment.NewLine, CultureInfo.InvariantCulture );
            int fieldsLength = -1;
            if ( Fields != null )
            {
                fieldsLength = Fields.Count;
            }
            sb.AppendFormat( CultureInfo.InvariantCulture, "{0, -30} {1}{2}", "EnumFields:", fieldsLength, Environment.NewLine );
            const int leftAlign = -30;
            sb.AppendLine( "----------------------------------------------------------------------------------------------------------" );
            sb.AppendLine( $"{"Name",leftAlign} | {"DisplayName",leftAlign} | {"Description",10} | {"Value",12}" );
            sb.AppendLine( "----------------------------------------------------------------------------------------------------------" );
            for ( int i = 0; i < fieldsLength; i++ )
            {
                sb.AppendLine( Fields[i]
                                     ?.ToString()
                            ?? "null"
                             );
            }
            sb.AppendFormat( CultureInfo.InvariantCulture, "{0, -30} {1}{2}", "BuildInType:", Type, Environment.NewLine );
            return sb.ToString();
        }
    }
}