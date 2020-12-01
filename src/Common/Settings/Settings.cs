// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

namespace opc.ua.pubsub.dotnet.common.Settings
{
    public class Settings
    {
        public Broker     Broker     { get; set; }
        public Client     Client     { get; set; }
        public Simulation Simulation { get; set; }
    }
}