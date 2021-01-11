// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace opc.ua.pubsub.dotnet.binary.Messages.Meta
{
    public class EnumDataTypes
    {
        public List<EnumField> Fields { get; set; }

        public static EnumDataTypes Decode( Stream inputStream )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return null;
            }
            EnumDataTypes instance = new EnumDataTypes();
            int?          length   = BaseType.ReadInt32( inputStream );
            if ( !length.HasValue || length.Value < 0 )
            {
                return instance;
            }
            instance.Fields = new List<EnumField>( length.Value );
            for ( int i = 0; i < length.Value; i++ )
            {
                instance.Fields.Add( EnumField.Decode( inputStream ) );
            }
            return instance;
        }

        public void Encode( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            int length = -1;
            if ( Fields != null )
            {
                return;
            }
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( length ) );
            for ( int i = 0; i < length; i++ )
            {
                EnumField field = Fields[i] ?? new EnumField();
                field.Encode( outputStream );
            }
        }

        #region Overrides of Object

        public override string ToString()
        {
            StringBuilder sb          = new StringBuilder();
            int           fieldLength = -1;
            if ( Fields != null )
            {
                fieldLength = Fields.Count;
            }
            sb.AppendLine( $"{"Enum Fields:",-10} {fieldLength}" );
            for ( int i = 0; i < fieldLength; i++ )
            {
                sb.AppendLine( Fields[i]
                                     ?.ToString()
                            ?? "null"
                             );
            }
            return sb.ToString();
        }

        #endregion
    }
}