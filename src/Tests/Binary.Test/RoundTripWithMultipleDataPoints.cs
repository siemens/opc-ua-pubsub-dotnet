// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections;
using System.Collections.Generic;
using Binary;
using Binary.DataPoints;
using Binary.Decode;
using Binary.Messages;
using Binary.Messages.Delta;
using Binary.Messages.Key;
using Binary.Messages.Meta;
using NUnit.Framework;
using opc.ua.pubsub.dotnet.client;

namespace opc.ua.pubsub.dotnet.binary.test
{
    public class RoundTripWithMultipleDataPoints
    {
        private static IEnumerable MultipleDataPointsTestCases
        {
            get
            {
                yield return new TestCaseData( new List<ProcessDataPointValue>
                                               {
                                                       new SPSEvent
                                                       {
                                                               Value     = true,
                                                               Name      = "MySPSValue",
                                                               Orcat     = OrcatConstants.Process,
                                                               Quality   = QualityConstants.Invalid,
                                                               Unit      = "",
                                                               Prefix    = "",
                                                               Timestamp = DateTime.UtcNow.ToFileTimeUtc()
                                                       },
                                                       new DPSEvent
                                                       {
                                                               Value     = 5,
                                                               Name      = "MyDPSValue",
                                                               Orcat     = OrcatConstants.General_Interrogation,
                                                               Quality   = QualityConstants.Good,
                                                               Timestamp = DateTime.UtcNow.ToFileTimeUtc()
                                                       },
                                                       new IntegerEvent
                                                       {
                                                               Value     = 1248,
                                                               Name      = "MyIntValue",
                                                               Orcat     = OrcatConstants.Automatic_Bay,
                                                               Quality   = QualityConstants.Inaccurate,
                                                               Unit      = "m",
                                                               Prefix    = "k",
                                                               Timestamp = DateTime.UtcNow.ToFileTimeUtc()
                                                       },
                                                       new StringEvent
                                                       {
                                                               Value     = "TestStringValueContent",
                                                               Name      = "MyStringEvent",
                                                               Orcat     = OrcatConstants.Process,
                                                               Quality   = QualityConstants.Good,
                                                               Unit      = "",
                                                               Prefix    = "",
                                                               Timestamp = DateTime.UtcNow.ToFileTimeUtc()
                                                       },
                                                       new CounterValue
                                                       {
                                                               Value     = 2000,
                                                               Quantity  = 3.1415F,
                                                               Name      = "MyCounterValue",
                                                               Orcat     = OrcatConstants.Remote_Control,
                                                               Quality   = QualityConstants.Old_Data,
                                                               Unit      = "Wh",
                                                               Prefix    = "m",
                                                               Timestamp = DateTime.UtcNow.ToFileTimeUtc()
                                                       },
                                                       new MeasuredValue
                                                       {
                                                               Value     = (float)5.487,
                                                               Name      = "MyMeasuredValue",
                                                               Orcat     = OrcatConstants.Process,
                                                               Quality   = QualityConstants.Good,
                                                               Unit      = "A",
                                                               Prefix    = "µ",
                                                               Timestamp = DateTime.UtcNow.ToFileTimeUtc()
                                                       },
                                                       new ComplexMeasuredValue
                                                       {
                                                               Value     = (float)9.487,
                                                               Name      = "MyComplexMeasuredValue",
                                                               Angle     = 123.456F,
                                                               Orcat     = OrcatConstants.Process,
                                                               Quality   = QualityConstants.Good,
                                                               Unit      = "A",
                                                               Prefix    = "µ",
                                                               Timestamp = DateTime.UtcNow.ToFileTimeUtc()
                                                       },
                                                       new MeasuredValuesArray50
                                                       {
                                                               Value = new[]
                                                                       {
                                                                               1.1F,
                                                                               2.2F,
                                                                               3.3F,
                                                                               4.4F,
                                                                               5.5F,
                                                                               6.6F,
                                                                               7.7F,
                                                                               8.8F,
                                                                               9.9F,
                                                                               11F,
                                                                               12.1F,
                                                                               13.2F,
                                                                               14.3F,
                                                                               15.4F,
                                                                               16.5F,
                                                                               17.6F,
                                                                               18.7F,
                                                                               19.8F,
                                                                               20.9F,
                                                                               22F,
                                                                               23.1F,
                                                                               24.2F,
                                                                               25.3F,
                                                                               26.4F,
                                                                               27.5F,
                                                                               28.6F,
                                                                               29.7F,
                                                                               30.8F,
                                                                               31.9F,
                                                                               33F,
                                                                               34.1F,
                                                                               35.2F,
                                                                               36.3F,
                                                                               37.4F,
                                                                               38.5F,
                                                                               39.6F,
                                                                               40.7F,
                                                                               41.8F,
                                                                               42.9F,
                                                                               44F,
                                                                               45.1F,
                                                                               46.2F,
                                                                               47.3F,
                                                                               48.4F,
                                                                               49.5F,
                                                                               50.6F,
                                                                               51.7F,
                                                                               52.8F,
                                                                               53.9F,
                                                                               55F
                                                                       },
                                                               Name      = "MyMeasuredValueArray",
                                                               Orcat     = OrcatConstants.Process,
                                                               Quality   = QualityConstants.Good,
                                                               Unit      = "V",
                                                               Prefix    = "k",
                                                               Timestamp = DateTime.UtcNow.ToFileTimeUtc()
                                                       }
                                               }
                                             );
            }
        }

        [TestCaseSource( nameof(MultipleDataPointsTestCases) )]
        public void DeltaFrame( List<ProcessDataPointValue> dataPoints )
        {
            ProcessDataSet dataSet = new ProcessDataSet( "test-publisher", "test001", 123, ProcessDataSet.DataSetType.TimeSeries );
            foreach ( ProcessDataPointValue dataPoint in dataPoints )
            {
                dataSet.AddDataPoint( dataPoint );
            }
            DecodeMessage  decoder            = new DecodeMessage();
            byte[]         encodedMeta        = dataSet.GetEncodedMetaFrame( new EncodingOptions(), 1, true );
            byte[]         encodedDelta       = dataSet.GetEncodedDeltaFrame( 2 );
            NetworkMessage metaNetworkMessage = decoder.ParseBinaryMessage( encodedMeta );
            Assert.That( metaNetworkMessage, Is.Not.Null );
            Assert.That( metaNetworkMessage, Is.InstanceOf( typeof(MetaFrame) ) );
            NetworkMessage deltaNetworkMessage = decoder.ParseBinaryMessage( encodedDelta );
            Assert.That( deltaNetworkMessage, Is.Not.Null );
            Assert.That( deltaNetworkMessage, Is.InstanceOf( typeof(DeltaFrame) ) );
            DeltaFrame decodedDeltaMessage = (DeltaFrame)deltaNetworkMessage;
            Assert.That( decodedDeltaMessage,             Is.Not.Null );
            Assert.That( decodedDeltaMessage.Items,       Is.Not.Empty );
            Assert.That( decodedDeltaMessage.Items.Count, Is.EqualTo( dataPoints.Count ) );
            Assert.That( decodedDeltaMessage.Items,       Is.EqualTo( dataPoints ) );
        }

        [TestCaseSource( nameof(MultipleDataPointsTestCases) )]
        public void KeyFrame( List<ProcessDataPointValue> dataPoints )
        {
            ProcessDataSet dataSet = new ProcessDataSet( "test-publisher", "test001", 123, ProcessDataSet.DataSetType.TimeSeries );
            foreach ( ProcessDataPointValue dataPoint in dataPoints )
            {
                dataSet.AddDataPoint( dataPoint );
            }
            DecodeMessage  decoder            = new DecodeMessage();
            byte[]         encodedMeta        = dataSet.GetEncodedMetaFrame( new EncodingOptions(), 1, true );
            byte[]         encodedKey         = dataSet.GetEncodedKeyFrame( 2 );
            NetworkMessage metaNetworkMessage = decoder.ParseBinaryMessage( encodedMeta );
            Assert.That( metaNetworkMessage, Is.Not.Null );
            Assert.That( metaNetworkMessage, Is.InstanceOf( typeof(MetaFrame) ) );
            NetworkMessage keyNetworkMessage = decoder.ParseBinaryMessage( encodedKey );
            Assert.That( keyNetworkMessage, Is.Not.Null );
            Assert.That( keyNetworkMessage, Is.InstanceOf( typeof(KeyFrame) ) );
            KeyFrame decodedKeyMessage = (KeyFrame)keyNetworkMessage;
            Assert.That( decodedKeyMessage,             Is.Not.Null );
            Assert.That( decodedKeyMessage.Items,       Is.Not.Empty );
            Assert.That( decodedKeyMessage.Items.Count, Is.EqualTo( dataPoints.Count ) );
            Assert.That( decodedKeyMessage.Items,       Is.EqualTo( dataPoints ) );
        }

        [OneTimeSetUp]
        public void Setup() { }
    }
}