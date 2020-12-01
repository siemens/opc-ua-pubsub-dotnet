// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.IO;

namespace Binary.Messages.Chunk
{
    public class ChunkedMessage : NetworkMessage
    {
        public byte[]               ChunkData             { get; set; }
        public uint                 ChunkOffset           { get; set; }
        public ushort               MessageSequenceNumber { get; set; }
        public ChunkedPayloadHeader PayloadHeader         { get; set; }
        public uint                 TotalSize             { get; set; }

        public static ChunkedMessage Decode( Stream inputStream )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return null;
            }
            ChunkedMessage instance = new ChunkedMessage();
            instance.PayloadHeader = ChunkedPayloadHeader.Decode( inputStream );
            ushort? seqValue = BaseType.ReadUInt16( inputStream );
            if ( seqValue == null )
            {
                return null;
            }
            instance.MessageSequenceNumber = seqValue.Value;
            uint? offSetValue = BaseType.ReadUInt32( inputStream );
            if ( offSetValue == null )
            {
                return null;
            }
            instance.ChunkOffset = offSetValue.Value;
            uint? sizeValue = BaseType.ReadUInt32( inputStream );
            if ( sizeValue == null )
            {
                return null;
            }
            instance.TotalSize = sizeValue.Value;
            instance.ChunkData = SimpleArray<byte>.Decode( inputStream, BaseType.ReadByte );
            return instance;
        }

        public override void Encode( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            NetworkMessageHeader.Encode( outputStream );
            PayloadHeader.Encode( outputStream );
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( MessageSequenceNumber ) );
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( ChunkOffset ) );
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( TotalSize ) );
            int length = -1;
            if ( ChunkData != null )
            {
                length = ChunkData.Length;
            }
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( length ) );
            for ( int i = 0; i < length; i++ )
            {
                // ReSharper disable once PossibleNullReferenceException
                outputStream.WriteByte( ChunkData[i] );
            }
        }
    }
}