// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using Binary.Messages.Meta;
using Binary.Messages.Meta.Structure;

namespace opc.ua.pubsub.dotnet.simulation.Excel.Model
{
    public class CommonConfig
    {
        public CommonConfig()
        {
            KnownDataTypes   = new Dictionary<string, NodeID>();
            MetaConfig       = new MetaConfig();
            EnumDescriptions = new Dictionary<NodeID, EnumDescription>();
        }

        public ushort                              DataSetWriterID  { get; set; }
        public Dictionary<NodeID, EnumDescription> EnumDescriptions { get; set; }
        public Dictionary<string, NodeID>          KnownDataTypes   { get; }
        public MetaConfig                          MetaConfig       { get; set; }
        public string                              PublisherID      { get; set; }
    }
}