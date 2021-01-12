// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;

namespace opc.ua.pubsub.dotnet.binary.Messages
{
    [Flags]
    public enum DataSetFlags1Enum : byte
    {
        IsValid                          = 1,
        DataSetSequenceNumberEnabled     = 8,
        StatusEnabled                    = 16,
        ConfigurationVersionMajorVersion = 32,
        ConfigurationVersionMinorVersion = 64,
        DataSetFlags2Enabled             = 128
    }

    public enum FieldEncodingEnum : byte
    {
        Variant   = 0,
        RawData   = 1,
        DataValue = 2,
        Reserved  = 3
    }

    public class DataSetFlags1
    {
        public FieldEncodingEnum FieldEncoding
        {
            get
            {
                int temp = RawValue & 0x6;
                return (FieldEncodingEnum)( temp >> 1 );
            }
        }

        public DataSetFlags1Enum Flags1
        {
            get
            {
                int temp = RawValue & 0xF9;
                return (DataSetFlags1Enum)temp;
            }
        }

        public byte RawValue { get; set; }
    }
}