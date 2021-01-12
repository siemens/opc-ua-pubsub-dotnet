// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Globalization;

namespace opc.ua.pubsub.dotnet.binary.Header
{
    [Flags]
    public enum ExtendedFlags1Enum
    {
        DataSetClassIdEnabled = 8,
        SecurityEnabled       = 16,
        TimestampEnabled      = 32,
        PicosecondsEnabled    = 64,
        ExtendedFlags2Enabled = 128
    }

    public enum PublisherIdType
    {
        Byte      = 0,
        UInt16    = 1,
        UInt32    = 2,
        UInt64    = 3,
        String    = 4,
        Guid      = 5,
        Reserved1 = 6,
        Reserved2 = 7
    }

    public class ExtendedFlags1
    {
        /// <summary>
        ///     Initialize the instance with a PublisherID of type string.
        /// </summary>
        public ExtendedFlags1()
        {
            // PublisherID is of type string.
            RawValue = 0x4;
        }

        public ExtendedFlags1Enum Flags1
        {
            get
            {
                int temp = RawValue & 0xF8;
                return (ExtendedFlags1Enum)temp;
            }
        }

        public PublisherIdType PublisherIdType
        {
            get
            {
                int temp = RawValue & 0x7;
                return (PublisherIdType)temp;
            }
        }

        public byte RawValue { get; set; }

        #region Overrides of Object

        public override string ToString()
        {
            return
                    $"{int.Parse( Convert.ToString( RawValue, 2 ), CultureInfo.InvariantCulture ):0000 0000} [PublisherID Type: {PublisherIdType.ToString()}, {Flags1.ToString()}]";
        }

        #endregion
    }
}