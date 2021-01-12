// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.IO;

namespace opc.ua.pubsub.dotnet.binary.Messages
{
    public class DataSetPayloadHeader : BaseHeader
    {
        public byte Count { get; set; }

        public static DataSetPayloadHeader Decode( Stream inputStream )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return null;
            }
            DataSetPayloadHeader instance = new DataSetPayloadHeader();
            instance.Count           = (byte)inputStream.ReadByte();
            instance.DataSetWriterID = SimpleArray<ushort>.Decode( inputStream, BaseType.ReadUInt16, instance.Count );
            return instance;
        }

        public override void Encode( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            outputStream.WriteByte( Count );
            SimpleArray<ushort>.Encode( outputStream, DataSetWriterID, true );
        }
    }
}