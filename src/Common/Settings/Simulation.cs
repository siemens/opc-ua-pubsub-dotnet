// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

namespace opc.ua.pubsub.dotnet.common.Settings
{
    public class Simulation
    {
        public string BrokerCACertDER      { get; set; }
        public string BrokerHostname       { get; set; } = "localhost";
        public string ClientCertP12        { get; set; }
        public string ClientCertPassword   { get; set; }
        public string ClientRootCACertDER  { get; set; }
        public string InputFile            { get; set; }
        public string PublisherID          { get; set; }
        public bool   SkipDelta            { get; set; }
        public bool   SkipKeepAlive        { get; set; }
        public bool   SkipKey              { get; set; }
        public bool   SkipMeta             { get; set; }
        public string TopicPrefix          { get; set; } = "c/{ClientID}/o/opcua/{VersionMS}/u/{T:d/t}{TM:m/t}{F:d/f}{FM:m/f}{E:d/e}{EM:m/e}";
        public int    WaitAfterKeyMessage  { get; set; }
        public int    WaitAfterMetaMessage { get; set; } = 1500;
        public int    WaitBeforeClosing    { get; set; } = 1500;
        public int    WaitBeforeStarting   { get; set; } = 1500;
    }
}