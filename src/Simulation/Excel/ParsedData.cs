// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using opc.ua.pubsub.dotnet.simulation.Excel.Model;

namespace opc.ua.pubsub.dotnet.simulation.Excel
{
    public class ParsedData
    {
        public CommonConfig     CommonConfig { get; set; }
        public List<DeltaEntry> DeltaEntries { get; set; }
        public List<KeyEntry>   KeyEntries   { get; set; }
    }
}