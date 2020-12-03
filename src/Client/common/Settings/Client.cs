// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

namespace opc.ua.pubsub.dotnet.client.common.Settings
{
    public class Client
    {
        public bool   AllowUntrustedCertificates        { get; set; }
        public string BrokerCACert                      { get; set; }
        public string BrokerHostname                    { get; set; } = "localhost";
        public int    BrokerPort                        { get; set; } = 8883;
        public string ClientCertP12                     { get; set; }
        public string ClientCertPassword                { get; set; }
        public string DefaultPublisherTopicName         { get; set; }
        public string DefaultSubscribeTopicName         { get; set; }
        public bool   EnableLogging                     { get; set; }
        public bool   IgnoreCertificateChainErrors      { get; set; }
        public bool   IgnoreCertificateRevocationErrors { get; set; }
        public bool   SubscribeUseTls                   { get; set; }

        // Mqtt communication timeout in seconds
        public int CommunicationTimeout { get; set; } = 10;

        // Mqtt keep alive period in seconds
        public int MqttKeepAlivePeriod { get; set; } = 15;
    }
}