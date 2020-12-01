// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.IO;

namespace Binary.Messages.Meta
{
    public class EnumField : IEquatable<EnumField>
    {
        public EnumField()
        {
            Name = new String();
        }

        public LocalizedText Description { get; set; }
        public LocalizedText DisplayName { get; set; }
        public String        Name        { get; set; }
        public int           Value       { get; set; }

        public bool Equals( EnumField other )
        {
            if ( ReferenceEquals( null, other ) )
            {
                return false;
            }
            if ( ReferenceEquals( this, other ) )
            {
                return true;
            }
            return Value == other.Value && Equals( DisplayName, other.DisplayName ) && Equals( Description, other.Description ) && Equals( Name, other.Name );
        }

        public static EnumField Decode( Stream inputStream )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return null;
            }
            EnumField instance  = new EnumField();
            int?      readInt32 = BaseType.ReadInt32( inputStream );
            if ( readInt32 != null )
            {
                instance.Value = readInt32.Value;
            }
            instance.DisplayName = LocalizedText.Decode( inputStream );
            instance.Description = LocalizedText.Decode( inputStream );
            instance.Name        = String.Decode( inputStream );
            return instance;
        }

        public void Encode( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( Value ) );
            DisplayName.Encode( outputStream );
            Description.Encode( outputStream );
            Name.Encode( outputStream );
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
            return Equals( (EnumField)obj );
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Value;
                hashCode = ( hashCode * 397 ) ^ ( DisplayName != null ? DisplayName.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ ( Description != null ? Description.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ ( Name        != null ? Name.GetHashCode() : 0 );
                return hashCode;
            }
        }

        public static bool operator ==( EnumField left, EnumField right )
        {
            return Equals( left, right );
        }

        public static bool operator !=( EnumField left, EnumField right )
        {
            return !Equals( left, right );
        }

        #region Overrides of Object

        public override string ToString()
        {
            const int leftAlign = -30;
            return $"{Name,leftAlign} | {DisplayName,leftAlign} | {Description,10} | {Value,12}";
        }

        #endregion
    }
}