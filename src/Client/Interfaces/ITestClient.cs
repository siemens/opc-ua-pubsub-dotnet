// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;

namespace opc.ua.pubsub.dotnet.client.Interfaces
{
    public interface ITestClient : IClientService
    {
        bool                               SimulateConnectError { get; set; }
        event EventHandler<ProcessDataSet> ReceiveDataFromApp;
        void                               SendDataToApp( ProcessDataSet dataSet, string topic, string publisherId = null );
    }
}