// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Binary
{
    public class String : IEquatable<String>
    {
        public String( string value )
        {
            Value = value;
        }

        public String()
        {
            Value = null;
        }

        public int Length
        {
            get
            {
                if ( string.IsNullOrEmpty( Value ) )
                {
                    return -1;
                }
                return Value.Length;
            }
        }

        public string Value { get; set; }

        public bool Equals( String other )
        {
            if ( ReferenceEquals( null, other ) )
            {
                return false;
            }
            if ( ReferenceEquals( this, other ) )
            {
                return true;
            }
            return Value == other.Value;
        }

        public static bool Decode( Stream inputStream, out string result )
        {
            result = null;
            int lengthLength = sizeof(int);
            if ( inputStream == null || !inputStream.CanRead )
            {
                return false;
            }
            byte[] buffer    = new byte[lengthLength];
            int    readBytes = int.MinValue;
            try
            {
                readBytes = inputStream.Read( buffer, 0, lengthLength );
            }
            catch ( Exception e )
            {
                Console.WriteLine( e );
            }
            if ( readBytes != lengthLength )
            {
                return false;
            }
            if ( Common.ReverseOrder )
#pragma warning disable 162
            {
                Array.Reverse( buffer );
            }
#pragma warning restore 162
            int length = BitConverter.ToInt32( buffer, 0 );
            if ( length < 0 )
            {
                return false;
            }
            buffer    = new byte[length];
            readBytes = int.MinValue;
            try
            {
                readBytes = inputStream.Read( buffer, 0, length );
            }
            catch ( Exception e )
            {
                Console.WriteLine( e );
            }
            if ( readBytes < length )
            {
                return false;
            }
            if ( Common.ReverseOrder )
#pragma warning disable 162
            {
                Array.Reverse( buffer );
            }
#pragma warning restore 162
            result = Encoding.UTF8.GetString( buffer );
            return true;
        }

        public static String Decode( Stream inputStream )
        {
            Decode( inputStream, out string temp );
            return new String( temp );
        }

        /// <summary>
        ///     Encodes a string according to UADP definition.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] Encode()
        {
            if ( Value == null )
            {
                return BitConverter.GetBytes( -1 );
            }
            byte[] encodedString = Encoding.UTF8.GetBytes( Value );
            byte[] length        = BitConverter.GetBytes( encodedString.Length );
            if ( Common.ReverseOrder )
            {
#pragma warning disable CS0162 // Unreachable code detected
                Array.Reverse( encodedString );
                Array.Reverse( length );
#pragma warning restore CS0162 // Unreachable code detected
            }
            byte[] encodedData = new byte[length.Length + encodedString.Length];
            length.CopyTo( encodedData, 0 );
            encodedString.CopyTo( encodedData, length.Length );
            return encodedData;
        }

        public void Encode( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            byte[] buffer = Encode();
            if ( buffer == null )
            {
                return;
            }
            outputStream.Write( buffer, 0, buffer.Length );
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
            return Equals( (String)obj );
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

        public static bool operator ==( String left, String right )
        {
            return Equals( left, right );
        }

        public static bool operator !=( String left, String right )
        {
            return !Equals( left, right );
        }

        #region Overrides of Object

        public override string ToString()
        {
            string    value;
            const int maxLength = 40;
            if ( Length > maxLength )
            {
                int offset = Length - maxLength;
                value = $"...{Value.Substring( offset )}";
            }
            else
            {
                value = Value;
            }
            return string.Format( CultureInfo.InvariantCulture, "[{0}]{1}", Length, value );
        }

        #endregion
    }
}