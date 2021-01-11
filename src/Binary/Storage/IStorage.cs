// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using opc.ua.pubsub.dotnet.binary.Messages;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;

namespace opc.ua.pubsub.dotnet.binary.Storage
{
    public interface IStorage
    {
        byte[] GetMetaMessage( string      publisherId, ConfigurationVersion cfgVersion );
        void   StoreDataMessage( DataFrame dataFrame );
        void   StoreMetaMessage( MetaFrame metaFrame );
    }
}