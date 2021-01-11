// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace opc.ua.pubsub.dotnet.binary
{
    /// <summary>
    ///     Reads an array of "simple" types, e.g. types which a fixed size.
    ///     Do not use it for types which have a varying size like FieldMetaData[] !!!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class SimpleArray<T> where T : struct
    {
        public static T[] Decode( Stream inputStream, Func<byte[], T?> parseMethod )
        {
            int    size   = Marshal.SizeOf( default(T) );
            byte[] buffer = Common.ReadBytes( inputStream, sizeof(int) );
            if ( buffer == null )
            {
                return null;
            }
            int length = BitConverter.ToInt32( buffer, 0 );
            if ( length < 1 )
            {
                return null;
            }
            return Decode( inputStream, parseMethod, length );
        }

        public static T[] Decode( Stream inputStream, Func<byte[], T?> parseMethod, int length )
        {
            int    size        = Marshal.SizeOf( default(T) );
            T[]    resultArray = new T[length];
            byte[] buffer      = new byte[size * length];
            for ( int i = 0; i < length; i++ )
            {
                int bytesRead = inputStream.Read( buffer, 0, size );
                if ( bytesRead != size )
                {
                    return null;
                }
                T? result = parseMethod( buffer );
                if ( !result.HasValue )
                {
                    return null;
                }
                resultArray[i] = result.Value;
            }
            return resultArray;
        }

        public static void Encode( Stream outputStream, T[] array, bool ignoreLength = false )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            int length = array != null ? array.Length : -1;
            if ( !ignoreLength )
            {
                length = EncodeArrayLength( outputStream, array );
            }
            TypeCode typeCode = Type.GetTypeCode( typeof(T) );
            for ( int i = 0; i < length; i++ )
            {
                byte[] buffer = EncodeElement( typeCode, array[i] );
                if ( buffer != null )
                {
                    outputStream.Write( buffer, 0, buffer.Length );
                }
            }
        }

        private static int EncodeArrayLength( Stream outputStream, T[] array )
        {
            int length = -1;
            if ( array != null )
            {
                length = array.Length;
            }
            byte[] lengthBuffer = BitConverter.GetBytes( length );
            outputStream.Write( lengthBuffer, 0, lengthBuffer.Length );
            return length;
        }

        private static byte[] EncodeElement( TypeCode typeCode, T element )
        {
            byte[] buffer = null;
            switch ( typeCode )
            {
                case TypeCode.Boolean:
                    buffer = BitConverter.GetBytes( (bool)(object)element );
                    break;

                case TypeCode.Byte:
                    buffer = BitConverter.GetBytes( (byte)(object)element );
                    break;

                case TypeCode.SByte:
                    buffer = BitConverter.GetBytes( (sbyte)(object)element );
                    break;

                case TypeCode.Int16:
                    buffer = BitConverter.GetBytes( (short)(object)element );
                    break;

                case TypeCode.UInt16:
                    buffer = BitConverter.GetBytes( (ushort)(object)element );
                    break;

                case TypeCode.Int32:
                    buffer = BitConverter.GetBytes( (int)(object)element );
                    break;

                case TypeCode.UInt32:
                    buffer = BitConverter.GetBytes( (uint)(object)element );
                    break;

                case TypeCode.Int64:
                    buffer = BitConverter.GetBytes( (long)(object)element );
                    break;

                case TypeCode.UInt64:
                    buffer = BitConverter.GetBytes( (ulong)(object)element );
                    break;

                case TypeCode.Single:
                    buffer = BitConverter.GetBytes( (float)(object)element );
                    break;

                case TypeCode.Double:
                    buffer = BitConverter.GetBytes( (double)(object)element );
                    break;

                default:
                    if ( Debugger.IsAttached )
                    {
                        Debugger.Break();
                    }
                    else
                    {
                        throw new NotImplementedException( $"Encoding of arrays of type {typeCode} is not implemented." );
                    }
                    break;
            }
            return buffer;
        }
    }
}