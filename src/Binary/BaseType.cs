// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Text;

namespace opc.ua.pubsub.dotnet.binary
{
    public static class BaseType
    {
        public static byte? ReadByte( Stream inputStream )
        {
            const int size   = sizeof(byte);
            byte[]    buffer = ReadFromStream( inputStream, size );
            return ReadByte( buffer );
        }

        public static byte? ReadByte( byte[] buffer )
        {
            if ( buffer == null || buffer.Length == 0 )
            {
                return null;
            }
            return buffer[0];
        }

        public static double? ReadDouble( Stream inputStream )
        {
            const int size   = sizeof(double);
            byte[]    buffer = ReadFromStream( inputStream, size );
            return ReadDouble( buffer );
        }

        public static double? ReadDouble( byte[] buffer )
        {
            if ( buffer == null )
            {
                return null;
            }
            return BitConverter.ToDouble( buffer, 0 );
        }

        public static float? ReadFloat( Stream inputStream )
        {
            const int size   = sizeof(float);
            byte[]    buffer = ReadFromStream( inputStream, size );
            return ReadFloat( buffer );
        }

        public static float? ReadFloat( byte[] buffer )
        {
            if ( buffer == null )
            {
                return null;
            }
            return BitConverter.ToSingle( buffer, 0 );
        }

        /// <summary>
        ///     Reads the requested amount of bytes from a stream and automatically reverse the byte order if required.
        /// </summary>
        /// <param name="inputStream">The stream from which the data should be retrieved.</param>
        /// <param name="size">Number of bytes which should be read from the stream.</param>
        /// <returns></returns>
        public static byte[] ReadFromStream( Stream inputStream, int size )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return null;
            }
            int    bytesRead;
            byte[] buffer = new byte[size];
            try
            {
                bytesRead = inputStream.Read( buffer, 0, size );
            }
            catch ( Exception e )
            {
                Console.WriteLine( e );
                return null;
            }
            if ( bytesRead != size )
            {
                return null;
            }
            if ( Common.ReverseOrder )
#pragma warning disable 162
            {
                Array.Reverse( buffer );
            }
#pragma warning restore 162
            return buffer;
        }

        public static short? ReadInt16( Stream inputStream )
        {
            const int size   = sizeof(short);
            byte[]    buffer = ReadFromStream( inputStream, size );
            return ReadInt16( buffer );
        }

        public static short? ReadInt16( byte[] buffer )
        {
            if ( buffer == null )
            {
                return null;
            }
            return BitConverter.ToInt16( buffer, 0 );
        }

        public static int? ReadInt32( Stream inputStream )
        {
            const int size   = sizeof(int);
            byte[]    buffer = ReadFromStream( inputStream, size );
            return ReadInt32( buffer );
        }

        public static int? ReadInt32( byte[] buffer )
        {
            if ( buffer == null )
            {
                return null;
            }
            return BitConverter.ToInt32( buffer, 0 );
        }

        public static long? ReadInt64( Stream inputStream )
        {
            const int size   = sizeof(long);
            byte[]    buffer = ReadFromStream( inputStream, size );
            return ReadInt64( buffer );
        }

        public static long? ReadInt64( byte[] buffer )
        {
            if ( buffer == null )
            {
                return null;
            }
            return BitConverter.ToInt64( buffer, 0 );
        }

        public static String ReadString( Stream inputStream )
        {
            byte[] buffer = null;
            int?   length = ReadInt32( inputStream );
            if ( length > 0 )
            {
                buffer = ReadFromStream( inputStream, (int)length );
            }
            return ReadString( buffer );
        }

        public static String ReadString( byte[] buffer )
        {
            if ( buffer == null )
            {
                return null;
            }
            String uadpstring = new String();
            uadpstring.Value = Encoding.UTF8.GetString( buffer );
            return uadpstring;
        }

        public static ushort? ReadUInt16( Stream inputStream )
        {
            const int size   = sizeof(ushort);
            byte[]    buffer = ReadFromStream( inputStream, size );
            return ReadUInt16( buffer );
        }

        public static ushort? ReadUInt16( byte[] buffer )
        {
            if ( buffer == null )
            {
                return null;
            }
            return BitConverter.ToUInt16( buffer, 0 );
        }

        public static uint? ReadUInt32( Stream inputStream )
        {
            const int size   = sizeof(uint);
            byte[]    buffer = ReadFromStream( inputStream, size );
            return ReadUInt32( buffer );
        }

        public static uint? ReadUInt32( byte[] buffer )
        {
            if ( buffer == null )
            {
                return null;
            }
            return BitConverter.ToUInt32( buffer, 0 );
        }

        public static ulong? ReadUInt64( Stream inputStream )
        {
            const int size   = sizeof(ulong);
            byte[]    buffer = ReadFromStream( inputStream, size );
            return ReadUInt64( buffer );
        }

        public static ulong? ReadUInt64( byte[] buffer )
        {
            if ( buffer == null )
            {
                return null;
            }
            return BitConverter.ToUInt64( buffer, 0 );
        }

        public static void WriteToStream( Stream outputStream, byte[] buffer )
        {
            if ( outputStream == null || !outputStream.CanWrite || buffer == null )
            {
                return;
            }
            outputStream.Write( buffer, 0, buffer.Length );
        }
    }
}