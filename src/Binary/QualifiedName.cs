// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.IO;

namespace Binary
{
    public class QualifiedName : IEquatable<QualifiedName>
    {
        public QualifiedName() { }

        public QualifiedName( string name )
        {
            NamespaceIndex = 1;
            Name           = new String( name );
        }

        public String Name           { get; set; }
        public ushort NamespaceIndex { get; set; }

        public bool Equals( QualifiedName other )
        {
            if ( ReferenceEquals( null, other ) )
            {
                return false;
            }
            if ( ReferenceEquals( this, other ) )
            {
                return true;
            }
            return NamespaceIndex == other.NamespaceIndex && Equals( Name, other.Name );
        }

        public static QualifiedName Decode( Stream inputStream )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return null;
            }
            QualifiedName instance   = new QualifiedName();
            ushort?       readUInt16 = BaseType.ReadUInt16( inputStream );
            if ( readUInt16 != null )
            {
                instance.NamespaceIndex = readUInt16.Value;
            }
            instance.Name = String.Decode( inputStream );
            return instance;
        }

        public void Encode( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( NamespaceIndex ) );
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
            return Equals( (QualifiedName)obj );
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ( NamespaceIndex.GetHashCode() * 397 ) ^ ( Name != null ? Name.GetHashCode() : 0 );
            }
        }

        public static bool operator ==( QualifiedName left, QualifiedName right )
        {
            return Equals( left, right );
        }

        public static bool operator !=( QualifiedName left, QualifiedName right )
        {
            return !Equals( left, right );
        }

        #region Overrides of Object

        public override string ToString()
        {
            return $"[{NamespaceIndex}]{Name}";
        }

        #endregion
    }
}