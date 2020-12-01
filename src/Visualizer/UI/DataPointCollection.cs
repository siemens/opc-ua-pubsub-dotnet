// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using Binary.DataPoints;

namespace opc.ua.pubsub.dotnet.visualizer.UI
{
    public class DataPointCollection
    {
        public ushort               DataSetWriterID { get; set; }
        public bool                 IsKeyMessage    { get; set; }
        public string               PublisherID     { get; set; }
        public long                 Timestamp       { get; set; }
        public List<DataPointValue> Values          { get; set; }
    }
}