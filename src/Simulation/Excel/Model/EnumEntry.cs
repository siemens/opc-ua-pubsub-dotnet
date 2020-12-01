// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using Binary.Messages.Meta;

namespace opc.ua.pubsub.dotnet.simulation.Excel.Model
{
    public class EnumEntry
    {
        public NodeID DataType                { get; set; }
        public string Description             { get; set; }
        public string DisplayName             { get; set; }
        public NodeID EnumDescriptionDataType { get; set; }
        public string QualifiedName           { get; set; }
        public int    Value                   { get; set; }
        public string ValueName               { get; set; }
    }
}