// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

namespace opc.ua.pubsub.dotnet.binary.Header
{
    public enum MessageType
    {
        DataSetMessage    = 0,
        DiscoveryRequest  = 1,
        DiscoveryResponse = 2,
        Reserved          = 3
    }

    public class ExtendedFlags2
    {
        public bool Chunk
        {
            get
            {
                bool result = ( RawValue & ( 1 << 0 ) ) != 0;
                return result;
            }
        }

        public MessageType MessageType
        {
            get
            {
                int temp = ( RawValue >> 2 ) & 0xF;
                return (MessageType)temp;
            }
        }

        public bool PromotedFieldsEnabled
        {
            get
            {
                bool result = ( RawValue & ( 1 << 1 ) ) != 0;
                return result;
            }
        }

        public byte RawValue { get; set; }
    }
}