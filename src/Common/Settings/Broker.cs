// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

namespace opc.ua.pubsub.dotnet.common.Settings
{
    public class Broker
    {
        public string BrokerP12         { get; set; }
        public string BrokerP12Password { get; set; }
        public bool   UseMutualAuth     { get; set; }
        public bool   UseTLS            { get; set; }
    }
}