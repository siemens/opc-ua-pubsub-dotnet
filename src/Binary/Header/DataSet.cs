// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.IO;

namespace opc.ua.pubsub.dotnet.binary.Header
{
    public enum MessageTypeEnum : byte
    {
        Undefined  = byte.MinValue,
        KeyFrame   = 1,
        DeltaFrame = 2,
        KeepAlive  = 4,
        MetaFrame  = 5
    }

    public class DataSet
    {
        public ConfigurationVersion ConfigurationVersion
        {
            get
            {
                return new ConfigurationVersion
                       {
                               Major = MajorVersion,
                               Minor = MinorVersion
                       };
            }
        }

        public ushort[]        DataSetWriterID       { get; set; }
        public byte            EncodingFlags         { get; set; }
        public uint            MajorVersion          { get; set; }
        public byte            MessageCount          { get; set; }
        public ushort          MessageSequenceNumber { get; set; }
        public MessageTypeEnum MessageType           { get; set; }
        public uint            MinorVersion          { get; set; }
        public ushort[]        Size                  { get; set; }

        public static DataSet Decode( Stream inputStream )
        {
            DataSet instance = new DataSet();
            instance.MessageCount    = (byte)inputStream.ReadByte();
            instance.DataSetWriterID = SimpleArray<ushort>.Decode( inputStream, BaseType.ReadUInt16 );
            instance.Size            = SimpleArray<ushort>.Decode( inputStream, BaseType.ReadUInt16 );
            instance.MessageType     = (MessageTypeEnum)inputStream.ReadByte();
            if ( instance.MessageType == MessageTypeEnum.MetaFrame
              || instance.MessageType == MessageTypeEnum.KeyFrame
              || instance.MessageType == MessageTypeEnum.DeltaFrame )
            {
                instance.EncodingFlags = (byte)inputStream.ReadByte();
                ushort? sequenceNumber = BaseType.ReadUInt16( inputStream );
                if ( !sequenceNumber.HasValue )
                {
                    return null;
                }
                instance.MessageSequenceNumber = sequenceNumber.Value;
                uint? major = BaseType.ReadUInt32( inputStream );
                if ( !major.HasValue )
                {
                    return null;
                }
                instance.MajorVersion = major.Value;
                uint? minor = BaseType.ReadUInt32( inputStream );
                if ( !minor.HasValue )
                {
                    return null;
                }
                instance.MinorVersion = minor.Value;
            }
            return instance;
        }
    }
}