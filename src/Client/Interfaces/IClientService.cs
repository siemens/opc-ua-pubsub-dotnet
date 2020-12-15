// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using Binary;
using Binary.Decode;
using opc.ua.pubsub.dotnet.client.common.Settings;
using OPCUAFile = Binary.DataPoints.File;
using static opc.ua.pubsub.dotnet.client.ProcessDataSet;

namespace opc.ua.pubsub.dotnet.client.Interfaces
{
    public class FileReceivedEventArgs : EventArgs
    {
        public FileReceivedEventArgs( OPCUAFile file, string topic, string publisherId )
        {
            File        = file;
            Topic       = topic;
            PublisherId = publisherId;
        }

        public OPCUAFile File        { get; set; }
        public string    PublisherId { get; set; }
        public string    Topic       { get; set; }
    }

    public delegate void FileReceivedEventHandler( object sender, FileReceivedEventArgs eventArgs );

    public interface IClientService
    {
        uint                                           AutomaticKeyAndMetaSendInterval { get; set; }
        byte[]                                         BrokerCACert                    { get; set; }
        uint                                           ChunkSize                       { get; set; }
        string                                         ClientId                        { get; set; }
        bool                                           IsConnected                     { get; }
        Dictionary<uint, DateTime>                     LastKeyAndMetaSentTimes         { get; set; }
        EncodingOptions                                Options                         { get; }
        Settings                                       Settings                        { get; }
        event EventHandler<string>                     ClientDisconnected;
        event EventHandler<Exception>                  ExceptionCaught;
        event FileReceivedEventHandler                 FileReceived;
        event DecodeMessage.MessageDecodedEventHandler MessageReceived;
        event DecodeMessage.MessageDecodedEventHandler MetaMessageReceived;
        event DecodeMessage.MessageDecodedEventHandler UnknownMessageReceived;
        void                                           Connect( ClientCredentials credentials = null );
        void                                           Disconnect();
        ProcessDataSet                                 GenerateDataSet( string       name,    ushort writerId,             DataSetType dataType );
        bool                                           SendDataSet( ProcessDataSet   dataSet, string m_TopicConfigRequest, bool        delta );
        bool                                           SendDataSet( ProcessDataSet   dataSet, bool   delta );
        bool                                           SendFile( OPCUAFile           file,    ushort writerId );
        bool                                           SendFile( OPCUAFile           file,    string topicPrefix, ushort writerId );
        void                                           SendKeepAlive( ProcessDataSet dataSet );
        void                                           Subscribe( string             topic = null );
    }
}