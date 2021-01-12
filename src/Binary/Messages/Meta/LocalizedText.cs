// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.IO;

namespace opc.ua.pubsub.dotnet.binary.Messages.Meta
{
    public class LocalizedText : IEquatable<LocalizedText>
    {
        public LocalizedText()
        {
            Locale = new String();
            Text   = new String();
        }

        /// <summary>
        ///     A bit mask that indicates which fields are present in the stream.<br></br>
        ///     The mask has the following bits: <br></br>
        ///     0x01	Locale<br></br>
        ///     0x02	Text
        /// </summary>
        public byte EncodingMask
        {
            get
            {
                byte mask = 0;
                if ( Locale.Length >= 0 )
                {
                    mask |= 0x01;
                }
                if ( Text.Length >= 0 )
                {
                    mask |= 0x02;
                }
                return mask;
            }
        }

        public String Locale { get; set; }
        public String Text   { get; set; }

        public bool Equals( LocalizedText other )
        {
            if ( ReferenceEquals( null, other ) )
            {
                return false;
            }
            if ( ReferenceEquals( this, other ) )
            {
                return true;
            }
            return Equals( Locale, other.Locale ) && Equals( Text, other.Text );
        }

        public static LocalizedText Decode( Stream inputStream )
        {
            LocalizedText instance        = new LocalizedText();
            byte          encodingMask    = (byte)inputStream.ReadByte();
            bool          localeIsPresent = false;
            bool          textIsPresent   = false;

            // Check if bit 1 is set to true
            // --> Locale is present
            if ( ( encodingMask & ( 1 << 0x00 ) ) != 0 )
            {
                localeIsPresent = true;
            }
            if ( localeIsPresent )
            {
                instance.Locale = String.Decode( inputStream );
            }

            // Check if bit 2 is to true
            // --> Text is present
            if ( ( encodingMask & ( 1 << 0x01 ) ) != 0 )
            {
                textIsPresent = true;
            }
            if ( textIsPresent )
            {
                instance.Text = String.Decode( inputStream );
            }
            return instance;
        }

        public void Encode( Stream outputStream )
        {
            outputStream.WriteByte( EncodingMask );
            if ( ( EncodingMask & 0x01 ) == 1 )
            {
                byte[] localeBytes = Locale.Encode();
                outputStream.Write( localeBytes, 0, localeBytes.Length );
            }
            if ( ( EncodingMask & 0x02 ) == 2 )
            {
                byte[] textBytes = Text.Encode();
                outputStream.Write( textBytes, 0, textBytes.Length );
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
            return Equals( (LocalizedText)obj );
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ( ( Locale != null ? Locale.GetHashCode() : 0 ) * 397 ) ^ ( Text != null ? Text.GetHashCode() : 0 );
            }
        }

        public static bool operator ==( LocalizedText left, LocalizedText right )
        {
            return Equals( left, right );
        }

        public static bool operator !=( LocalizedText left, LocalizedText right )
        {
            return !Equals( left, right );
        }

        #region Overrides of Object

        public override string ToString()
        {
            return $"[{Locale}]{Text}";
        }

        #endregion
    }
}