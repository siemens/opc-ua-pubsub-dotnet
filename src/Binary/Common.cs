// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Globalization;
using System.IO;

namespace Binary
{
    internal static class Common
    {
        public const bool ReverseOrder = false;

        public static T[] FromString<T>( string stringArray )
        {
            string[] tempArray = stringArray.Split( ';' );
            T[]      array     = new T[tempArray.Length];
            for ( int i = 0; i < tempArray.Length; i++ )
            {
                array[i] = (T)Convert.ChangeType( tempArray[i], typeof(T), CultureInfo.InvariantCulture );
            }
            return array;
        }

        public static byte[] ReadBytes( Stream inputStream, int size )
        {
            byte[] buffer    = new byte[size];
            int    readBytes = int.MinValue;
            try
            {
                readBytes = inputStream.Read( buffer, 0, size );
            }
            catch ( Exception e )
            {
                Console.WriteLine( e );
            }
            if ( readBytes != size )
            {
                return null;
            }
            if ( ReverseOrder )
#pragma warning disable 162
            {
                Array.Reverse( buffer );
            }
#pragma warning restore 162
            return buffer;
        }
    }
}