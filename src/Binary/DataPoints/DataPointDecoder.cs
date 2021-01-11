// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using log4net;

namespace opc.ua.pubsub.dotnet.binary.DataPoints
{
    public static class DataPointEncoderDecoder
    {
        private static readonly ILog Logger = LogManager.GetLogger( typeof(DataPointEncoderDecoder) );

        public static void Decode( Stream inputStream, Dictionary<string, object> fields, NodeID dataType, string name )
        {
            if ( dataType.Equals( WellKnownNodeIDs.UInt32 ) )
            {
                uint? readMDSPQuality = BaseType.ReadUInt32( inputStream );
                if ( readMDSPQuality != null )
                {
                    fields[name] = readMDSPQuality.Value;
                }
            }
            else if ( dataType.Equals( WellKnownNodeIDs.DateTime ) )
            {
                long? readInt64 = BaseType.ReadInt64( inputStream );
                if ( readInt64 != null )
                {
                    fields[name] = readInt64.Value;
                }
            }
            else if ( dataType.Equals( WellKnownNodeIDs.Boolean ) )
            {
                fields[name] = inputStream.ReadByte() != 0;
            }
            else if ( dataType.Equals( WellKnownNodeIDs.Byte ) )
            {
                fields[name] = Convert.ToByte( inputStream.ReadByte() );
            }
            else if ( dataType.Equals( WellKnownNodeIDs.Float ) )
            {
                float? readFloat = BaseType.ReadFloat( inputStream );
                if ( readFloat != null )
                {
                    fields[name] = readFloat.Value;
                }
            }
            else if ( dataType.Equals( WellKnownNodeIDs.Int32 ) )
            {
                int? readInt32 = BaseType.ReadInt32( inputStream );
                if ( readInt32 != null )
                {
                    fields[name] = readInt32.Value;
                }
            }
            else if ( dataType.Equals( WellKnownNodeIDs.Int64 ) )
            {
                long? readInt64 = BaseType.ReadInt64( inputStream );
                if ( readInt64 != null )
                {
                    fields[name] = readInt64.Value;
                }
            }
            else if ( dataType.Equals( WellKnownNodeIDs.String ) )
            {
                String readString = BaseType.ReadString( inputStream );
                if ( readString != null )
                {
                    fields[name] = readString.Value;
                }
            }
            else /*enum - backward compatibility*/
            {
                int? readInt32 = BaseType.ReadInt32( inputStream );
                if ( readInt32 != null )
                {
                    fields[name] = readInt32.Value;
                }
            }
        }

        public static void Encode( Stream outputStream, NodeID dataType, object value )
        {
            if ( dataType.Equals( WellKnownNodeIDs.UInt32 ) )
            {
                BaseType.WriteToStream( outputStream, BitConverter.GetBytes( Convert.ToUInt32( value, CultureInfo.InvariantCulture ) ) );
            }
            else if ( dataType.Equals( WellKnownNodeIDs.DateTime ) )
            {
                BaseType.WriteToStream( outputStream, BitConverter.GetBytes( Convert.ToInt64( value, CultureInfo.InvariantCulture ) ) );
            }
            else if ( dataType.Equals( WellKnownNodeIDs.Boolean ) )
            {
                outputStream.WriteByte( (byte)( Convert.ToBoolean( value, CultureInfo.InvariantCulture ) ? 1 : 0 ) );
            }
            else if ( dataType.Equals( WellKnownNodeIDs.Byte ) )
            {
                outputStream.WriteByte( Convert.ToByte( value, CultureInfo.InvariantCulture ) );
            }
            else if ( dataType.Equals( WellKnownNodeIDs.Float ) )
            {
                BaseType.WriteToStream( outputStream, BitConverter.GetBytes( Convert.ToSingle( value, CultureInfo.InvariantCulture ) ) );
            }
            else if ( dataType.Equals( WellKnownNodeIDs.Int32 ) )
            {
                BaseType.WriteToStream( outputStream, BitConverter.GetBytes( Convert.ToInt32( value, CultureInfo.InvariantCulture ) ) );
            }
            else if ( dataType.Equals( WellKnownNodeIDs.Int64 ) )
            {
                BaseType.WriteToStream( outputStream, BitConverter.GetBytes( Convert.ToInt64( value, CultureInfo.InvariantCulture ) ) );
            }
            else if ( dataType.Equals( WellKnownNodeIDs.String ) )
            {
                BaseType.WriteToStream( outputStream,
                                        BitConverter.GetBytes( value.ToString()
                                                                    .Length
                                                             )
                                      );
                BaseType.WriteToStream( outputStream, Encoding.UTF8.GetBytes( value.ToString() ) );
            }
            else
            {
                Logger.Info( "Unsupported data type cannot be encoded" );
            }
        }
    }
}