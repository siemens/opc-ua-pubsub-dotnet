// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using Binary.Messages;
using Binary.Messages.Meta;

namespace Binary.Storage
{
    public interface IStorage
    {
        byte[] GetMetaMessage( string      publisherId, ConfigurationVersion cfgVersion );
        void   StoreDataMessage( DataFrame dataFrame );
        void   StoreMetaMessage( MetaFrame metaFrame );
    }
}