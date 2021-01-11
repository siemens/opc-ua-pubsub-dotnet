// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using opc.ua.pubsub.dotnet.binary;
using opc.ua.pubsub.dotnet.binary.Decode;
using opc.ua.pubsub.dotnet.binary.Messages;
using opc.ua.pubsub.dotnet.binary.Messages.Delta;
using opc.ua.pubsub.dotnet.binary.Messages.Key;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using NUnit.Framework;

namespace opc.ua.pubsub.dotnet.binary.test
{
    public class Regression_TypeB_Events
    {
        private const string        PublisherID = "SIPROTEC_7KE85_SIP_BMTTTT123456";
        private       DecodeMessage Decoder        { get; set; }
        private       string        TestDataFolder { get; set; }

        [OneTimeSetUp]
        public void SetUp()
        {
            TestDataFolder = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly()
                                                                          .Location
                                                                ),
                                           "Regression",
                                           "TypeB",
                                           "Events"
                                         );
            Decoder = new DecodeMessage( new EncodingOptions
                                         {
                                                 LegacyFieldFlagEncoding = false
                                         }
                                       );
        }

        [Test]
        public void EdgeTest()
        {
            IEnumerable<string> files = Directory.EnumerateFiles( TestDataFolder, "" );
            foreach ( string filePath in files )
            {
                byte[]         rawMessage = File.ReadAllBytes( filePath );
                NetworkMessage message    = null;
                Assert.DoesNotThrow( () => message = Decoder.ParseBinaryMessage( rawMessage ) );
                Assert.That( message, Is.Not.Null );
            }
        }

        [Test]
        [Order( 100 )]
        public void MetaMessage()
        {
            string         chunkFilePath = Path.Combine( TestDataFolder, "00_Meta_Event.bin" );
            byte[]         rawMessage    = File.ReadAllBytes( chunkFilePath );
            NetworkMessage message       = null;
            Assert.DoesNotThrow( () => message = Decoder.ParseBinaryMessage( rawMessage ) );
            Assert.That( message,                                        Is.Not.Null );
            Assert.That( message.NetworkMessageHeader.PublisherID.Value, Is.EqualTo( PublisherID ) );
            Assert.That( message,                                        Is.InstanceOf( typeof(MetaFrame) ) );
            MetaFrame metaMessage = (MetaFrame)message;
            Assert.That( metaMessage.DataSetWriterID,         Is.EqualTo( 1001 ) );
            Assert.That( metaMessage.FieldMetaDataList.Count, Is.EqualTo( 62 ) );

            // Check Namespaces
            // First entry must be null
            Assert.That( metaMessage.Namespaces[0], Is.EqualTo( new String( null ) ) );
            Assert.That( metaMessage.Namespaces[1], Is.EqualTo( new String( "http://siemens.com/energy/schema/opcua/ps/v2" ) ) );
            Assert.That( metaMessage.Namespaces[2], Is.EqualTo( new String( "https://mindsphere.io/OPCUAPubSub/v3" ) ) );
        }

        [Test]
        [Order( 200 )]
        public void KeyMessage()
        {
            if ( Debugger.IsAttached )
            {
                MetaMessage();
            }
            string         filePath   = Path.Combine( TestDataFolder, "01_Key_Event.bin" );
            byte[]         rawMessage = File.ReadAllBytes( filePath );
            NetworkMessage message    = null;
            Assert.DoesNotThrow( () => message = Decoder.ParseBinaryMessage( rawMessage ) );
            Assert.That( message,                                        Is.Not.Null );
            Assert.That( message.NetworkMessageHeader.PublisherID.Value, Is.EqualTo( PublisherID ) );
            Assert.That( message,                                        Is.InstanceOf( typeof(KeyFrame) ) );
            KeyFrame keyMessage = (KeyFrame)message;
            Assert.That( keyMessage.DataSetMessageSequenceNumber, Is.EqualTo( 0 ) );
            Assert.That( keyMessage.Items.Count,                  Is.EqualTo( 62 ) );
        }

        [TestCase( "04_Delta_Event.bin", ExpectedResult = 1 )]
        [TestCase( "07_Delta_Event.bin", ExpectedResult = 3 )]
        [TestCase( "10_Delta_Event.bin", ExpectedResult = 12 )]
        [TestCase( "12_Delta_Event.bin", ExpectedResult = 3 )]
        [TestCase( "14_Delta_Event.bin", ExpectedResult = 1 )]
        [TestCase( "17_Delta_Event.bin", ExpectedResult = 3 )]
        [TestCase( "22_Delta_Event.bin", ExpectedResult = 1 )]
        [TestCase( "25_Delta_Event.bin", ExpectedResult = 3 )]
        [TestCase( "30_Delta_Event.bin", ExpectedResult = 1 )]
        [TestCase( "33_Delta_Event.bin", ExpectedResult = 3 )]
        [TestCase( "37_Delta_Event.bin", ExpectedResult = 1 )]
        [TestCase( "41_Delta_Event.bin", ExpectedResult = 3 )]
        [TestCase( "44_Delta_Event.bin", ExpectedResult = 18 )]
        [TestCase( "46_Delta_Event.bin", ExpectedResult = 3 )]
        [TestCase( "48_Delta_Event.bin", ExpectedResult = 1 )]
        [TestCase( "50_Delta_Event.bin", ExpectedResult = 3 )]
        [TestCase( "54_Delta_Event.bin", ExpectedResult = 1 )]
        [TestCase( "57_Delta_Event.bin", ExpectedResult = 3 )]
        [TestCase( "61_Delta_Event.bin", ExpectedResult = 1 )]
        [TestCase( "64_Delta_Event.bin", ExpectedResult = 3 )]
        [TestCase( "68_Delta_Event.bin", ExpectedResult = 1 )]
        [TestCase( "71_Delta_Event.bin", ExpectedResult = 3 )]
        [Order( 400 )]
        public int DeltaMessages( string fileName )
        {
            if ( Debugger.IsAttached )
            {
                MetaMessage();
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
    }
}