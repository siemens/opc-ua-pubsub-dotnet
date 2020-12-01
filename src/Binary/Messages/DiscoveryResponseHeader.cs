// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.IO;

namespace Binary.Messages
{
    public class DiscoveryResponseHeader
    {
        public byte   ResponseType   { get; set; }
        public ushort SequenceNumber { get; set; }

        public static DiscoveryResponseHeader Decode( Stream inputStream )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return null;
            }
            DiscoveryResponseHeader instance = new DiscoveryResponseHeader();
            instance.ResponseType = (byte)inputStream.ReadByte();
            ushort? sequenceNUmber = BaseType.ReadUInt16( inputStream );
            if ( sequenceNUmber != null )
            {
                instance.SequenceNumber = sequenceNUmber.Value;
            }
            return instance;
        }

        public void Encode( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            outputStream.WriteByte( ResponseType );
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( SequenceNumber ) );
        }
    }
}