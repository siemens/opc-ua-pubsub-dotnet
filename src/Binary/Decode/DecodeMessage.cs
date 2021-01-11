// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using opc.ua.pubsub.dotnet.binary.Header;
using opc.ua.pubsub.dotnet.binary.Messages;
using opc.ua.pubsub.dotnet.binary.Messages.Chunk;
using opc.ua.pubsub.dotnet.binary.Messages.Delta;
using opc.ua.pubsub.dotnet.binary.Messages.KeepAlive;
using opc.ua.pubsub.dotnet.binary.Messages.Key;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using opc.ua.pubsub.dotnet.binary.Storage;
using log4net;

namespace opc.ua.pubsub.dotnet.binary.Decode
{
    public class DecodeMessage
    {
        public delegate void MessageDecodedEventHandler( object sender, MessageDecodedEventArgs eventArgs );
        private static readonly ILog Logger = LogManager.GetLogger( MethodBase.GetCurrentMethod()
                                                                              .DeclaringType
                                                                  );
        protected ChunkManager                 m_ChunkManager;
        protected LocalMetaStorage             m_LocalMetaStorage;
        protected ConcurrentQueue<MqttMessage> m_MessageQueue;
        private   bool                         m_Stop;
        public DecodeMessage() : this( null, new EncodingOptions() ) { }
        public DecodeMessage( EncodingOptions options ) : this( null, options ) { }

        public DecodeMessage( ConcurrentQueue<MqttMessage> messageQueue, EncodingOptions options )
        {
            m_MessageQueue     = messageQueue;
            m_LocalMetaStorage = new LocalMetaStorage();
            m_ChunkManager     = new ChunkManager();
            Options            = options;
        }

        private EncodingOptions                 Options { get; }
        public event MessageDecodedEventHandler MessageDecoded;

        /// <summary>
        ///     Parses a binary encoded OPC UA PubSub message.
        ///     In case a message was decoded successfully a <see cref="MessageDecoded" /> event is raised.
        /// </summary>
        /// <param name="payload">Binary payload which should be decoded.</param>
        /// <returns><see cref="NetworkMessage" />The decoded message or <c>null</c> if the decoding failed for any reason.</returns>
        public NetworkMessage ParseBinaryMessage( byte[] payload )
        {
            if ( payload == null )
            {
                Logger.Warn( "ParsePayload - payload == null" );
                return null;
            }
            if ( payload.Length == 0 )
            {
                Logger.Warn( "ParsePayload - payload.Length == 0" );
                return null;
            }
            NetworkMessage parsedMessage = null;
            using ( MemoryStream memoryStream = new MemoryStream( payload, false ) )
            {
                NetworkMessageHeader networkMessageHeader = NetworkMessageHeader.Decode( memoryStream );
                byte                 version              = networkMessageHeader.ProtocolVersion;
                if ( version != 1 )
                {
                    Logger.Warn( $"Not supported protocol version: {version}. Skipping message." );
                    return null;
                }
                if ( Logger.IsDebugEnabled )
                {
                    Logger.Debug( $"Message\t\t[{networkMessageHeader.ExtendedFlags2.MessageType}]\t\tfrom: {networkMessageHeader.PublisherID}" );
                }
                bool isChunkMessage = networkMessageHeader.ExtendedFlags2.Chunk;
                if ( Logger.IsDebugEnabled )
                {
                    Logger.Debug( $"Chunked Message: {isChunkMessage}" );
                }
                if ( isChunkMessage )
                {
                    ChunkedMessage chunkedMessage = ChunkedMessage.Decode( memoryStream );
                    if ( chunkedMessage == null )
                    {
                        Logger.Error( "Unable to parse chunked message." );
                    }
                    else
                    {
                        chunkedMessage.NetworkMessageHeader = networkMessageHeader;
                        if ( m_ChunkManager.Store( chunkedMessage ) )
                        {
                            if ( Logger.IsDebugEnabled )
                            {
                                Logger.Debug( "All chunks received. Starting decoding of message." );
                            }
                            byte[] completeMessage = m_ChunkManager.GetPayload( chunkedMessage );
                            using ( MemoryStream chunkedStream = new MemoryStream( completeMessage, false ) )
                            {
                                parsedMessage = GetSpecificMessage( networkMessageHeader, chunkedStream, chunkedMessage.PayloadHeader.DataSetWriterID );
                            }
                            if ( Logger.IsDebugEnabled )
                            {
                                Logger.Debug( parsedMessage != null ? "Chunked message successfully decoded." : "Failed to decode chunked message." );
                            }
                        }
                        else
                        {
                            if ( Logger.IsDebugEnabled )
                            {
                                Logger.Debug( "Not all chunked received yet. Cannot decode message." );
                            }
                            parsedMessage = chunkedMessage;
                        }
                    }
                }
                else
                {
                    parsedMessage = GetSpecificMessage( networkMessageHeader, memoryStream );
                }

                // Ensure that we have at least the Network Message Header available.
                if ( parsedMessage == null )
                {
                    parsedMessage = new NetworkMessage
                                    {
                                            NetworkMessageHeader = networkMessageHeader
                                    };
                }
                else
                {
                    if ( parsedMessage.NetworkMessageHeader == null )
                    {
                        parsedMessage.NetworkMessageHeader = networkMessageHeader;
                    }
                }

                // Because we might have modified the PublisherID,
                // the RawPayload is set to null to avoid inconsistencies.
                parsedMessage.RawPayload = null;
            }
            return parsedMessage;
        }

        public void Start()
        {
            if ( m_MessageQueue == null )
            {
                InvalidOperationException ex =
                        new InvalidOperationException( "This object has not been initialized with a ConcurrentQueue<MqttMessage> messageQueue. "
                                                     + "Before you can call Start() you have to provide the message queue."
                                                     );
                Logger.Error( "No message queue available. Unable to start.", ex );
                throw ex;
            }
            m_Stop = false;
            while ( !m_Stop )
            {
                MqttMessage msg;
                if ( m_MessageQueue.TryDequeue( out msg ) )
                {
                    if ( Logger.IsDebugEnabled )
                    {
                        Logger.Debug( "Starting to decode message..." );
                    }
                    Logger.Info( $"Topic: {msg.Topic}" );
                    NetworkMessage decodedMessage = null;
                    try
                    {
                        decodedMessage = ParseBinaryMessage( msg.Payload );
                    }
                    catch ( Exception e )
                    {
                        if ( Debugger.IsAttached )
                        {
                            Debugger.Break();
                        }
                        Logger.Error( $"Error while decoding message: {e}" );
                        Console.WriteLine( $"Error while decoding message: {e}" );
                    }
                    if ( decodedMessage != null )
                    {
                        MessageDecoded?.Invoke( this, new MessageDecodedEventArgs( decodedMessage, msg.Topic ) );
                    }
                }
                else
                {
                    Thread.Sleep( 10 );
                }
            }
        }

        public void Stop()
        {
            m_Stop = true;
        }

        /// <summary>
        ///     Returns a specific instance e.g. a MetaFrame, Key- or DeltaFrame.
        /// </summary>
        /// <param name="networkMessageHeader">The NetworkMessageHeader which was already parsed from the input stream.</param>
        /// <param name="inputStream">The input stream which should be parsed.</param>
        /// <param name="writerID">
        ///     Optionally the original WriterID. This is only required when a chunked DataFrame is parsed,
        ///     because it does not contain the real WriterID.
        /// </param>
        /// <returns>
        ///     Either a <c>MetaFrame</c>, <c>KeyFrame</c> or <c>DeltaFrame</c> if the message could be parsed successfully or
        ///     <c>null</c> if the binary message could not be parsed.
        /// </returns>
        private NetworkMessage GetSpecificMessage( NetworkMessageHeader networkMessageHeader, Stream inputStream, ushort? writerID = null )
        {
            NetworkMessage parsedMessage = null;
            switch ( networkMessageHeader.ExtendedFlags2.MessageType )
            {
                case MessageType.DiscoveryResponse:
                    Logger.Debug( "ParsePayload - Discovery Response" );
                    parsedMessage = ParseMetaFrame( inputStream, networkMessageHeader, Options );
                    break;

                case MessageType.DataSetMessage:
                    Logger.Debug( "ParsePayload - DataSetMessage" );
                    DataFrame dataFrame = null;
                    if ( writerID != null )
                    {
                        // If a writeID is present we have a chunked message,
                        // and we must not try to parse the Header, because there's no
                        // header in case of a chunked DataSetMessage.
                        dataFrame = new DataFrame();
                        if ( !DataFrame.DecodeChunk( inputStream, ref dataFrame ) )
                        {
                            Logger.Error( $"Unable to decode chunked DataFrame: {dataFrame}." );
                            return null;
                        }
                        dataFrame.PayloadHeader.DataSetWriterID = new[] { writerID.Value };
                    }
                    else
                    {
                        dataFrame = DataFrame.Decode( inputStream );
                        if ( dataFrame == null )
                        {
                            Logger.Error( $"Unable to decode DataFrame: {networkMessageHeader}." );
                            return null;
                        }
                    }
                    dataFrame.NetworkMessageHeader = networkMessageHeader;
                    parsedMessage                  = ParseDataSetFrame( inputStream, dataFrame );
                    break;

                default:
                    Logger.Warn( $"ParsePayload - Not supported message type: {networkMessageHeader.ExtendedFlags2.MessageType}" );
                    break;
            }
            return parsedMessage;
        }

        private DataFrame ParseDataSetFrame( Stream inputStream, DataFrame dataFrame )
        {
            if ( dataFrame == null )
            {
                return null;
            }
            ushort    writerID  = dataFrame.PayloadHeader.DataSetWriterID[0];
            MetaFrame metaFrame = null;
            if ( dataFrame.Flags2.DataSetMessageType != DataSetMessageTypeEnum.KeepAlive )
            {
                metaFrame = m_LocalMetaStorage.RetrieveMetaMessageLocally( dataFrame.NetworkMessageHeader.PublisherID.Value, writerID, dataFrame.ConfigurationVersion );
                if ( metaFrame == null )
                {
                    Logger.Warn( $"Could not find MetaMessage for {dataFrame.NetworkMessageHeader.PublisherID.Value} [{dataFrame.ConfigurationVersion}]" );
                    return dataFrame;
                }
            }
            switch ( dataFrame.Flags2.DataSetMessageType )
            {
                case DataSetMessageTypeEnum.DataKeyFrame:
                    if ( Logger.IsDebugEnabled )
                    {
                        Logger.Debug( "Starting decoding KeyFrame" );
                    }
                    KeyFrame keyFrame = KeyFrame.Decode( inputStream, dataFrame, metaFrame );
                    if ( keyFrame == null )
                    {
                        Logger.Error( "Unable to decode KeyFrame" );
                    }
                    if ( Logger.IsDebugEnabled && keyFrame != null )
                    {
                        Logger.Debug( keyFrame.ToString() );
                    }
                    return keyFrame;

                case DataSetMessageTypeEnum.DataDeltaFrame:
                    if ( Logger.IsDebugEnabled )
                    {
                        Logger.Debug( "Starting decoding DeltaFrame" );
                    }
                    DeltaFrame deltaFrame = DeltaFrame.Decode( inputStream, dataFrame, metaFrame );
                    if ( deltaFrame == null )
                    {
                        Logger.Error( "Unable to decode DeltaFrame" );
                    }
                    if ( Logger.IsDebugEnabled && deltaFrame != null )
                    {
                        Logger.Debug( deltaFrame.ToString() );
                    }
                    return deltaFrame;

                case DataSetMessageTypeEnum.KeepAlive:
                    if ( dataFrame.NetworkMessageHeader?.PublisherID != null )
                    {
                        if ( Logger.IsInfoEnabled )
                        {
                            Logger.Info( $"KeepAlive from {dataFrame.NetworkMessageHeader.PublisherID}" );
                        }
                        KeepAliveFrame keepAliveFrame = KeepAliveFrame.Decode( inputStream, dataFrame );
                        if ( keepAliveFrame == null )
                        {
                            Logger.Error( "Unable to decode keep alive frame" );
                        }
                        if ( Logger.IsDebugEnabled && keepAliveFrame != null )
                        {
                            Logger.Debug( keepAliveFrame.ToString() );
                        }
                    }
                    return dataFrame;
            }
            return null;
        }

        private MetaFrame ParseMetaFrame( Stream inputStream, NetworkMessageHeader networkMessageHeader, EncodingOptions options )
        {
            MetaFrame metaFrame = MetaFrame.Decode( inputStream, options );
            if ( metaFrame == null )
            {
                Logger.Error( "Could not parse Meta Message." );
            }
            if ( metaFrame != null )
            {
                metaFrame.NetworkMessageHeader = networkMessageHeader;
                m_LocalMetaStorage.StoreMetaMessageLocally( metaFrame );
                if ( Logger.IsDebugEnabled )
                {
                    Logger.Debug( $"Meta Message decoded with Configuration Version: {metaFrame.ConfigurationVersion}" );
                    Logger.Debug( metaFrame.ToString() );
                }
            }
            return metaFrame;
        }

        public struct MqttMessage
        {
            public string Topic;
            public byte[] Payload;
        }
    }
}