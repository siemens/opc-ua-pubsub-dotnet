// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.IO;
using System.Text;
using log4net;

namespace Binary.Messages.KeepAlive
{
    public class KeepAliveFrame : DataFrame
    {
        private static readonly ILog Logger = LogManager.GetLogger( typeof(KeepAliveFrame) );
        public KeepAliveFrame() { }
        public KeepAliveFrame( DataFrame dataFrame ) : base( dataFrame ) { }

        public static KeepAliveFrame Decode( Stream inputStream, DataFrame dataFrame )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return null;
            }
            KeepAliveFrame instance = new KeepAliveFrame( dataFrame );
            return instance;
        }

        public override void Encode( Stream outputStream, bool withHeader = true )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            base.Encode( outputStream, withHeader );
        }

        public override void EncodeChunk( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            base.EncodeChunk( outputStream );
        }

        #region Overrides of DataFrame

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine( "" );
            sb.AppendLine( "=============================================================================================================================================="
                         );
            sb.AppendLine( "Keep Alive Message" );
            sb.AppendLine( "=============================================================================================================================================="
                         );
            sb.AppendLine();
            return sb.ToString();
        }

        #endregion
    }
}