// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;

namespace opc.ua.pubsub.dotnet.simulation.Excel.Model
{
    public class KeyEntry : DeltaEntry
    {
        public string Description { get; set; }
        public Guid   FieldID     { get; set; }
        public string Prefix      { get; set; }
        public string Unit        { get; set; }
    }
}