// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.IO;

namespace opc.ua.pubsub.dotnet.binary.Messages.Chunk
{
    public class ChunkedPayloadHeader
    {
        public ushort DataSetWriterID { get; set; }

        public static ChunkedPayloadHeader Decode( Stream inputStream )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return null;
            }
            ChunkedPayloadHeader instance = new ChunkedPayloadHeader();
            ushort?              value    = BaseType.ReadUInt16( inputStream );
            if ( value == null )
            {
                return null;
            }
            instance.DataSetWriterID = value.Value;
            return instance;
        }

        public void Encode( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( DataSetWriterID ) );
        }
    }
}