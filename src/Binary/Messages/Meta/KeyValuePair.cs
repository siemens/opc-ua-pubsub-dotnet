// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.IO;

namespace Binary.Messages.Meta
{
    public class KeyValuePair : IEquatable<KeyValuePair>
    {
        private object m_Value;
        public KeyValuePair() { }

        public KeyValuePair( string name, String value )
        {
            Name      = new QualifiedName( name );
            ValueType = BuiltinType.String;
            Value     = value;
        }

        public QualifiedName Name { get; set; }

        public object Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                // check that only supported types can be set 
                if ( value is String || value is byte )
                {
                    m_Value = value;
                }
                else
                {
                    m_Value = null;

                    // throw exception; only UADP.String and byte are supporte as Value of KeyValue pairs at the moment
                    throw new NotImplementedException( "Set Value of a KeyValuePair: Object type not supported." );
                }
            }
        }

        public BuiltinType ValueType { get; set; }

        public bool Equals( KeyValuePair other )
        {
            if ( ReferenceEquals( null, other ) )
            {
                return false;
            }
            if ( ReferenceEquals( this, other ) )
            {
                return true;
            }
            return Equals( m_Value, other.m_Value ) && Equals( Name, other.Name ) && ValueType == other.ValueType;
        }

        public static KeyValuePair Decode( Stream inputStream )
        {
            KeyValuePair instance = new KeyValuePair();
            instance.Name      = QualifiedName.Decode( inputStream );
            instance.ValueType = (BuiltinType)inputStream.ReadByte();
            if ( !ReadBuiltinTypeValue( instance.ValueType, inputStream, out object parsedValue ) )
            {
                return null;
            }
            if ( instance.ValueType == BuiltinType.String )
            {
                // strings always as UADP.String types in Value of KeyValuePair
                instance.Value = new String( parsedValue as string );
            }
            else
            {
                instance.Value = parsedValue;
            }
            return instance;
        }

        public void Encode( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            Name.Encode( outputStream );

            // 2. Variant
            // 2.1 Encoding Mask (Byte)
            byte encodingMask = (byte)ValueType;
            outputStream.WriteByte( encodingMask );

            // 2.2 Value
            switch ( ValueType )
            {
                case BuiltinType.Byte:
                    outputStream.WriteByte( (byte)Value );
                    break;

                case BuiltinType.String:
                    if ( Value == null )
                    {
                        String valueString = new String();
                        valueString.Encode( outputStream );
                        break;
                    }
                    if ( Value is String opcString )
                    {
                        opcString.Encode( outputStream );
                    }
                    break;

                default:
                    throw new NotImplementedException( $"Encoding a KeyValuePair, with a variant containing type {ValueType} is not implemented." );
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
            return Equals( (KeyValuePair)obj );
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = m_Value                 != null ? m_Value.GetHashCode() : 0;
                hashCode = ( hashCode * 397 ) ^ ( Name != null ? Name.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ (int)ValueType;
                return hashCode;
            }
        }

        public static bool operator ==( KeyValuePair left, KeyValuePair right )
        {
            return Equals( left, right );
        }

        public static bool operator !=( KeyValuePair left, KeyValuePair right )
        {
            return !Equals( left, right );
        }

        private static bool ReadBuiltinTypeValue( BuiltinType type, Stream inputStream, out object result )
        {
            switch ( type )
            {
                case BuiltinType.Byte:
                    result = (byte)inputStream.ReadByte();
                    return true;

                case BuiltinType.String:
                    bool success = String.Decode( inputStream, out string parsed );
                    result = parsed;
                    return success;

                default:
                    throw new NotImplementedException( $"Parsing a KeyValuePair, with a variant containing type {type} is not implemented." );
            }
        }
    }
}