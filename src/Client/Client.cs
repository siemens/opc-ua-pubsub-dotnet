// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using opc.ua.pubsub.dotnet.binary;
using opc.ua.pubsub.dotnet.binary.DataPoints;
using opc.ua.pubsub.dotnet.binary.Decode;
using opc.ua.pubsub.dotnet.binary.Header;
using opc.ua.pubsub.dotnet.binary.Messages;
using opc.ua.pubsub.dotnet.binary.Messages.Key;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using opc.ua.pubsub.dotnet.binary.Messages.Meta.Structure;
using log4net;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.Protocol;
using opc.ua.pubsub.dotnet.client.Interfaces;
using opc.ua.pubsub.dotnet.client.common;
using opc.ua.pubsub.dotnet.client.common.Settings;
using OPCUAFile = opc.ua.pubsub.dotnet.binary.DataPoints.File;
using static opc.ua.pubsub.dotnet.client.ProcessDataSet;
using String = opc.ua.pubsub.dotnet.binary.String;
using System.Security.Authentication;
using System.Text;

[assembly: InternalsVisibleTo( "Client.Test" )]

namespace opc.ua.pubsub.dotnet.client
{
    internal class DataNotSentException : Exception
    {
        public DataNotSentException() { }
        public DataNotSentException( string message ) : base( message ) { }
        public DataNotSentException( string message, Exception innerException ) : base( message, innerException ) { }
    }

    /// <summary>
    ///     Data transfer object
    /// </summary>
    internal class TopicWildCardCharacterDto
    {
        /// <summary>
        ///     Position of character/string to be replaced
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        ///     value to be replaced
        /// </summary>
        public string Value { get; set; }
    }

    public class Client : IClientService
    {
        #region Fields

        private static readonly string StatusMessageOnline = "{ \"status\": \"online\" }";
        private static readonly string StatusMessageOffline = "{ \"status\": \"offline\" }";
        private static readonly string StatusMessageDisconnected = "{ \"status\": \"disconnected\" }";

        internal static         ushort                                     m_SequenceNumber;
        private                 IMqttClient                                m_MqttClient;
        private                 IMqttNetLogger                             m_MqttLogger;
        private static readonly ILog                                       Logger = LogManager.GetLogger( typeof(Client) );
        private                 ConcurrentQueue<DecodeMessage.MqttMessage> m_MessageQueue;
        private                 Task                                       m_DecoderTask;
        private                 DecodeMessage                              m_Decoder;

        #endregion

        #region Properties & Events

        public uint                                           ChunkSize { get; set; }
        public event DecodeMessage.MessageDecodedEventHandler MessageReceived;
        public event DecodeMessage.MessageDecodedEventHandler MetaMessageReceived;
        public event DecodeMessage.MessageDecodedEventHandler UnknownMessageReceived;
        public event FileReceivedEventHandler                 FileReceived;
        public event RawDataReceivedEventHandler              RawDataReceived;
        public event EventHandler<Exception>                  ExceptionCaught;
        public event EventHandler<string>                     ClientDisconnected;

        public EncodingOptions                                Options { get; }

        public Settings Settings { get; }

        public bool IsConnected
        {
            get
            {
                return m_MqttClient != null && m_MqttClient.IsConnected;
            }
        }

        public string                     ClientId                        { get; set; }
        public uint                       AutomaticKeyAndMetaSendInterval { get; set; }
        public Dictionary<uint, DateTime> LastKeyAndMetaSentTimes         { get; set; }
        /// <summary>
        ///     the broker CA certificate used for TLS handshake;
        ///     stored as byte array; derive an X509Certificate2 object when necessary
        /// </summary>
        public byte[] BrokerCACert { get; set; } = null;

        #endregion

        #region Construction / Connect / Cleanup

        public Client( Settings settings ) : this( settings, null, new EncodingOptions() ) { }
        public Client( Settings settings, string clientId ) : this( settings, clientId, new EncodingOptions() ) { }

        public Client( Settings settings, string clientId, EncodingOptions options )
        {
            ChunkSize                       = 0;
            Options                         = options;
            AutomaticKeyAndMetaSendInterval = 300;
            LastKeyAndMetaSentTimes         = new Dictionary<uint, DateTime>();
            Settings                        = settings;
            ClientId                        = clientId ?? $"Client_{Assembly.GetEntryAssembly()?.FullName.Split( ',' )[0]}_{Environment.MachineName}";

            if ( Settings.Client.EnableLogging )
            {
                //TODO: find out how this works now
                //MqttNetGlobalLogger.LogMessagePublished += OnLogMessagePublished;
            }
        }

        public void Connect( ClientCredentials credentials = null )
        {
            if ( credentials != null && !credentials.HasCertificates() )
            {
                Logger.Error( "No certificates imported" );
            }
            MqttClientOptionsBuilder optionsBuilder = CreateOptionsBuilder( credentials );

            if (Settings.Client.SendStatusMessages && !string.IsNullOrEmpty(Settings.Client.StatusMessageTopic))
            {
                string topic = CreateTopicName( Settings.Client.StatusMessageTopic, ClientId, 0, null, DataSetType.Event );
                optionsBuilder.WithWillTopic( topic );
                optionsBuilder.WithWillPayload( Encoding.UTF8.GetBytes( StatusMessageDisconnected ) );
                optionsBuilder.WithWillRetain( !Settings.Client.NeverSendRetain );
            }

            //TODO: improve logging...
            this.m_MqttLogger = new MqttNetNullLogger();

            m_MqttClient = new MqttFactory().CreateMqttClient(m_MqttLogger);
            m_MqttClient.ApplicationMessageReceivedAsync += MqttClientOnApplicationMessageReceived;
            m_MqttClient.DisconnectedAsync               += MqttClientOnDisconnected;
            MqttClientOptions options = optionsBuilder.Build();
            Logger.Debug( $"Waiting for connection ... (Timeout: {options.Timeout.TotalSeconds} s, MqttKeepAlive: {options.KeepAlivePeriod.TotalSeconds} s)" );
            m_MqttClient.ConnectAsync( options )
                        .Wait();

            if ( Settings.Client.SendStatusMessages && m_MqttClient.IsConnected )
            {
                Publish(
                    Encoding.UTF8.GetBytes( StatusMessageOnline ),
                    Settings.Client.StatusMessageTopic,
                    0,
                    null,
                    DataSetType.Event,
                    true );
            }

            //m_Logger.Debug($"Connecting finished. Result: {result.ResultCode}");
        }

        public void Disconnect()
        {
            if ( m_Decoder != null )
            {
                m_Decoder.MessageDecoded -= DecoderOnMessageDecoded;
                m_Decoder.Stop();
            }

            if ( m_MqttClient != null )
            {
                if ( m_MqttClient.IsConnected )
                {
                    if ( Settings.Client.SendStatusMessages )
                    {
                        Publish(
                            Encoding.UTF8.GetBytes( StatusMessageOffline ),
                            Settings.Client.StatusMessageTopic,
                            0,
                            null,
                            DataSetType.Event,
                            true );
                    }

                    //m_MqttClient.UnsubscribeAsync(Settings.Client.ClientCertP12).Wait();
                    m_MqttClient.ApplicationMessageReceivedAsync -= MqttClientOnApplicationMessageReceived;
                    m_MqttClient.DisconnectedAsync               -= MqttClientOnDisconnected;
                }

                try
                {
                    m_MqttClient.DisconnectAsync()
                                .Wait( TimeSpan.FromSeconds( 5 ) );
                }
                catch ( TaskCanceledException ) { } //don't know why this happens always...
                finally
                {
                    m_MqttClient.Dispose();
                }
            }

            m_DecoderTask?.Wait();
            m_MessageQueue = null;
            m_Decoder = null;
            
            if(m_DecoderTask != null)
            {
                m_DecoderTask.Dispose();
                m_DecoderTask = null;
            }

            m_MqttClient = null;

            ClientDisconnected?.Invoke( this, "" );
        }

        #endregion

        #region Subscriber
        public void Subscribe( string topic = null )
        {
            if ( !IsConnected )
            {
                Console.Error.WriteLine( "No connection for client: " + ClientId );
                throw new Exception( "No connection for client: " + ClientId );
            }
            if ( topic == null )
            {
                topic = Settings.Client.DefaultSubscribeTopicName;
            }

            if ( m_DecoderTask == null && !Settings.Client.RawDataMode)
            {
                m_MessageQueue = new ConcurrentQueue<DecodeMessage.MqttMessage>();
                m_Decoder = new DecodeMessage( m_MessageQueue, Options );
                m_Decoder.MessageDecoded += DecoderOnMessageDecoded;
                m_DecoderTask = Task.Run( () => m_Decoder.Start() );
            }


            var mqttSubscribeOptions = new MqttFactory().CreateSubscribeOptionsBuilder()
                .WithTopicFilter( f => { 
                    f.WithTopic( topic );
                    f.WithAtLeastOnceQoS();
                } )
                .Build();

            m_MqttClient.SubscribeAsync( mqttSubscribeOptions ).Wait();
                
            Logger.Debug( $"MQTT SubscribeAsync DONE, Topic: {topic}" );
        }

        private Task MqttClientOnDisconnected( MqttClientDisconnectedEventArgs e )
        {
            string msg = $"{e?.Reason}";
            string exceptionMessage = e?.Exception?.Message;

            if ( !string.IsNullOrEmpty( exceptionMessage ) )
            {
                msg += $"[{exceptionMessage}]";
            }

            ClientDisconnected?.Invoke( this, msg );
            return Task.CompletedTask;
        }

        private Task MqttClientOnApplicationMessageReceived( MqttApplicationMessageReceivedEventArgs e )
        {
            Logger.Debug( "OnMessage received. Enqueuing message..." );
            
            string topic = e.ApplicationMessage.Topic;
            byte[] payload = e.ApplicationMessage.Payload;

            if ( Settings.Client.RawDataMode )
            {
                RawDataReceived?.Invoke( this, new RawDataReceivedEventArgs( payload, topic ) );
            }
            else if ( m_MessageQueue == null )
            {
                Logger.Debug( "Message received without initialized queue!" );
            }
            else
            {
                m_MessageQueue.Enqueue( new DecodeMessage.MqttMessage
                                        {
                                                Topic   = topic,
                                                Payload = payload
                }
                                      );
            }
            return Task.CompletedTask;
        }

        private void DecoderOnMessageDecoded( object sender, MessageDecodedEventArgs eventArgs )
        {
            if ( eventArgs.Message != null )
            {
                try
                {
                    if ( eventArgs.Message is DataFrame dataFrame && dataFrame.Items != null )
                    {
                        DataPointValue[] files = dataFrame.Items.Where( i => i is OPCUAFile )
                                                          .ToArray();
                        foreach ( OPCUAFile file in files )
                        {
                            dataFrame.Items.Remove( file );
                            FileReceived?.Invoke( this, new FileReceivedEventArgs( file, eventArgs.Topic, dataFrame.NetworkMessageHeader.PublisherID.Value ) );
                        }
                        if ( dataFrame.Items.Count > 0 )
                        {
                            MessageReceived?.Invoke( this, eventArgs );
                        }
                    }
                    else if ( eventArgs.Message is MetaFrame metaFrame )
                    {
                        MetaMessageReceived?.Invoke( this, eventArgs );
                    }
                    else
                    {
                        UnknownMessageReceived?.Invoke( this, eventArgs );
                    }
                }
                catch ( Exception e )
                {
                    Console.WriteLine( $"Error in message received event handling for client: {ClientId}" );
                    Console.WriteLine( $"Exception: {e}" );
                    ForwardException( e );
                }
            }
        }

        #endregion

        #region Publisher

        public ProcessDataSet GenerateDataSet( string name, ushort writerId, DataSetType dataType )
        {
            return new ProcessDataSet( ClientId, name, writerId, dataType );
        }

        private DateTime GetLastKeyAndMetaSentTime( uint writerId )
        {
            DateTime lastKeyAndMetaSentTime;
            if ( !LastKeyAndMetaSentTimes.TryGetValue( writerId, out lastKeyAndMetaSentTime ) )
            {
                lastKeyAndMetaSentTime = DateTime.MinValue;
                LastKeyAndMetaSentTimes.Add( writerId, lastKeyAndMetaSentTime );
            }
            return lastKeyAndMetaSentTime;
        }

        private void UpdateLastKeyAndMetaSentTime( uint writerId )
        {
            LastKeyAndMetaSentTimes[writerId] = DateTime.UtcNow;
        }

        public void SendKeepAlive( ProcessDataSet dataSet, string topicPrefix )
        {
            if ( !IsConnected )
            {
                Console.Error.WriteLine( "No connection for client: " + ClientId );
                throw new Exception( "No connection for client: "     + ClientId );
            }
            DateTime lastKeyAndMetaSentTime = GetLastKeyAndMetaSentTime( dataSet.GetWriterId() );
            if ( dataSet.MetaDataUpToDate )
            {
                if ( ( DateTime.UtcNow - lastKeyAndMetaSentTime ).TotalSeconds > AutomaticKeyAndMetaSendInterval )
                {
                    SendDataSet( dataSet, false );
                }
                else
                {
                    byte[] keepAliveChunk = dataSet.GetEncodedKeepAliveMessage( m_SequenceNumber++ );
                    Publish( keepAliveChunk, topicPrefix, dataSet.GetWriterId(), "Meas", dataSet.GetDataSetType(), false );
                }
            }
        }

        public void SendKeepAlive( ProcessDataSet dataSet )
        {
            SendKeepAlive( dataSet, Settings.Client.DefaultPublisherTopicName );
        }

        public bool SendDataSet( ProcessDataSet dataSet, string topicPrefix, bool delta )
        {
            bool sent = false;
            if ( !IsConnected )
            {
                Console.Error.WriteLine( "No connection for client: " + ClientId );
                throw new Exception( "No connection for client: "     + ClientId );
            }
            DateTime lastKeyAndMetaSentTime = GetLastKeyAndMetaSentTime( dataSet.GetWriterId() );
            bool     sendMeta               = !dataSet.MetaDataUpToDate;
            bool     sendDelta              = delta && !sendMeta; //we can only send delta if meta frame is up to date
            if ( ( DateTime.UtcNow - lastKeyAndMetaSentTime ).TotalSeconds > AutomaticKeyAndMetaSendInterval )
            {
                sendDelta = false;
                sendMeta  = true;
            }
            try
            {
                if ( sendDelta )
                {
                    byte[] deltaChunk = dataSet.GetEncodedDeltaFrame( m_SequenceNumber++ );
                    Publish( deltaChunk, topicPrefix, dataSet.GetWriterId(), "Meas", dataSet.GetDataSetType(), false );
                }
                else //meta + key
                {
                    if ( sendMeta )
                    {
                        List<byte[]> metaChunks = dataSet.GetChunkedMetaFrame( ChunkSize, Options, m_SequenceNumber++ );
                        foreach ( byte[] chunk in metaChunks )
                        {
                            bool retain = ( metaChunks.Count == 1 ) && ( !Options.SendMetaMessageWithoutRetain );
                            Publish( chunk, topicPrefix, dataSet.GetWriterId(), "Meta", dataSet.GetDataSetType(), retain );
                            UpdateLastKeyAndMetaSentTime( dataSet.GetWriterId() );
                        }
                    }
                    List<byte[]> keyChunks = dataSet.GetChunkedKeyFrame( ChunkSize, m_SequenceNumber++ );
                    foreach ( byte[] chunk in keyChunks )
                    {
                        Publish( chunk, topicPrefix, dataSet.GetWriterId(), "Meas", dataSet.GetDataSetType(), false );
                    }
                }

                //sent is only true if all chunks are sent without exception
                sent = true;
            }
            catch ( DataNotSentException ) { }
            return sent;
        }

        public bool SendDataSet( ProcessDataSet dataSet, bool delta )
        {
            string topic = Settings.Client.DefaultPublisherTopicName;
            return SendDataSet( dataSet, topic, delta );
        }

        public bool SendRawData( byte[] payload, string topic,bool retain )
        {
            bool sent = false;
            try
            {
                Publish( payload, topic, 0, null, DataSetType.Event, retain );                
                sent = true; //sent is only true if all chunks are sent without exception
            }
            catch ( DataNotSentException ) { }
            return sent;
        }

        private MetaFrame CreateFileMetaFrame( OPCUAFile file, ushort writerId )
        {
            MetaFrame metaFrame = new MetaFrame( Options );
            ExtendedFlags1 extendedFlags1 = new ExtendedFlags1
                                            {
                                                    RawValue = 0x84
                                            };
            ExtendedFlags2 extendedFlags2 = new ExtendedFlags2
                                            {
                                                    RawValue = 0x08
                                            };
            metaFrame.NetworkMessageHeader = new NetworkMessageHeader
                                             {
                                                     VersionAndFlags = 0x91,
                                                     ExtendedFlags1  = extendedFlags1,
                                                     ExtendedFlags2  = extendedFlags2,
                                                     PublisherID     = new String( ClientId )
                                             };
            metaFrame.SequenceNumber = m_SequenceNumber++;
            DateTime time = DateTime.UtcNow;
            metaFrame.ConfigurationVersion.Minor = (uint)( time.Ticks & uint.MaxValue );
            metaFrame.ConfigurationVersion.Major = (uint)( time.Ticks >> 32 );
            metaFrame.Name                       = new String( "FileDataset" );
            metaFrame.DataSetWriterID            = writerId;
            metaFrame.FieldMetaDataList          = new List<FieldMetaData>();
            metaFrame.StructureDataTypes         = new Dictionary<NodeID, StructureDescription>();
            metaFrame.Namespaces = new List<String>( 3 )
                                   {
                                           new String(),
                                           new String( "http://siemens.com/energy/schema/opcua/ps/v2" ),
                                           new String( "https://mindsphere.io/OPCUAPubSub/v3" )
                                   };
            FieldMetaData fieldMetaData = new FieldMetaData (Options)
                                          {
                                                  Name     = new String( file.Name ),
                                                  DataType = OPCUAFile.PreDefinedNodeID
                                          };
            metaFrame.FieldMetaDataList.Add( fieldMetaData );
            metaFrame.StructureDataTypes[fieldMetaData.DataType] = file.StructureDescription;
            metaFrame.EnumDataTypes                              = new Dictionary<NodeID, EnumDescription>();
            return metaFrame;
        }

        private KeyFrame CreateFileKeyFrame( MetaFrame metaFrame, OPCUAFile file )
        {
            KeyFrame key = new KeyFrame( Options );
            key.ConfigurationVersion = metaFrame.ConfigurationVersion;
            key.NetworkMessageHeader = new NetworkMessageHeader
                                       {
                                               PublisherID     = new String( ClientId ),
                                               VersionAndFlags = 0xD1,
                                               ExtendedFlags1 = new ExtendedFlags1
                                                                {
                                                                        RawValue = 0x04
                                                                }
                                       };
            key.DataSetMessageSequenceNumber = m_SequenceNumber++;
            key.Flags1.RawValue              = 0xEB; // EBh = 1110 1011b

            // Bit 0 = 1: Data set is valid
            // Bit 1-2 = 01: RawData Field encoding
            // Bit 3 = 1: DataSetMessageSequenceNumber enabled
            // Bit 5-6 = 11: Minor and Major version enabled
            // Bit 7 = 1: DataSetFlags2 enabled
            // without Flags2 and without timestamp enabled: Flags1 = 0x6B;
            key.Flags2.RawValue = 0x10; // Bit 0-3 = 0000: Data Key Frame

            // Bit 4 = 1: Timestamp (in DataSetMessageHeader) enabled
            key.Items = new List<DataPointValue>
                        {
                                file
                        };
            key.MetaFrame = metaFrame;
            key.Timestamp = DateTime.FromFileTimeUtc( file.Time );
            key.PayloadHeader = new DataSetPayloadHeader
                                {
                                        Count           = 1,
                                        DataSetWriterID = new[] { metaFrame.DataSetWriterID }
                                };
            return key;
        }

        public bool SendFile( OPCUAFile file, ushort writerId )
        {
            string topic = Settings.Client.DefaultPublisherTopicName;
            return SendFile( file, topic, writerId );
        }

        public bool SendFile( OPCUAFile file, string topicPrefix, ushort writerId )
        {
            bool fileSent = false;
            if ( !IsConnected )
            {
                Console.Error.WriteLine( "No connection for client: " + ClientId );
                throw new Exception( "No connection for client: "     + ClientId );
            }
            try
            {
                byte[]    metaFrameBytes = null;
                byte[]    keyFrameBytes  = null;
                MetaFrame metaFrame      = CreateFileMetaFrame( file, writerId );
                KeyFrame  keyFrame       = CreateFileKeyFrame( metaFrame, file );
                using ( MemoryStream outputStream = new MemoryStream() )
                {
                    metaFrame.Encode( outputStream );
                    metaFrameBytes = outputStream.ToArray();
                }
                using ( MemoryStream outputStream = new MemoryStream() )
                {
                    keyFrame.Encode( outputStream );
                    keyFrameBytes = outputStream.ToArray();
                }
                try
                {
                    Publish( metaFrameBytes, topicPrefix, writerId, "Meta", DataSetType.TimeSeriesEventFile, false );
                    Publish( keyFrameBytes,  topicPrefix, writerId, "File", DataSetType.TimeSeriesEventFile, false );
                    fileSent = true;
                }
                catch ( DataNotSentException ) { }
            }
            catch ( Exception e )
            {
                Logger.Error( "SendFile failed! " + e );
                ForwardException( e );
            }
            return fileSent;
        }

        /// <summary>
        ///     get postion of the string to be replaced and value
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="index"></param>
        /// <param name="dataSetWriterID"></param>
        /// <param name="wildCharLength"></param>
        /// <returns></returns>
        private static TopicWildCardCharacterDto GetWildCharReplacementString( string topicName, int index, ushort dataSetWriterID, int wildCharLength )
        {
            string value = string.Empty;
            if ( index != -1 )
            {
                int endWildChar = topicName.IndexOf( "|", index, StringComparison.InvariantCulture );
                if ( endWildChar == -1 )
                {
                    endWildChar = topicName.IndexOf( "}", index, StringComparison.InvariantCulture );
                }
                int start = index + wildCharLength;
                value = topicName.Substring( start, endWildChar - start );
            }
            else
            {
                value = dataSetWriterID.ToString( CultureInfo.InvariantCulture );
                index = topicName.IndexOf( "{", StringComparison.InvariantCulture );
            }
            TopicWildCardCharacterDto wildCardChar = new TopicWildCardCharacterDto
                                                     {
                                                             Index = index,
                                                             Value = value
                                                     };
            return wildCardChar;
        }

        /// <summary>
        ///     Get value to be replaced for meta data
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="dataSetWriterID"></param>
        /// <returns></returns>
        private static TopicWildCardCharacterDto GetMeta( string topicName, ushort dataSetWriterID, DataSetType dataSetType )
        {
            int metaIndex      = -1;
            int wildCharLength = 0;
            if ( dataSetType == DataSetType.TimeSeries )
            {
                metaIndex      = topicName.IndexOf( "TM:", StringComparison.InvariantCulture );
                wildCharLength = 3;
                if ( metaIndex == -1 )
                {
                    metaIndex      = topicName.IndexOf( "Meta:", StringComparison.InvariantCulture );
                    wildCharLength = 5;
                }
            }
            else if ( dataSetType == DataSetType.Event )
            {
                metaIndex      = topicName.IndexOf( "EM:", StringComparison.InvariantCulture );
                wildCharLength = 3;
            }
            else if ( dataSetType == DataSetType.TimeSeriesEventFile )
            {
                metaIndex      = topicName.IndexOf( "FM:", StringComparison.InvariantCulture );
                wildCharLength = 3;
            }
            return GetWildCharReplacementString( topicName, metaIndex, dataSetWriterID, wildCharLength );
        }

        /// <summary>
        ///     Get value to be replaced for meas data
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="dataSetWriterID"></param>
        /// <returns></returns>
        private static TopicWildCardCharacterDto GetMeas( string topicName, ushort dataSetWriterID, DataSetType dataSetType )
        {
            int measIndex      = -1;
            int wildCharLength = 0;
            if ( dataSetType == DataSetType.TimeSeries )
            {
                measIndex      = topicName.IndexOf( "T:", StringComparison.InvariantCulture );
                wildCharLength = 2;
                if ( measIndex == -1 )
                {
                    measIndex      = topicName.IndexOf( "Meas:", StringComparison.InvariantCulture );
                    wildCharLength = 5;
                }
            }
            else if ( dataSetType == DataSetType.Event )
            {
                measIndex      = topicName.IndexOf( "E:", StringComparison.InvariantCulture );
                wildCharLength = 2;
            }
            return GetWildCharReplacementString( topicName, measIndex, dataSetWriterID, wildCharLength );
        }

        /// <summary>
        ///     Get value to be replaced for file data
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="dataSetWriterID"></param>
        /// <returns></returns>
        private static TopicWildCardCharacterDto GetFile( string topicName, ushort dataSetWriterID, DataSetType dataSetType )
        {
            int fileIndex      = -1;
            int wildCharLength = 0;
            if ( dataSetType == DataSetType.TimeSeriesEventFile )
            {
                fileIndex      = topicName.IndexOf( "F:", StringComparison.InvariantCulture );
                wildCharLength = 2;
                if ( fileIndex == -1 )
                {
                    fileIndex      = topicName.IndexOf( "File:", StringComparison.InvariantCulture );
                    wildCharLength = 5;
                }
            }
            return GetWildCharReplacementString( topicName, fileIndex, dataSetWriterID, wildCharLength );
        }

        protected internal static string CreateTopicName( string      topicPrefix,
                                                          string      publisherID,
                                                          ushort      dataSetWriterID,
                                                          string      messageType,
                                                          DataSetType dataSetType
                )
        {
            string topicName    = topicPrefix;
            string newTopicName = string.Empty;

            // for backward compatibility we wont manipulate the topic name if i didn't find {ClientID} in it
            if ( !topicName.Contains( "{", StringComparison.InvariantCulture ) )
            {
                // default topic name : /siemens/uapubsub
                //topicName = $"{topicPrefix}/{publisherID}/{dataSetWriterID}";
                return topicName;
            }
            topicName = topicName.Replace( "{ClientID}",  publisherID, StringComparison.InvariantCultureIgnoreCase );
            topicName = topicName.Replace( "{VersionMS}", "v3", StringComparison.InvariantCultureIgnoreCase );

            TopicWildCardCharacterDto wildCardCharDto = null;
            int                       startIndex      = 0;
            while ( true )
            {
                if ( startIndex > topicName.Length )
                {
                    break;
                }
                int curlyBraceStart = topicName.IndexOf( "/{", startIndex, StringComparison.InvariantCulture );
                if ( curlyBraceStart == -1 )
                {
                    // none or only "{ClientID}" and/or "{VersionMS}" are used
                    // in topic name pattern
                    return topicName;
                }
                int curlyBraceEnd = topicName.IndexOf( "}/", startIndex, StringComparison.InvariantCulture );
                if ( curlyBraceEnd == -1 ) // end of topic name
                {
                    curlyBraceEnd = topicName.LastIndexOf( "}", StringComparison.InvariantCulture );
                }
                newTopicName = string.Concat( newTopicName, topicName.Substring( startIndex, curlyBraceStart - startIndex ) );
                string stringToReplace = topicName.Substring( curlyBraceStart + 1, curlyBraceEnd - curlyBraceStart );
                newTopicName = string.Concat( newTopicName, "/", stringToReplace, "/" );
                if ( messageType == "Meta" )
                {
                    wildCardCharDto = GetMeta( newTopicName, dataSetWriterID, dataSetType );
                }
                else if ( messageType == "Meas" )
                {
                    wildCardCharDto = GetMeas( newTopicName, dataSetWriterID, dataSetType );
                }
                else if ( messageType == "File" )
                {
                    wildCardCharDto = GetFile( newTopicName, dataSetWriterID, dataSetType );
                }
                if ( wildCardCharDto != null )
                {
                    newTopicName = newTopicName.Replace( stringToReplace, wildCardCharDto.Value, StringComparison.InvariantCulture );
                }
                startIndex = curlyBraceEnd + 2;
            }

            // remove extra "/" that was added
            newTopicName = newTopicName.Substring( 0, newTopicName.Length - 1 );
            return newTopicName;
        }

        private void Publish( byte[] payload, string topicPrefix, ushort dataSetWriterId, string messageType, DataSetType dataSetType, bool retain )
        {
            string topic = CreateTopicName( topicPrefix, ClientId, dataSetWriterId, messageType, dataSetType );

            MqttApplicationMessageBuilder messageBuilder;
            messageBuilder = new MqttApplicationMessageBuilder()
                            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                            .WithPayload(payload)
                            .WithTopic(topic)
                            .WithRetainFlag(retain && !Settings.Client.NeverSendRetain);
            bool dataSent = false;

            //try twice in case of timeout
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    m_MqttClient.PublishAsync(messageBuilder.Build())
                                .Wait();

                    //successfully sent - quit the retry loop
                    dataSent = true;
                    break;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Error while {i + 1}. publishing message via MQTT for client: " + ClientId);
                    Console.Error.WriteLine("Exception:" + e);
                    Logger.Error("Error while publishing message via MQTT for client: " + ClientId);
                    Logger.Error("Exception:" + e);
                    ForwardException(e);

                    //try again (doesnt't make sense for all errors, 
                    //but due to the AggregatedExceptions it's very difficult to find out what really went wrong...)
                }
            }
            if (!dataSent)
            {
                throw new DataNotSentException();
            }
        }

        #endregion

        #region Certificate handling

        /// <summary>
        ///     According to the implementation of MQTTNet this callback is similar to the one used by the SSLStream class.
        ///     https://docs.microsoft.com/en-us/dotnet/api/system.net.security.remotecertificatevalidationcallback
        /// </summary>
        /// <param name="callbackContext">
        ///     from MQTTnet V3.0.10 a class containing the fct. argument that were separated so far:
        ///     <list type="bullet">
        ///         <item>
        ///             <term>
        ///                 <see cref="X509Certificate">Certificate</see>
        ///             </term>
        ///             <desccription>used to authenticate the remote party.</desccription>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 <see cref="X509Chain">Chain</see>
        ///             </term>
        ///             <desccription>The chain of certificate authorities associated with the remote certificate.</desccription>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 <see cref="SslPolicyErrors">SSLPolicyErrors</see>
        ///             </term>
        ///             <desccription>One or more errors associated with the remote certificate.</desccription>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 <see cref="IMqttClientChannelOptions">ClientOptions</see>
        ///             </term>
        ///             <desccription>MQTTClient options which were used to establish the connection.</desccription>
        ///         </item>
        ///     </list>
        /// </param>
        /// <returns>A Boolean value that determines whether the specified certificate is accepted for authentication.</returns>
        private bool CertificateValidationCallback( MqttClientCertificateValidationEventArgs eventArgs )
        {
            // a broker CA certificate must exist
            if ( BrokerCACert == null )
            {
                return false;
            }

            // get the broker CA certificate for validation 
            using ( X509Certificate2 brokerCACert = new X509Certificate2( BrokerCACert, "", X509KeyStorageFlags.Exportable ) )
            {
                // the certificate received from broker during TLS handshake
                using ( X509Certificate2 brokerCert = new X509Certificate2( eventArgs.Certificate ) )
                {
                    // the validation which was made base on the certificates stored in the certificate store 
                    // was successful
                    if ( eventArgs.SslPolicyErrors == SslPolicyErrors.None )
                    {
                        // that means that all CA certificates that are required for validation check are entered in the
                        // "trusted CA" part of the certificate store and match with the certtificate/chain from remote

                        // it seems that also e.g. in the docker container the cert store of the OS is used for 
                        // validation; but we only want to validate against the broker CA that was selected
                        // during configuration of the device; so we only make our own validations here
                        // (see below) also if policyErrors is none
                        //return true;
                    }

                    // further validation checks:

                    // a certificate chain is received and it is valid
                    if ( eventArgs.Chain != null && eventArgs.Chain.ChainElements.Count > 1 )
                    {
                        // we check whether the broker CA certificate we have stored is part of this chain
                        foreach ( X509ChainElement cert in eventArgs.Chain.ChainElements )
                        {
                            X509Certificate2 certInChain = cert.Certificate;
                            if ( brokerCACert.Equals( certInChain ) )
                            {
                                // we accept as valid
                                return true;
                            }
                        }
                    }

                    // for the next check we build a certificate chain including the received certificate from 
                    // remote and the broker CA we have stored and check this chain for validitiy
                    using ( X509Chain testChain = new X509Chain() )
                    {
                        testChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                        testChain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
                        testChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                        testChain.ChainPolicy.VerificationTime = DateTime.Now;
                        testChain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan( 0, 0, 0 );

                        // the CA certificate we want to test whether it is the root from the broker cert
                        testChain.ChainPolicy.ExtraStore.Add( brokerCACert );
                        if ( testChain.Build( brokerCert ) )
                        {
                            // the .Contains check below is added because the .AllowUnknownCertificateAuthority results all in true
                            // if the ExtraStore.Add adds nothing or empty
                            if ( testChain.ChainStatus.Length == 1
                              && testChain.ChainStatus.First()
                                          .Status
                              == X509ChainStatusFlags.UntrustedRoot
                              && testChain.ChainPolicy.ExtraStore.Contains( testChain.ChainElements[testChain.ChainElements.Count - 1]
                                                                                     .Certificate
                                                                          ) )
                            {
                                // chain is valid,
                                // and we expect that root is untrusted which the status flag tells us
                                return true;
                            }
                        }
                    }
                }
            }

            // the validation check errors shall be ignored by setting
            if ( eventArgs.ClientOptions.TlsOptions.IgnoreCertificateChainErrors )
            {
                Logger.Error( "Ignoring broker certificate validation errors." );
                return true;
            }

            // not valid
            Logger.Error( "Broker certificate validation failed" );
            if ( Logger.IsDebugEnabled )
            {
                try
                {
                    Logger.Error( "Remote Certificate:" );
                    using ( X509Certificate2 certificate = new X509Certificate2( eventArgs.Certificate ) )
                    {
                        CertifacteLogging.LogCertifacte( certificate, Logger );
                    }
                    Logger.Error( "Remote Certificate Chain:" );
                    CertifacteLogging.LogCertificateChain( eventArgs.Chain, Logger );
                }
                catch ( Exception ex )
                {
                    Logger.Error( "Exception while logging certificate details.", ex );
                }
            }
            return false;
        }

        private MqttClientOptionsBuilder CreateOptionsBuilder( ClientCredentials credentials = null )
        {
            MqttClientOptionsBuilder clientOptionsBuilder = new MqttClientOptionsBuilder();
            MqttClientOptionsBuilderTlsParameters tlsParameters = null;
            string hostName = Settings.Client.BrokerHostname;
            int portNum = Settings.Client.BrokerPort;

            //check if broker endpoint for local connections is defined in environment, only possible for connections without credentials
            if ( credentials == null )
            {
                string brokerEndpoint = Environment.GetEnvironmentVariable( "GE_BROKER_CONNECTION_ENDPOINT" );
                if ( !string.IsNullOrEmpty( brokerEndpoint ) )
                {
                    string[] tokens = brokerEndpoint.Split( ':' );
                    if ( tokens.Length == 2 )
                    {
                        hostName = tokens[0];
                        portNum = Convert.ToInt32( tokens[1], CultureInfo.InvariantCulture );
                    }
                }
            }
            clientOptionsBuilder.WithCleanSession();
            clientOptionsBuilder.WithClientId( ClientId );
            if ( portNum == 443 )
            {
                clientOptionsBuilder.WithWebSocketServer( hostName );
            }
            else
            {
                clientOptionsBuilder.WithTcpServer( hostName, portNum );
            }
            if ( credentials != null )
            {
                if ( credentials.HasCertificates() )
                {
                    tlsParameters = new MqttClientOptionsBuilderTlsParameters
                    {
                        UseTls = true,
                        AllowUntrustedCertificates = Settings.Client.AllowUntrustedCertificates,
                        IgnoreCertificateChainErrors = Settings.Client.IgnoreCertificateChainErrors,
                        IgnoreCertificateRevocationErrors = Settings.Client.IgnoreCertificateRevocationErrors,
                        CertificateValidationHandler = CertificateValidationCallback,
                        Certificates = credentials.ClientCertAndCaChain,
                        SslProtocol = SslProtocols.Tls12
                    };
                    clientOptionsBuilder.WithTls( tlsParameters );
                }
                if ( credentials.IsUserNameAndPasswordRequired() )
                {
                    credentials.GetUserNameAndPassword( ClientId, out string username, out string password );
                    clientOptionsBuilder.WithCredentials( username, password );
                }
            }

            if ( !string.IsNullOrEmpty(Settings.Client.ProxyAddress) && !string.IsNullOrEmpty(Settings.Client.ProxyPort))
            {
                string address = Settings.Client.ProxyAddress + ":" + Settings.Client.ProxyPort;
                clientOptionsBuilder.WithProxy(address, Settings.Client.ProxyUsername, Settings.Client.ProxyPassword.ToString() );
            }

            // settings for connection timeout and MQTT kepp alive interval, given in seconds
            // (defaults in MQTTnet stack are CommunicationTimeout = 10 sec and KeepAlivePeriod = 15 sec.,
            //  see in MqttClientOptions.cs of MQTTnet)
            clientOptionsBuilder.WithTimeout( new TimeSpan( 0, 0, Settings.Client.CommunicationTimeout ) );
            clientOptionsBuilder.WithKeepAlivePeriod( new TimeSpan( 0, 0, Settings.Client.MqttKeepAlivePeriod ) );
            return clientOptionsBuilder;
        }

        private void ForwardException( Exception ex )
        {
            if ( ex is AggregateException ae )
            {
                foreach ( Exception ie in ae.InnerExceptions )
                {
                    ExceptionCaught?.Invoke( this, ie );
                }
            }
            else
            {
                ExceptionCaught?.Invoke( this, ex );
            }
        }

        private static void OnLogMessagePublished( object sender, MqttNetLogMessagePublishedEventArgs eventArgs )
        {
            switch ( eventArgs.LogMessage.Level )
            {
                case MqttNetLogLevel.Error:
                    Logger.Error( eventArgs.LogMessage.Message, eventArgs.LogMessage.Exception );
                    break;

                case MqttNetLogLevel.Info:
                    Logger.Info( eventArgs.LogMessage.Message, eventArgs.LogMessage.Exception );
                    break;

                case MqttNetLogLevel.Warning:
                    Logger.Warn( eventArgs.LogMessage.Message, eventArgs.LogMessage.Exception );
                    break;

                case MqttNetLogLevel.Verbose:
                    Logger.Debug( eventArgs.LogMessage.Message, eventArgs.LogMessage.Exception );
                    break;

                default:
                    Logger.Fatal( eventArgs.LogMessage.Message, eventArgs.LogMessage.Exception );
                    break;
            }
        }

        #endregion 
    }
}