// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using opc.ua.pubsub.dotnet.binary.Messages.Meta.Structure;

namespace opc.ua.pubsub.dotnet.binary.DataPoints
{
    public class File : DataPointValue
    {
        public const           NodeIDType PreDefinedType   = NodeIDType.File;
        public static readonly NodeID     PreDefinedNodeID = new NodeID( 2, 7, 3 );
        public                 byte[]     Content  { get; set; }
        public                 String     FileType { get; set; }

        public NodeIDType NodeIDType
        {
            get
            {
                return PreDefinedType;
            }
        }

        public override NodeID NodeID
        {
            get
            {
                return PreDefinedNodeID;
            }
        }

        public String Path { get; set; }

        public override StructureDescription StructureDescription
        {
            get
            {
                StructureDescription desc = new StructureDescription
                                            {
                                                    DataTypeId = NodeID,
                                                    Name = new QualifiedName
                                                           {
                                                                   NamespaceIndex = 1,
                                                                   Name           = new String( "File" )
                                                           },
                                                    DefaultEncoding = new NodeID
                                                                      {
                                                                              Namespace = 0,
                                                                              Value     = 76
                                                                      },
                                                    BaseDataType = new NodeID
                                                                   {
                                                                           Namespace = 0,
                                                                           Value     = 22
                                                                   },
                                                    StructureType = 0,
                                                    Fields = new List<StructureField>
                                                             {
                                                                     new StructureField
                                                                     {
                                                                             Name        = new String( "Content" ), // content of the file
                                                                             Description = new LocalizedText(),
                                                                             DataType = new NodeID
                                                                                        {
                                                                                                Namespace = 0,
                                                                                                Value     = 15
                                                                                        }, // ByteString == byte[]
                                                                             ValueRank  = -1,
                                                                             IsOptional = false
                                                                     },
                                                                     new StructureField
                                                                     {
                                                                             Name        = new String( "Path" ),
                                                                             Description = new LocalizedText(),
                                                                             DataType = new NodeID
                                                                                        {
                                                                                                Namespace = 0,
                                                                                                Value     = 12
                                                                                        }, // String
                                                                             ValueRank  = -1,
                                                                             IsOptional = false
                                                                     },
                                                                     new StructureField
                                                                     {
                                                                             Name        = new String( "Timestamp" ), // modified time of the file
                                                                             Description = new LocalizedText(),
                                                                             DataType = new NodeID
                                                                                        {
                                                                                                Namespace = 0,
                                                                                                Value     = 13
                                                                                        }, // DateTime
                                                                             ValueRank  = -1,
                                                                             IsOptional = false
                                                                     },
                                                                     new StructureField
                                                                     {
                                                                             Name        = new String( "Type" ),
                                                                             Description = new LocalizedText(),
                                                                             DataType = new NodeID
                                                                                        {
                                                                                                Namespace = 0,
                                                                                                Value     = 12
                                                                                        }, // String
                                                                             ValueRank  = -1,
                                                                             IsOptional = false
                                                                     }
                                                             }
                                            };
                return desc;
            }
        }

        // created time
        public long Time { get; set; }

        public override void Decode( Stream inputStream )
        {
            if ( inputStream == null || !inputStream.CanRead )
            {
                return;
            }

            // 1. Content
            Content = SimpleArray<byte>.Decode( inputStream, BaseType.ReadByte );

            // 2. Path
            Path = String.Decode( inputStream );

            // 3. Timestamp
            long? readTime = BaseType.ReadInt64( inputStream );
            if ( readTime != null )
            {
                Time = readTime.Value;
            }

            // 4. File type
            FileType = String.Decode( inputStream );

            // 5. Name (is not used via OPC UA but can be used for logging etc.)
            Name = System.IO.Path.GetFileName( Path.Value );
        }

        public override void Encode( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }

            // 1. Content
            int contentLength = -1;
            if ( Content != null )
            {
                contentLength = Content.Length;
            }
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( contentLength ) );
            for ( int i = 0; i < contentLength; i++ )
            {
                // ReSharper disable once PossibleNullReferenceException
                outputStream.WriteByte( Content[i] );
            }

            // 2. Path
            Path.Encode( outputStream );

            // 3. Timestamp
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( Time ) );

            // 4. File type
            FileType.Encode( outputStream );
        }
    }
}