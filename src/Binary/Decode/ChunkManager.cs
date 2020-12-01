// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Reflection;
using Binary.Messages.Chunk;
using log4net;

namespace Binary.Decode
{
    public class ChunkManager
    {
        private static readonly ILog Logger = LogManager.GetLogger( MethodBase.GetCurrentMethod()
                                                                              .DeclaringType
                                                                  );
        private readonly Dictionary<string, Dictionary<ushort, Dictionary<ushort, ChunkStorage>>> m_Storage;

        public ChunkManager()
        {
            m_Storage = new Dictionary<string, Dictionary<ushort, Dictionary<ushort, ChunkStorage>>>();
        }

        public byte[] GetPayload( string publisherID, ushort writerID, ushort sequenceNumber, bool clearAfterRetrieval = true )
        {
            Dictionary<ushort, ChunkStorage> sequenceStorage = GetSequenceStorage( publisherID, writerID );
            if ( !sequenceStorage.TryGetValue( sequenceNumber, out ChunkStorage storage ) )
            {
                return null;
            }
            byte[] payload = storage.CompletePayload();
            if ( clearAfterRetrieval )
            {
                storage.Clear();
                sequenceStorage.Remove( sequenceNumber );
            }
            return payload;
        }

        public byte[] GetPayload( ChunkedMessage message, bool clearAfterRetrieval = true )
        {
            if ( message == null )
            {
                return null;
            }
            return GetPayload( message.NetworkMessageHeader.PublisherID.Value,
                               message.PayloadHeader.DataSetWriterID,
                               message.MessageSequenceNumber,
                               clearAfterRetrieval
                             );
        }

        /// <summary>
        ///     Store a chunk for the given Chunked Network Message.
        ///     The method returns true, if the provided chunk was the last missing element.
        ///     Otherwise false is returned.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>true if all chunks are available, otherwise false.</returns>
        public bool Store( ChunkedMessage message )
        {
            string                           publisherID     = message.NetworkMessageHeader.PublisherID.Value;
            ushort                           writerID        = message.PayloadHeader.DataSetWriterID;
            ushort                           sequenceNumber  = message.MessageSequenceNumber;
            Dictionary<ushort, ChunkStorage> sequenceStorage = GetSequenceStorage( publisherID, writerID );
            if ( !sequenceStorage.TryGetValue( sequenceNumber, out ChunkStorage storage ) )
            {
                storage = new ChunkStorage
                          {
                                  TotalSize       = message.TotalSize,
                                  PublisherID     = publisherID,
                                  DataSetWriterID = sequenceNumber
                          };
                sequenceStorage.Add( sequenceNumber, storage );
            }
            if ( storage.TotalSize != message.TotalSize )
            {
                string text =
                        $"TotalSize mismatch for Publisher {publisherID} and Message Sequence {message.MessageSequenceNumber}: previous TotalSize: {storage.TotalSize} != {message.TotalSize}";
                Logger.Error( text );
                throw new ApplicationException( text );
            }
            if ( Logger.IsDebugEnabled )
            {
                Logger.Debug( $"Storing chunk from Publisher {publisherID} with WriterID {writerID} and Sequence Number {sequenceNumber}. OffSet: {message.ChunkOffset}, TotalSize: {message.TotalSize}, ChunkSize: {message.ChunkData.Length}"
                            );
            }
            Chunk newChunk = new Chunk
                             {
                                     Offset = message.ChunkOffset,
                                     Data   = message.ChunkData
                             };
            return storage.Add( newChunk );
        }

        private Dictionary<ushort, ChunkStorage> GetSequenceStorage( string publisherID, ushort writerID )
        {
            if ( !m_Storage.TryGetValue( publisherID, out Dictionary<ushort, Dictionary<ushort, ChunkStorage>> dataSetWriters ) )
            {
                dataSetWriters = new Dictionary<ushort, Dictionary<ushort, ChunkStorage>>();
                m_Storage.Add( publisherID, dataSetWriters );
            }
            if ( !dataSetWriters.TryGetValue( writerID, out Dictionary<ushort, ChunkStorage> sequenceStorage ) )
            {
                sequenceStorage = new Dictionary<ushort, ChunkStorage>();
                dataSetWriters.Add( writerID, sequenceStorage );
            }
            return sequenceStorage;
        }
    }
}