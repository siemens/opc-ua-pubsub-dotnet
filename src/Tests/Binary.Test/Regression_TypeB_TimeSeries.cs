// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.IO;
using System.Reflection;
using Binary;
using Binary.Decode;
using Binary.Messages;
using Binary.Messages.Delta;
using Binary.Messages.Key;
using Binary.Messages.Meta;
using NUnit.Framework;

namespace opc.ua.pubsub.dotnet.binary.test
{
    public class Regression_TypeB_TimeSeries
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
                                           "TimeSeries"
                                         );
            Decoder = new DecodeMessage( new EncodingOptions
                                         {
                                                 LegacyFieldFlagEncoding = false
                                         }
                                       );
        }

        [Test]
        [Order( 100 )]
        public void MetaMessage()
        {
            string         chunkFilePath = Path.Combine( TestDataFolder, "02_Meta_TimeSeries.bin" );
            byte[]         rawMessage    = File.ReadAllBytes( chunkFilePath );
            NetworkMessage message       = null;
            Assert.DoesNotThrow( () => message = Decoder.ParseBinaryMessage( rawMessage ) );
            Assert.That( message,                                        Is.Not.Null );
            Assert.That( message.NetworkMessageHeader.PublisherID.Value, Is.EqualTo( PublisherID ) );
            Assert.That( message,                                        Is.InstanceOf( typeof(MetaFrame) ) );
            MetaFrame metaMessage = (MetaFrame)message;
            Assert.That( metaMessage.DataSetWriterID,         Is.EqualTo( 1000 ) );
            Assert.That( metaMessage.FieldMetaDataList.Count, Is.EqualTo( 88 ) );

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
            string         filePath   = Path.Combine( TestDataFolder, "03_Key_TimeSeries.bin" );
            byte[]         rawMessage = File.ReadAllBytes( filePath );
            NetworkMessage message    = null;
            Assert.DoesNotThrow( () => message = Decoder.ParseBinaryMessage( rawMessage ) );
            Assert.That( message,                                        Is.Not.Null );
            Assert.That( message.NetworkMessageHeader.PublisherID.Value, Is.EqualTo( PublisherID ) );
            Assert.That( message,                                        Is.InstanceOf( typeof(KeyFrame) ) );
            KeyFrame keyMessage = (KeyFrame)message;
            Assert.That( keyMessage.DataSetMessageSequenceNumber, Is.EqualTo( 0 ) );
            Assert.That( keyMessage.Items.Count,                  Is.EqualTo( 88 ) );
        }

        [TestCase( "05_Delta_TimeSeries.bin", ExpectedResult = 20 )]
        [TestCase( "06_Delta_TimeSeries.bin", ExpectedResult = 7 )]
        [TestCase( "08_Delta_TimeSeries.bin", ExpectedResult = 37 )]
        [TestCase( "09_Delta_TimeSeries.bin", ExpectedResult = 1 )]
        [TestCase( "11_Delta_TimeSeries.bin", ExpectedResult = 27 )]
        [TestCase( "13_Delta_TimeSeries.bin", ExpectedResult = 1 )]
        [TestCase( "18_Delta_TimeSeries.bin", ExpectedResult = 17 )]
        [TestCase( "19_Delta_TimeSeries.bin", ExpectedResult = 9 )]
        [TestCase( "20_Delta_TimeSeries.bin", ExpectedResult = 7 )]
        [TestCase( "21_Delta_TimeSeries.bin", ExpectedResult = 5 )]
        [TestCase( "23_Delta_TimeSeries.bin", ExpectedResult = 23 )]
        [TestCase( "24_Delta_TimeSeries.bin", ExpectedResult = 4 )]
        [TestCase( "26_Delta_TimeSeries.bin", ExpectedResult = 2 )]
        [TestCase( "27_Delta_TimeSeries.bin", ExpectedResult = 24 )]
        [TestCase( "28_Delta_TimeSeries.bin", ExpectedResult = 5 )]
        [TestCase( "29_Delta_TimeSeries.bin", ExpectedResult = 7 )]
        [TestCase( "31_Delta_TimeSeries.bin", ExpectedResult = 21 )]
        [TestCase( "32_Delta_TimeSeries.bin", ExpectedResult = 6 )]
        [TestCase( "34_Delta_TimeSeries.bin", ExpectedResult = 26 )]
        [TestCase( "35_Delta_TimeSeries.bin", ExpectedResult = 4 )]
        [TestCase( "36_Delta_TimeSeries.bin", ExpectedResult = 8 )]
        [TestCase( "38_Delta_TimeSeries.bin", ExpectedResult = 14 )]
        [TestCase( "39_Delta_TimeSeries.bin", ExpectedResult = 7 )]
        [TestCase( "42_Delta_TimeSeries.bin", ExpectedResult = 37 )]
        [TestCase( "43_Delta_TimeSeries.bin", ExpectedResult = 1 )]
        [TestCase( "45_Delta_TimeSeries.bin", ExpectedResult = 27 )]
        [TestCase( "47_Delta_TimeSeries.bin", ExpectedResult = 1 )]
        [TestCase( "49_Delta_TimeSeries.bin", ExpectedResult = 6 )]
        [TestCase( "51_Delta_TimeSeries.bin", ExpectedResult = 26 )]
        [TestCase( "52_Delta_TimeSeries.bin", ExpectedResult = 6 )]
        [TestCase( "53_Delta_TimeSeries.bin", ExpectedResult = 6 )]
        [TestCase( "55_Delta_TimeSeries.bin", ExpectedResult = 17 )]
        [TestCase( "56_Delta_TimeSeries.bin", ExpectedResult = 4 )]
        [TestCase( "58_Delta_TimeSeries.bin", ExpectedResult = 26 )]
        [TestCase( "59_Delta_TimeSeries.bin", ExpectedResult = 4 )]
        [TestCase( "60_Delta_TimeSeries.bin", ExpectedResult = 8 )]
        [TestCase( "62_Delta_TimeSeries.bin", ExpectedResult = 21 )]
        [TestCase( "63_Delta_TimeSeries.bin", ExpectedResult = 6 )]
        [TestCase( "65_Delta_TimeSeries.bin", ExpectedResult = 26 )]
        [TestCase( "66_Delta_TimeSeries.bin", ExpectedResult = 3 )]
        [TestCase( "67_Delta_TimeSeries.bin", ExpectedResult = 9 )]
        [TestCase( "69_Delta_TimeSeries.bin", ExpectedResult = 29 )]
        [TestCase( "70_Delta_TimeSeries.bin", ExpectedResult = 7 )]
        [TestCase( "72_Delta_TimeSeries.bin", ExpectedResult = 37 )]
        [TestCase( "73_Delta_TimeSeries.bin", ExpectedResult = 1 )]
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