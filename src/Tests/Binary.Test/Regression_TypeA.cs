// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.IO;
using System.Reflection;
using Binary;
using Binary.Decode;
using Binary.Messages;
using Binary.Messages.Chunk;
using Binary.Messages.Delta;
using Binary.Messages.Key;
using Binary.Messages.Meta;
using NUnit.Framework;

namespace opc.ua.pubsub.dotnet.binary.test
{
    public class Regression_TypeA
    {
        private const string        PublisherID = "A8000_CP802x_IoTHub_GF1708502441";
        private       DecodeMessage Decoder        { get; set; }
        private       string        TestDataFolder { get; set; }

        [TestCase( "04.delta.bin", ExpectedResult = 37 )]
        [TestCase( "05.delta.bin", ExpectedResult = 6 )]
        [TestCase( "06.delta.bin", ExpectedResult = 100 )]
        [TestCase( "07.delta.bin", ExpectedResult = 16 )]
        [Order( 400 )]
        public int DeltaMessages( string fileName )
        {
            if ( Debugger.IsAttached )
            {
                MetaMessageFirstChunk();
                MetaMessageLastChunk();
            }
            string         filePath   = Path.Combine( TestDataFolder, fileName );
            byte[]         rawMessage = File.ReadAllBytes( filePath );
            NetworkMessage message    = null;
            Assert.DoesNotThrow( () => message = Decoder.ParseBinaryMessage( rawMessage ) );
            Assert.That( message,                                        Is.Not.Null );
            Assert.That( message.NetworkMessageHeader.PublisherID.Value, Is.EqualTo( PublisherID ) );
            Assert.That( message,                                        Is.InstanceOf( typeof(DeltaFrame) ) );
            DeltaFrame deltaMessage = (DeltaFrame)message;
            Assert.That( deltaMessage.DataSetMessageSequenceNumber, Is.EqualTo( 0 ) );
            return deltaMessage.FieldIndexList.Count;
        }

        [Test]
        [Order( 300 )]
        public void KeyMessage()
        {
            if ( Debugger.IsAttached )
            {
                MetaMessageFirstChunk();
                MetaMessageLastChunk();
            }
            string         filePath   = Path.Combine( TestDataFolder, "03.key.bin" );
            byte[]         rawMessage = File.ReadAllBytes( filePath );
            NetworkMessage message    = null;
            Assert.DoesNotThrow( () => message = Decoder.ParseBinaryMessage( rawMessage ) );
            Assert.That( message,                                        Is.Not.Null );
            Assert.That( message.NetworkMessageHeader.PublisherID.Value, Is.EqualTo( PublisherID ) );
            Assert.That( message,                                        Is.InstanceOf( typeof(KeyFrame) ) );
            KeyFrame keyMessage = (KeyFrame)message;
            Assert.That( keyMessage.DataSetMessageSequenceNumber, Is.EqualTo( 0 ) );
            Assert.That( keyMessage.Items.Count,                  Is.EqualTo( 138 ) );
        }

        [Test]
        [Order( 100 )]
        public void MetaMessageFirstChunk()
        {
            string         chunkFilePath = Path.Combine( TestDataFolder, "01.meta.chunk.bin" );
            byte[]         rawMessage    = File.ReadAllBytes( chunkFilePath );
            NetworkMessage message       = null;
            Assert.DoesNotThrow( () => message = Decoder.ParseBinaryMessage( rawMessage ) );
            Assert.That( message,                                        Is.Not.Null );
            Assert.That( message.NetworkMessageHeader.PublisherID.Value, Is.EqualTo( PublisherID ) );
            Assert.That( message,                                        Is.InstanceOf( typeof(ChunkedMessage) ) );
            ChunkedMessage chunkedMessage = (ChunkedMessage)message;
            Assert.That( chunkedMessage.ChunkOffset, Is.EqualTo( 0 ) );
            Assert.That( chunkedMessage.TotalSize,   Is.EqualTo( 21189 ) );
        }

        [Test]
        [Order( 200 )]
        public void MetaMessageLastChunk()
        {
            if ( Debugger.IsAttached )
            {
                MetaMessageFirstChunk();
            }
            string         chunkFilePath = Path.Combine( TestDataFolder, "02.meta.chunk.bin" );
            byte[]         rawMessage    = File.ReadAllBytes( chunkFilePath );
            NetworkMessage message       = null;
            Assert.DoesNotThrow( () => message = Decoder.ParseBinaryMessage( rawMessage ) );
            Assert.That( message,                                        Is.Not.Null );
            Assert.That( message.NetworkMessageHeader.PublisherID.Value, Is.EqualTo( PublisherID ) );
            Assert.That( message,                                        Is.InstanceOf( typeof(MetaFrame) ) );
            MetaFrame metaMessage = (MetaFrame)message;
            Assert.That( metaMessage.DataSetWriterID,         Is.EqualTo( 1000 ) );
            Assert.That( metaMessage.FieldMetaDataList.Count, Is.EqualTo( 138 ) );

            // Check Namespaces
            // First entry must be null
            Assert.That( metaMessage.Namespaces[0], Is.EqualTo( new String( string.Empty ) ) );
            Assert.That( metaMessage.Namespaces[1], Is.EqualTo( new String( "http://siemens.com/energy/schema/opcua/ps/v2" ) ) );
            Assert.That( metaMessage.Namespaces[2], Is.EqualTo( new String( "https://mindsphere.io/OPCUAPubSub/v3" ) ) );
        }

        [OneTimeSetUp]
        public void Setup()
        {
            EncodingOptions options = new EncodingOptions
                                      {
                                              LegacyFieldFlagEncoding = true
                                      };
            Decoder = new DecodeMessage( options );
            TestDataFolder = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly()
                                                                          .Location
                                                                ),
                                           "Regression",
                                           "TypeA"
                                         );
        }
    }
}