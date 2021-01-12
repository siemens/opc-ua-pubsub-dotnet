// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.IO;

namespace opc.ua.pubsub.dotnet.binary.Messages.Meta.Structure
{
    public class StructureField : IEquatable<StructureField>
    {
        public StructureField() : this( "", new NodeID() ) { }

        public StructureField( string name, NodeID dataType, bool isOptional = false, int valueRank = -1, LocalizedText description = null, uint maxStringLen = 0 )
        {
            Name            = new String( name );
            DataType        = dataType;
            IsOptional      = isOptional;
            ValueRank       = valueRank;
            Description     = description == null ? new LocalizedText() : description;
            MaxStringLength = maxStringLen;
        }

        public uint[]        ArrayDimension  { get; set; }
        public NodeID        DataType        { get; set; }
        public LocalizedText Description     { get; set; }
        public bool          IsOptional      { get; set; }
        public uint          MaxStringLength { get; set; }
        public String        Name            { get; set; }
        public int           ValueRank       { get; set; }

        public bool Equals( StructureField other )
        {
            if ( ReferenceEquals( null, other ) )
            {
                return false;
            }
            if ( ReferenceEquals( this, other ) )
            {
                return true;
            }
            return Equals( Name,        other.Name )
                && Equals( Description, other.Description )
                && Equals( DataType,    other.DataType )
                && ValueRank  == other.ValueRank
                && IsOptional == other.IsOptional
                && ArrayDimension.NullableSequenceEquals( other.ArrayDimension )
                && MaxStringLength == other.MaxStringLength;
        }

        public static StructureField Decode( Stream inputStream )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return null;
            }
            StructureField instance = new StructureField();
            instance.Name        = String.Decode( inputStream );
            instance.Description = LocalizedText.Decode( inputStream );
            instance.DataType    = NodeID.Decode( inputStream );
            int? readInt32 = BaseType.ReadInt32( inputStream );
            if ( readInt32 != null )
            {
                instance.ValueRank = readInt32.Value;
            }
            instance.ArrayDimension = SimpleArray<uint>.Decode( inputStream, BaseType.ReadUInt32 );
            uint? readUInt32 = BaseType.ReadUInt32( inputStream );
            if ( readUInt32 != null )
            {
                instance.MaxStringLength = readUInt32.Value;
            }
            instance.IsOptional = inputStream.ReadByte() != 0;
            return instance;
        }

        public void Encode( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            Name.Encode( outputStream );
            Description.Encode( outputStream );
            DataType.Encode( outputStream );
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( ValueRank ) );
            SimpleArray<uint>.Encode( outputStream, ArrayDimension );
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( MaxStringLength ) );
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( IsOptional ) );
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
            return Equals( (StructureField)obj );
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Name                           != null ? Name.GetHashCode() : 0;
                hashCode = ( hashCode * 397 ) ^ ( Description != null ? Description.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ ( DataType    != null ? DataType.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ ValueRank;
                hashCode = ( hashCode * 397 ) ^ IsOptional.GetHashCode();
                hashCode = ( hashCode * 397 ) ^ ( ArrayDimension != null ? ArrayDimension.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ (int)MaxStringLength;
                return hashCode;
            }
        }

        public static bool operator ==( StructureField left, StructureField right )
        {
            return Equals( left, right );
        }

        public static bool operator !=( StructureField left, StructureField right )
        {
            return !Equals( left, right );
        }

        public string ToConsoleString()
        {
            const int leftAlign = -30;
            return $"{Name,leftAlign} | {Description,leftAlign} | {DataType,10} | {ValueRank,12} | {ArrayDimension,10} | {MaxStringLength,5} | {IsOptional,12}";
        }

        #region Overrides of Object

        public override string ToString()
        {
            return $"{Name}, {Description}, {DataType}, {ValueRank}, {ArrayDimension}, {MaxStringLength}, {IsOptional}";
        }

        #endregion
    }
}