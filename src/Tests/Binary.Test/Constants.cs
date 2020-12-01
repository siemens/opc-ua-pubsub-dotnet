// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

namespace opc.ua.pubsub.dotnet.binary.test
{
    public static class OpcConstants
    {
        public static readonly ushort EventWriterID                = 1001;
        public static readonly ushort FileWriterID                 = 2000;
        public static readonly ushort InternalNotificationWriterID = 3000;
        public static readonly char   PublisherIDSeparator         = '_';
        public static readonly ushort TimeSeriesWriterID           = 1000;
    }

    public static class OrcatConstants
    {
        public static readonly byte Automatic_Bay         = 4;
        public static readonly byte Automatic_Remote      = 6;
        public static readonly byte Automatic_Station     = 5;
        public static readonly byte Bay_Control           = 1;
        public static readonly byte General_Interrogation = 9;
        public static readonly byte Maintenance           = 7;
        public static readonly byte Process               = 8;
        public static readonly byte Remote_Control        = 3;
        public static readonly byte Station_Control       = 2;
        public static readonly byte Unknown               = 0;
    }

    public static class QualityConstants
    {
        public static readonly ushort Bad_Reference    = 0x0010;
        public static readonly ushort Failure          = 0x0040;
        public static readonly ushort Good             = 0x0000;
        public static readonly ushort Inaccurate       = 0x0200;
        public static readonly ushort Inconsistent     = 0x0100;
        public static readonly ushort Invalid          = 0x0001;
        public static readonly ushort Old_Data         = 0x0080;
        public static readonly ushort Operator_Blocked = 0x1000;
        public static readonly ushort Oscillatory      = 0x0020;
        public static readonly ushort Out_Of_Range     = 0x0008;
        public static readonly ushort Overflow         = 0x0004;
        public static readonly ushort Substituted      = 0x0400;
        public static readonly ushort Test             = 0x0800;
    }
}