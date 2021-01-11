// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace opc.ua.pubsub.dotnet.binary.Decode
{
    public class ChunkStorage
    {
        private static readonly ILog Logger = LogManager.GetLogger( MethodBase.GetCurrentMethod()
                                                                              .DeclaringType
                                                                  );

        public ChunkStorage()
        {
            Chunks = new SortedSet<Chunk>( new ChunkComparer() );
        }

        public ushort DataSetWriterID { get; set; }

        public bool IsComplete
        {
            get
            {
                if ( Logger.IsDebugEnabled )
                {
                    Logger.Debug( $"TotalSize: {TotalSize}, ReceivedSize {ReceivedSize} ? " );
                }
                return TotalSize == ReceivedSize;
            }
        }

        public string PublisherID { get; set; }
        /// <summary>
        ///     Sum of the size of all received chunks so far.
        ///     If ReceivedSize equals TotalSize all chunks are available
        ///     and can be retrieved.
        /// </summary>
        public uint ReceivedSize { get; set; }
        /// <summary>
        ///     Total Size of the Message according to the Chunk Header.
        /// </summary>
        public uint TotalSize { get; set; }
        protected SortedSet<Chunk> Chunks { get; }

        public bool Add( Chunk chunk )
        {
            if ( chunk == null )
            {
                return false;
            }
            Chunks.Add( chunk );
            ReceivedSize += (uint)chunk.Data.Length;
            return IsComplete;
        }

        public void Clear()
        {
            TotalSize    = 0;
            ReceivedSize = 0;
            Chunks.Clear();
        }

        public byte[] CompletePayload()
        {
            if ( !IsComplete )
            {
                return null;
            }
            byte[] payload = new byte[TotalSize];
            foreach ( Chunk chunk in Chunks )
            {
                chunk.Data.CopyTo( payload, chunk.Offset );
            }
            return payload;
        }
    }
}