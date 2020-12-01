// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using Binary.Messages.Meta;

namespace opc.ua.pubsub.dotnet.simulation.Excel.Model
{
    public interface IEntry
    {
        public NodeID DataType  { get; set; }
        public ushort Index     { get; set; }
        public string Name      { get; set; }
        public byte   Orcat     { get; set; }
        public ushort Quality   { get; set; }
        public long   TimeStamp { get; set; }
        public object Value     { get; set; }
        public object Value2    { get; set; }
    }

    public class DeltaEntry : IEntry
    {
        public NodeID DataType  { get; set; }
        public ushort Index     { get; set; }
        public string Name      { get; set; }
        public byte   Orcat     { get; set; }
        public ushort Quality   { get; set; }
        public long   TimeStamp { get; set; }
        public object Value     { get; set; }
        public object Value2    { get; set; }
    }
}