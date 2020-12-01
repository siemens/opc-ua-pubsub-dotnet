// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Reflection;
using Binary;
using Binary.DataPoints;
using Binary.Decode;
using opc.ua.pubsub.dotnet.client.Interfaces;
using opc.ua.pubsub.dotnet.common.Settings;
using static opc.ua.pubsub.dotnet.client.ProcessDataSet;

namespace opc.ua.pubsub.dotnet.client
{
    public class TestClient : IClientService, ITestClient
    {
        public TestClient( Settings settings, string clientId = null )
        {
            Settings = settings;
            ClientId = clientId;
            if ( ClientId == null )
            {
                ClientId =
                        $"Client_{Assembly.GetEntryAssembly().FullName.Split( ',' )[0]}_{Environment.MachineName}";
                ;
            }
        }

        public uint   AutomaticKeyAndMetaSendInterval { get; set; }
        public byte[] BrokerCACert                    { get; set; }
        public uint   ChunkSize                       { get; set; }
#pragma warning disable 67
        public event EventHandler<string> ClientDisconnected;
#pragma warning restore 67
        public string ClientId { get; set; }

        public void Connect( ClientCredentials credentials = null )
        {
            IsConnected = !SimulateConnectError;
        }

        public void Disconnect()
        {
            IsConnected = false;
        }

        public ProcessDataSet GenerateDataSet( string name, ushort writerId, DataSetType dataSetType )
        {
            return new ProcessDataSet( ClientId, name, writerId, dataSetType );
        }

        public bool                                           IsConnected             { get; private set; }
        public Dictionary<uint, DateTime>                     LastKeyAndMetaSentTimes { get; set; }
        public event DecodeMessage.MessageDecodedEventHandler MessageReceived;
#pragma warning disable 67
        public event DecodeMessage.MessageDecodedEventHandler MetaMessageReceived;
#pragma warning restore 67
        public EncodingOptions Options { get; }

        public bool SendDataSet( ProcessDataSet dataSet, string m_TopicConfigRequest, bool delta )
        {
            ReceiveDataFromApp?.Invoke( this, dataSet );
            return true;
        }

        public bool SendDataSet( ProcessDataSet dataSet, bool delta )
        {
            ReceiveDataFromApp?.Invoke( this, dataSet );
            return true;
        }

        public bool SendFile( File file, ushort writerId )
        {
            return true;
        }

        public bool SendFile( File file, string topicPrefix, ushort writerId )
        {
            return true;
        }

        public void     SendKeepAlive( ProcessDataSet dataSet ) { }
        public Settings Settings                                { get; }
        public void     Subscribe( string topic )               { }
#pragma warning disable 67
        public event DecodeMessage.MessageDecodedEventHandler UnknownMessageReceived;
#pragma warning restore 67
        public event EventHandler<ProcessDataSet> ReceiveDataFromApp;

        public void SendDataToApp( ProcessDataSet dataSet, string topic, string publisherId = null )
        {
            if ( !string.IsNullOrEmpty( publisherId ) )
            {
                ClientId            = publisherId;
                dataSet.PublisherId = publisherId;
            }
            if ( string.IsNullOrEmpty( topic ) )
            {
                topic = Settings.Client.DefaultPublisherTopicName;
                topic = Client.CreateTopicName( topic, ClientId, dataSet.GetWriterId(), "Meas", dataSet.GetDataSetType() );
            }
            MessageDecodedEventArgs args = new MessageDecodedEventArgs( dataSet.GenerateDateFrame(), topic );
            MessageReceived?.Invoke( this, args );
        }

        public bool SimulateConnectError { get; set; }

        private void OnRecievedDataFromApp( object sender, EventArgs args )
        {
            Console.WriteLine( sender );
            Console.WriteLine( args );
        }

#pragma warning disable 67
        public event EventHandler<Exception>  ExceptionCaught;
        public event FileReceivedEventHandler FileReceived;
#pragma warning restore 67
    }
}