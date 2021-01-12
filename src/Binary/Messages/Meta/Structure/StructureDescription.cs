// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace opc.ua.pubsub.dotnet.binary.Messages.Meta.Structure
{
    public class StructureDescription : IEquatable<StructureDescription>
    {
        public StructureDescription() { }

        public StructureDescription( string name, NodeID dataTypeId, NodeID defaultEncoding, NodeID baseDataType, int structureType, List<StructureField> structFields )
        {
            Name            = new QualifiedName( name );
            DataTypeId      = dataTypeId;
            DefaultEncoding = defaultEncoding;
            BaseDataType    = baseDataType;
            StructureType   = structureType;
            Fields          = structFields;
        }

        public NodeID               BaseDataType    { get; set; }
        public NodeID               DataTypeId      { get; set; }
        public NodeID               DefaultEncoding { get; set; }
        public List<StructureField> Fields          { get; set; }
        public QualifiedName        Name            { get; set; }
        public int                  StructureType   { get; set; }

        public bool Equals( StructureDescription other )
        {
            if ( ReferenceEquals( null, other ) )
            {
                return false;
            }
            if ( ReferenceEquals( this, other ) )
            {
                return true;
            }
            return Equals( DataTypeId,      other.DataTypeId )
                && Equals( Name,            other.Name )
                && Equals( DefaultEncoding, other.DefaultEncoding )
                && Equals( BaseDataType,    other.BaseDataType )
                && StructureType == other.StructureType
                && Fields.NullableSequenceEquals( other.Fields );
        }

        public static StructureDescription Decode( Stream inputStream )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return null;
            }
            StructureDescription instance = new StructureDescription();
            instance.DataTypeId      = NodeID.Decode( inputStream );
            instance.Name            = QualifiedName.Decode( inputStream );
            instance.DefaultEncoding = NodeID.Decode( inputStream );
            instance.BaseDataType    = NodeID.Decode( inputStream );
            int? readInt32 = BaseType.ReadInt32( inputStream );
            if ( readInt32 != null )
            {
                instance.StructureType = readInt32.Value;
            }
            int? int32 = BaseType.ReadInt32( inputStream );
            if ( int32 == null )
            {
                return instance;
            }
            instance.Fields = new List<StructureField>();
            int fieldsLength = int32.Value;
            for ( int i = 0; i < fieldsLength; i++ )
            {
                instance.Fields.Add( StructureField.Decode( inputStream ) );
            }
            return instance;
        }

        public void Encode( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            DataTypeId.Encode( outputStream );
            Name.Encode( outputStream );
            DefaultEncoding.Encode( outputStream );
            BaseDataType.Encode( outputStream );
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( StructureType ) );
            int arrayLength = -1;
            if ( Fields != null )
            {
                arrayLength = Fields.Count;
            }
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( arrayLength ) );
            for ( int i = 0; i < arrayLength; i++ )
            {
                Fields[i]
                       .Encode( outputStream );
            }
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
            return Equals( (StructureDescription)obj );
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = DataTypeId                         != null ? DataTypeId.GetHashCode() : 0;
                hashCode = ( hashCode * 397 ) ^ ( Name            != null ? Name.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ ( DefaultEncoding != null ? DefaultEncoding.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ ( BaseDataType    != null ? BaseDataType.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ StructureType;
                hashCode = ( hashCode * 397 ) ^ ( Fields != null ? Fields.GetHashCode() : 0 );
                return hashCode;
            }
        }

        public static bool operator ==( StructureDescription left, StructureDescription right )
        {
            return Equals( left, right );
        }

        public static bool operator !=( StructureDescription left, StructureDescription right )
        {
            return !Equals( left, right );
        }

        public string ToConsoleString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat( CultureInfo.InvariantCulture, "{0, -30} {1}{2}", "DataType:",        DataTypeId,      Environment.NewLine );
            sb.AppendFormat( CultureInfo.InvariantCulture, "{0, -30} {1}{2}", "Name:",            Name,            Environment.NewLine );
            sb.AppendFormat( CultureInfo.InvariantCulture, "{0, -30} {1}{2}", "DefaultEncoding:", DefaultEncoding, Environment.NewLine );
            sb.AppendFormat( CultureInfo.InvariantCulture, "{0, -30} {1}{2}", "BaseDataType:",    BaseDataType,    Environment.NewLine );
            sb.AppendFormat( CultureInfo.InvariantCulture, "{0, -30} {1}{2}", "StructureType:",   StructureType,   Environment.NewLine );
            int fieldsLength = -1;
            if ( Fields != null )
            {
                fieldsLength = Fields.Count;
            }
            sb.AppendFormat( CultureInfo.InvariantCulture, "{0, -30} {1}{2}", "StructureFields:", fieldsLength, Environment.NewLine );
            const int leftAlign = -30;
            sb.AppendLine( "---------------------------------------------------------------------------------------------------------------------------------------------------------"
                         );
            sb.AppendLine( $"{"Name",leftAlign} | {"Description",leftAlign} | {"DataType",10} | {"ValueRank",12} | {"ArrayDimension",10} | {"MaxStringLength",5} | {"IsOptional",12}"
                         );
            sb.AppendLine( "----------------------------------------------------------------------------------------------------------------------------------------------------------"
                         );
            for ( int i = 0; i < fieldsLength; i++ )
            {
                sb.AppendLine( Fields[i]
                                     ?.ToConsoleString()
                            ?? "null"
                             );
            }
            return sb.ToString();
        }

        #region Overrides of Object

        #endregion
    }
}