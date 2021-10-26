// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using opc.ua.pubsub.dotnet.binary;
using opc.ua.pubsub.dotnet.binary.DataPoints;
using opc.ua.pubsub.dotnet.binary.Header;
using opc.ua.pubsub.dotnet.binary.Messages;
using opc.ua.pubsub.dotnet.binary.Messages.Delta;
using opc.ua.pubsub.dotnet.binary.Messages.Key;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using opc.ua.pubsub.dotnet.binary.Messages.Meta.Structure;
using File = opc.ua.pubsub.dotnet.binary.DataPoints.File;
using String = opc.ua.pubsub.dotnet.binary.String;

namespace opc.ua.pubsub.dotnet.simulation
{
    public class FileDataSet : TestDataSet
    {
        public FileDataSet( string publisherID ) : base( publisherID ) { }

        public override List<byte[]> GetChunkedKeyFrame( uint chunkSize )
        {
            throw new NotImplementedException();
        }

        public override byte[] GetEncodedDeltaFrame()
        {
            FileInfo   assetInfo = GetFileInfo();
            DeltaFrame delta     = new DeltaFrame();
            delta.ConfigurationVersion = m_MetaFrame.ConfigurationVersion;
            delta.NetworkMessageHeader = new NetworkMessageHeader
                                         {
                                                 PublisherID     = new String( m_PublisherID ),
                                                 VersionAndFlags = 0xD1,
                                                 ExtendedFlags1 = new ExtendedFlags1
                                                                  {
                                                                          RawValue = 0x04
                                                                  }
                                         };
            delta.Flags1.RawValue = 0xEB;
            delta.Flags2.RawValue = 0x01;
            delta.Items = new List<DataPointValue>
                          {
                                  new File
                                  {
                                          Path     = new String( assetInfo.FullName ),
                                          Content  = GetContent(),
                                          Time     = assetInfo.LastWriteTimeUtc.ToFileTimeUtc(),
                                          FileType = new String( "asset" )
                                  }
                          };
            delta.MetaFrame = m_MetaFrame;
            delta.PayloadHeader = new DataSetPayloadHeader
                                  {
                                          Count           = 1,
                                          DataSetWriterID = new ushort[] { 2000 }
                                  };
            delta.FieldCount = (ushort)delta.Items.Count;
            delta.FieldIndexList = new List<ushort>
                                   {
                                           0
                                   };
            byte[] result = null;
            using ( MemoryStream outputStream = new MemoryStream() )
            {
                delta.Encode( outputStream );
                result = outputStream.ToArray();
            }
            return result;
        }

        public override byte[] GetEncodedKeyFrame()
        {
            FileInfo assetInfo = GetFileInfo();
            KeyFrame key       = new KeyFrame();
            key.ConfigurationVersion = m_MetaFrame.ConfigurationVersion;
            key.NetworkMessageHeader = new NetworkMessageHeader
                                       {
                                               PublisherID     = new String( m_PublisherID ),
                                               VersionAndFlags = 0xD1,
                                               ExtendedFlags1 = new ExtendedFlags1
                                                                {
                                                                        RawValue = 0x04
                                                                }
                                       };
            key.Flags1.RawValue = 0x6B;
            byte[] content = GetContent();
            key.Items = new List<DataPointValue>
                        {
                                new File
                                {
                                        Path     = new String( assetInfo.FullName ),
                                        Content  = content,
                                        Time     = assetInfo.LastWriteTimeUtc.ToFileTimeUtc(),
                                        FileType = new String( "asset" )
                                }
                        };
            key.MetaFrame = m_MetaFrame;
            key.PayloadHeader = new DataSetPayloadHeader
                                {
                                        Count           = 1,
                                        DataSetWriterID = new ushort[] { 2000 }
                                };

            //key.FieldCount = (ushort)key.Items.Count;
            byte[] result = null;
            using ( MemoryStream outputStream = new MemoryStream() )
            {
                key.Encode( outputStream );
                result = outputStream.ToArray();
            }
            return result;
        }

        protected void AddFile( string Name )
        {
            if ( m_MetaFrame.FieldMetaDataList == null )
            {
                m_MetaFrame.FieldMetaDataList = new List<FieldMetaData>();
            }
            if ( m_MetaFrame.StructureDataTypes == null )
            {
                m_MetaFrame.StructureDataTypes = new Dictionary<NodeID, StructureDescription>();
            }
            if ( m_MetaFrame.Namespaces == null )
            {
                m_MetaFrame.Namespaces = new List<String>( 3 )
                                         {
                                                 new String(),
                                                 new String( "http://siemens.com/energy/schema/opcua/ps/v2" ),
                                                 new String( "https://mindsphere.io/OPCUAPubSub/v3" )
                                         };
            }
            FieldMetaData fieldMetaData = new FieldMetaData (new EncodingOptions() )
                                          {
                                                  Name     = new String( Name ),
                                                  DataType = File.PreDefinedNodeID
                                          };
            m_MetaFrame.FieldMetaDataList.Add( fieldMetaData );
            m_MetaFrame.StructureDataTypes[fieldMetaData.DataType] = new File().StructureDescription;
        }

        protected override void CreateMeta()
        {
            m_MetaFrame = new MetaFrame();
            ExtendedFlags1 extendedFlags1 = new ExtendedFlags1
                                            {
                                                    RawValue = 0x84
                                            };
            ExtendedFlags2 extendedFlags2 = new ExtendedFlags2
                                            {
                                                    RawValue = 0x08
                                            };
            m_MetaFrame.NetworkMessageHeader = new NetworkMessageHeader
                                               {
                                                       VersionAndFlags = 0x91,
                                                       ExtendedFlags1  = extendedFlags1,
                                                       ExtendedFlags2  = extendedFlags2,
                                                       PublisherID     = new String( m_PublisherID )
                                               };
            DateTime now  = DateTime.UtcNow;
            TimeSpan time = now - ConfigurationVersion.Base;
            m_MetaFrame.ConfigurationVersion.Major = (uint)time.TotalSeconds;
            m_MetaFrame.ConfigurationVersion.Minor = (uint)time.TotalSeconds;
            m_MetaFrame.Name                       = new String( "DataSet 002" );
            m_MetaFrame.DataSetWriterID            = 2000;
            AddFile( "AssetFile.json" );
            if ( m_MetaFrame.EnumDataTypes == null )
            {
                m_MetaFrame.EnumDataTypes = new Dictionary<NodeID, EnumDescription>();
            }
        }

        private static byte[] GetContent()
        {
            byte[] content = null;
            if ( System.IO.File.Exists( "AssetFile.json" ) )
            {
                content = System.IO.File.ReadAllBytes( "AssetFile.json" );
            }
            else
            {
                if ( System.IO.File.Exists( @"lib\AssetFile.json" ) )
                {
                    content = System.IO.File.ReadAllBytes( @"lib\AssetFile.json" );
                }
            }
            return content;
        }

        private static FileInfo GetFileInfo()
        {
            FileInfo assetInfo = null;
            if ( System.IO.File.Exists( "AssetFile.json" ) )
            {
                assetInfo = new FileInfo( "AssetFile.json" );
            }
            else
            {
                if ( System.IO.File.Exists( @"lib\AssetFile.json" ) )
                {
                    assetInfo = new FileInfo( @"lib\AssetFile.json" );
                }
            }
            return assetInfo;
        }
    }
}