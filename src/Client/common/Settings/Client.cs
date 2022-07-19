// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Security;
using System.Text.Json.Serialization;

namespace opc.ua.pubsub.dotnet.client.common.Settings
{
    public class Client
    {
        public bool   AllowUntrustedCertificates        { get; set; }
        public string BrokerCACert                      { get; set; }
        public string BrokerHostname                    { get; set; } = "localhost";
        public int    BrokerPort                        { get; set; } = 1883;
        public string ClientCertP12                     { get; set; }
        public string ClientCertPassword                { get; set; }
        public string DefaultPublisherTopicName         { get; set; }
        public string DefaultSubscribeTopicName         { get; set; }
        public bool   EnableLogging                     { get; set; }
        public bool   IgnoreCertificateChainErrors      { get; set; }
        public bool   IgnoreCertificateRevocationErrors { get; set; }
        public bool   UseTls                            { get; set; } = false;
        public bool   RawDataMode                       { get; set; } = false;

        // Proxy Information
        public bool ProxyEnabled { get; set; }
        public string ProxyAddress { get; set; }
        public string ProxyPort { get; set; }
        public string ProxyUsername { get; set; }
        [JsonIgnore]
        public SecureString ProxyPassword { get; set; }

        // Mqtt communication timeout in seconds
        public int CommunicationTimeout { get; set; } = 10;

        // Mqtt keep alive period in seconds
        public int MqttKeepAlivePeriod { get; set; } = 15;

        public bool SendStatusMessages { get; set; } = false;
        public string StatusMessageTopic { get; set; }
        public bool NeverSendRetain { get; set; } = false;
    }
}