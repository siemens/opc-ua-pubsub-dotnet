// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.IO;

namespace Binary
{
    /// <summary>
    ///     This interface is implemented by every entity which can be encoded and decode to or from a OPC UA PubSub binary
    ///     stream.
    /// </summary>
    public interface ICodable<out T>
    {
        EncodingOptions Options { get; }
        void            Encode( Stream outputStream, bool withHeader = true );

        //T Decode(Stream inputStream);
    }
}