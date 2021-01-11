﻿// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;

namespace opc.ua.pubsub.dotnet.binary.Header
{
    [Flags]
    public enum UADPFlags
    {
        PublisherIdEnabled    = 16,
        GroupHeaderEnabled    = 32,
        PayloadHeaderEnabled  = 64,
        ExtendedFlags1Enabled = 128
    }
}