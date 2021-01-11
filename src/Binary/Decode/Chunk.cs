// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Collections.Generic;

namespace opc.ua.pubsub.dotnet.binary.Decode
{
    public class Chunk
    {
        public byte[] Data   { get; set; }
        public uint   Offset { get; set; }
    }

    public class ChunkComparer : Comparer<Chunk>
    {
        public override int Compare( Chunk x, Chunk y )
        {
            if ( x == null && y == null )
            {
                return 0;
            }
            if ( x == null )
            {
                return -1;
            }
            if ( y == null )
            {
                return 1;
            }
            if ( x.Offset == y.Offset )
            {
                return 0;
            }
            if ( x.Offset < y.Offset )
            {
                return -1;
            }
            return 1;
        }
    }
}