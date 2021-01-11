// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;

namespace opc.ua.pubsub.dotnet.binary.Messages
{
    [Flags]
    public enum DataSetFlags2Enum : byte
    {
        TimeStampEnabled   = 16,
        PicoSecondsEnabled = 32,
        Reserved           = 64
    }

    public enum DataSetMessageTypeEnum : byte
    {
        DataKeyFrame   = 0,
        DataDeltaFrame = 1,
        Event          = 2,
        KeepAlive      = 3,
        Reserved       = 4
    }

    public class DataSetFlags2
    {
        public DataSetMessageTypeEnum DataSetMessageType
        {
            get
            {
                int temp = RawValue & 0xF;
                return (DataSetMessageTypeEnum)temp;
            }
        }

        public DataSetFlags2Enum Flags2
        {
            get
            {
                int temp = RawValue & 0xF0;
                return (DataSetFlags2Enum)temp;
            }
        }

        public byte RawValue { get; set; }
    }
}