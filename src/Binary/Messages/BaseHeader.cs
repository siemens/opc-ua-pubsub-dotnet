// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.IO;

namespace opc.ua.pubsub.dotnet.binary.Messages
{
    public abstract class BaseHeader
    {
        public          ushort[] DataSetWriterID { get; set; }
        public abstract void     Encode( Stream outputStream );
    }
}