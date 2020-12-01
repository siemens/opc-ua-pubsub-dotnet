// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.IO;

namespace Binary.Header
{
    public class NetworkMessageHeader
    {
        public NetworkMessageHeader()
        {
            VersionAndFlags = 1;
            ExtendedFlags1  = new ExtendedFlags1();
            ExtendedFlags2  = new ExtendedFlags2();
        }

        public ExtendedFlags1 ExtendedFlags1 { get; set; }
        public ExtendedFlags2 ExtendedFlags2 { get; set; }

        /// <summary>
        ///     This is a convenience property which extracts just
        ///     the protocol version from the "Protocol Version / Flags" field.
        /// </summary>
        public byte ProtocolVersion
        {
            get
            {
                // Remove the UADP Flags
                int version = VersionAndFlags & 0xF;
                return (byte)version;
            }
        }

        public String PublisherID { get; set; }

        /// <summary>
        ///     Convenience property which just provides the flags from
        ///     the combined "Version / Flags" field.
        /// </summary>
        public UADPFlags UADPFlags
        {
            get
            {
                int       raw   = VersionAndFlags & 0xF0;
                UADPFlags flags = (UADPFlags)raw;
                return flags;
            }
        }

        /// <summary>
        ///     Original field according to the OPC UA PubSub standard.
        ///     Contains both the version and the flags.
        /// </summary>
        public byte VersionAndFlags { get; set; }

        public static NetworkMessageHeader Decode( Stream inputStream )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return null;
            }
            NetworkMessageHeader instance = new NetworkMessageHeader
                                            {
                                                    VersionAndFlags = (byte)inputStream.ReadByte()
                                            };
            if ( instance.UADPFlags.HasFlag( UADPFlags.ExtendedFlags1Enabled ) )
            {
                instance.ExtendedFlags1.RawValue = (byte)inputStream.ReadByte();
                if ( instance.ExtendedFlags1.Flags1.HasFlag( ExtendedFlags1Enum.ExtendedFlags2Enabled ) )
                {
                    instance.ExtendedFlags2.RawValue = (byte)inputStream.ReadByte();
                }
            }

            // Publisher ID
            if ( instance.UADPFlags.HasFlag( UADPFlags.PublisherIdEnabled ) )
            {
                if ( instance.ExtendedFlags1.PublisherIdType != PublisherIdType.String )
                {
                    instance.PublisherID = new String( "TODO: Decode Guid PublisherId!" );

                    //throw new NotSupportedException($"Parsing a PublisherID of type {instance.ExtendedFlags1.PublisherIdType} is currently not supported.");
                }
                else
                {
                    instance.PublisherID = String.Decode( inputStream );
                }
            }
            return instance;
        }

        public void Encode( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }

            // Protocol Version and Flags field
            outputStream.WriteByte( VersionAndFlags );

            // Extended Flags 1
            if ( UADPFlags.HasFlag( UADPFlags.ExtendedFlags1Enabled ) )
            {
                outputStream.WriteByte( ExtendedFlags1.RawValue );

                // Extended Flags 2
                if ( ExtendedFlags1.Flags1.HasFlag( ExtendedFlags1Enum.ExtendedFlags2Enabled ) )
                {
                    outputStream.WriteByte( ExtendedFlags2.RawValue );
                }
            }

            // Publisher ID
            if ( UADPFlags.HasFlag( UADPFlags.PublisherIdEnabled ) )
            {
                PublisherID.Encode( outputStream );
            }
        }
    }
}