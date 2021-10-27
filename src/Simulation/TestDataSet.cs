// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using opc.ua.pubsub.dotnet.binary.Header;
using opc.ua.pubsub.dotnet.binary.Messages.Chunk;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using String = opc.ua.pubsub.dotnet.binary.String;

namespace opc.ua.pubsub.dotnet.simulation
{
    public abstract class TestDataSet
    {
        protected MetaFrame m_MetaFrame;
        protected string    m_PublisherID;

        public TestDataSet( string publisherID )
        {
            m_PublisherID = publisherID;
            CreateMeta();
        }

        public abstract List<byte[]> GetChunkedKeyFrame( uint chunkSize );

        public virtual List<byte[]> GetChunkedMetaFrame( uint chunkSize )
        {
            if ( m_MetaFrame == null || chunkSize == 0 )
            {
                return null;
            }
            byte[] rawMessage = null;
            using ( MemoryStream outputStream = new MemoryStream() )
            {
                m_MetaFrame.EncodeChunk( outputStream );
                rawMessage = outputStream.ToArray();
            }
            List<byte[]> rawChunks = new List<byte[]>();
            for ( uint i = 0; i < rawMessage.LongLength; i += chunkSize )
            {
                NetworkMessageHeader networkHeader  = GetChunkedMetaNetworkHeader();
                ChunkedMessage       chunkedMessage = new ChunkedMessage();
                chunkedMessage.PayloadHeader = new ChunkedPayloadHeader();

                //chunkedMessage.PayloadHeader.DataSetWriterID = m_MetaFrame.DataSetWriterID;
                chunkedMessage.NetworkMessageHeader  = networkHeader;
                chunkedMessage.TotalSize             = (uint)rawMessage.LongLength;
                chunkedMessage.ChunkOffset           = i;
                chunkedMessage.MessageSequenceNumber = m_MetaFrame.SequenceNumber;

                // Check if can copy a "full" chunk or just the remaining elements of the array.
                long length = Math.Min( chunkSize, rawMessage.LongLength - chunkedMessage.ChunkOffset );
                chunkedMessage.ChunkData = new byte[length];
                Array.Copy( rawMessage, i, chunkedMessage.ChunkData, 0, length );
                using ( MemoryStream stream = new MemoryStream() )
                {
                    chunkedMessage.Encode( stream );
                    rawChunks.Add( stream.ToArray() );
                }
            }
            return rawChunks;
        }

        public abstract byte[] GetEncodedDeltaFrame();
        public abstract byte[] GetEncodedKeyFrame();

        public byte[] GetEncodedMetaFrame()
        {
            if ( m_MetaFrame == null )
            {
                return null;
            }
            using ( MemoryStream outputStream = new MemoryStream() )
            {
                m_MetaFrame.Encode( outputStream );
                return outputStream.ToArray();
            }
        }

        protected abstract void CreateMeta();

        protected NetworkMessageHeader GetChunkedMetaNetworkHeader()
        {
            NetworkMessageHeader networkHeader = new NetworkMessageHeader();
            networkHeader.PublisherID     = new String(m_PublisherID);
            networkHeader.VersionAndFlags = 0x91;
            networkHeader.ExtendedFlags1 = new ExtendedFlags1
                                           {
                                                   RawValue = 0x84
                                           };
            networkHeader.ExtendedFlags2 = new ExtendedFlags2
                                           {
                                                   RawValue = 0x09
                                           };
            return networkHeader;
        }

        protected NetworkMessageHeader GetChunkedNetworkHeader()
        {
            NetworkMessageHeader networkHeader = new NetworkMessageHeader();
            networkHeader.PublisherID     = new String( m_PublisherID );
            networkHeader.VersionAndFlags = 0xD1;
            networkHeader.ExtendedFlags1 = new ExtendedFlags1
                                           {
                                                   RawValue = 0x84
                                           };
            networkHeader.ExtendedFlags2 = new ExtendedFlags2
                                           {
                                                   RawValue = 0x01
                                           };
            return networkHeader;
        }
    }
}