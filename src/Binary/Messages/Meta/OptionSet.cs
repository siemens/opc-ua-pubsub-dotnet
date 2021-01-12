// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.IO;

namespace opc.ua.pubsub.dotnet.binary.Messages.Meta
{
    public class OptionSet : ICodable<DataSetFieldFlags>, IEquatable<OptionSet>
    {

        public OptionSet( EncodingOptions options )
        {
            Options = options;
        }

        public byte[] ValidBits { get; set; }
        public byte[] Value     { get; set; }

        public void Encode( Stream outputStream, bool withHeader = true )
        {
            if ( !Options.LegacyFieldFlagEncoding )
            {
                outputStream.WriteByte( Value[0] );
                outputStream.WriteByte( ValidBits[0] );
                return;
            }
            int valueLength     = -1;
            int ValidBitsLength = -1;
            if ( Value != null )
            {
                valueLength = Value.Length;
            }
            byte[] valueLengthEncoded = BitConverter.GetBytes( valueLength );
            if ( ValidBits != null )
            {
                ValidBitsLength = ValidBits.Length;
            }
            byte[] validLengthEncoded = BitConverter.GetBytes( ValidBitsLength );
            outputStream.Write( valueLengthEncoded, 0, valueLengthEncoded.Length );
            if ( Value != null && valueLength > 0 )
            {
                for ( int i = 0; i < Value.Length; i++ )
                {
                    outputStream.WriteByte( Value[i] );
                }
            }
            outputStream.Write( validLengthEncoded, 0, validLengthEncoded.Length );
            if ( ValidBits != null && ValidBitsLength > 0 )
            {
                for ( int i = 0; i < ValidBits.Length; i++ )
                {
                    outputStream.WriteByte( ValidBits[i] );
                }
            }
        }

        public EncodingOptions Options { get; protected set; }

        public bool Equals( OptionSet other )
        {
            if ( ReferenceEquals( null, other ) )
            {
                return false;
            }
            if ( ReferenceEquals( this, other ) )
            {
                return true;
            }
            return Equals( Value, other.Value ) && Equals( ValidBits, other.ValidBits ) && Equals( Options, other.Options );
        }

        public static OptionSet Decode( Stream inputStream, EncodingOptions options )
        {
            OptionSet instance = new OptionSet( options );
            if ( options.LegacyFieldFlagEncoding )
            {
                inputStream.Position += 4;
                instance.Value = new[] { (byte)inputStream.ReadByte() };
                inputStream.Position += 4;
                instance.ValidBits = new[] { (byte)inputStream.ReadByte() };
                return instance;
            }
            instance.Value     = new[] { (byte)inputStream.ReadByte() };
            instance.ValidBits = new[] { (byte)inputStream.ReadByte() };
            return instance;
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
            return Equals( (OptionSet)obj );
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Value                        != null ? Value.GetHashCode() : 0;
                hashCode = ( hashCode * 397 ) ^ ( ValidBits != null ? ValidBits.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ ( Options   != null ? Options.GetHashCode() : 0 );
                return hashCode;
            }
        }

        public static bool operator ==( OptionSet left, OptionSet right )
        {
            return Equals( left, right );
        }

        public static bool operator !=( OptionSet left, OptionSet right )
        {
            return !Equals( left, right );
        }
    }
}