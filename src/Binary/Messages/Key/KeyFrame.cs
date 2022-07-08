// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using opc.ua.pubsub.dotnet.binary.DataPoints;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using log4net;
using File = opc.ua.pubsub.dotnet.binary.DataPoints.File;

namespace opc.ua.pubsub.dotnet.binary.Messages.Key
{
    public class KeyFrame : DataFrame
    {
        private static readonly ILog Logger = LogManager.GetLogger( MethodBase.GetCurrentMethod()
                                                                              .DeclaringType
                                                                  );
        public KeyFrame() : this( new EncodingOptions() ) { }

        public KeyFrame( EncodingOptions options )
        {
            Options = options;
        }

        public KeyFrame( DataFrame dataFrame ) : this( dataFrame, new EncodingOptions() ) { }

        public KeyFrame( DataFrame dataFrame, EncodingOptions options ) : base( dataFrame )
        {
            Options = options;
        }

        public static KeyFrame Decode( Stream inputStream, DataFrame dataFrame, MetaFrame meta )
        {
            if ( inputStream == null || !inputStream.CanRead || meta == null )
            {
                return null;
            }
            KeyFrame instance = new KeyFrame( dataFrame );
            instance.MetaFrame = meta;
            instance.Timestamp = dataFrame.Timestamp;
            ushort  fieldCount = (ushort)(meta.FieldMetaDataList?.Count ?? 0);
            ushort? readUInt16 = BaseType.ReadUInt16( inputStream );
            if ( readUInt16 == null )
            {
                return null;
            }
            if ( readUInt16.Value != fieldCount )
            {
                Logger.Error( $"Number of fields from Meta: {meta.FieldMetaDataList.Count} vs. FieldCount: {readUInt16.Value}" );
                return null;
            }
            instance.Items = new List<DataPointValue>( fieldCount );
            for ( ushort index = 0; index < fieldCount; index++ )
            {
                DataPointValue item = ParseDataPoint( inputStream, meta, index );
                if ( item != null )
                {
                    instance.Items.Add( item );
                }
            }
            return instance;
        }

        public override void Encode( Stream outputStream, bool withHeader = true )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            base.Encode( outputStream, withHeader );
            EncodeDataPoints( outputStream );
        }

        public override void EncodeChunk( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            base.EncodeChunk( outputStream );
            EncodeDataPoints( outputStream );
        }

        #region Overrides of DataFrame

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine( "" );
            sb.AppendLine( "=============================================================================================================================================="
                         );
            sb.AppendLine( "Key Message" );
            sb.AppendLine( "----------------------------------------------------------------------------------------------------------------------------------------------"
                         );
            sb.AppendLine( System.Globalization.CultureInfo.InvariantCulture, $"{"Index",10} | {"Name",50} | {"Orcat",10} | {"Quality",10} | {"Custom",10} | {"MDSP q",10} | {"Timestamp",21} | {"Value",20}" );
            sb.AppendLine( "----------------------------------------------------------------------------------------------------------------------------------------------"
                         );
            for ( int i = 0; i < Items.Count; i++ )
            {
                DataPointValue dpv   = Items[i];
                string         name  = "";
                string         value = "";
                switch ( dpv )
                {
                    case ProcessDataPointValue pdv:
                        value = pdv.ToString();
                        break;

                    case File file:
                        value = file.ToString();
                        break;
                }
                if ( MetaFrame?.FieldMetaDataList != null && i < MetaFrame.FieldMetaDataList.Count )
                {
                    name = MetaFrame.FieldMetaDataList[dpv.Index]
                                    .Name.ToString();
                }
                sb.AppendLine( System.Globalization.CultureInfo.InvariantCulture, $"{dpv.Index,10} | {name,-50} | {value}" );
            }
            sb.AppendLine( "=============================================================================================================================================="
                         );
            sb.AppendLine();
            return sb.ToString();
        }

        #endregion

        private void EncodeDataPoints( Stream outputStream )
        {
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( (ushort)Items.Count ) );
            foreach ( DataPointValue dataPointValue in Items )
            {
                WriteSingleDataPoint( outputStream, dataPointValue );
            }
        }
    }
}