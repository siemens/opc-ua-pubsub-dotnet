// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections;
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
    public class RoundTripWithSingleDataPoint
    {
        private static IEnumerable SingleItemTestCases
        {
            get
            {
                yield return new TestCaseData( new DPSEvent
                                               {
                                                       Name      = "Sample DPS",
                                                       Value     = 2,
                                                       Orcat     = OrcatConstants.Process,
                                                       Quality   = QualityConstants.Good,
                                                       Timestamp = DateTime.Now.ToFileTimeUtc()
                                               }
                                             ).SetName( "{m}-DPSEvent" );
                yield return new TestCaseData( new SPSEvent
                                               {
                                                       Name      = "Sample SPS",
                                                       Value     = true,
                                                       Orcat     = OrcatConstants.Remote_Control,
                                                       Quality   = QualityConstants.Failure,
                                                       Timestamp = DateTime.Now.ToFileTimeUtc()
                                               }
                                             ).SetName( "{m}-SPSEvent" );
                yield return new TestCaseData( new IntegerEvent
                                               {
                                                       Name      = "Sample IntegerEvent",
                                                       Value     = 1234567,
                                                       Orcat     = OrcatConstants.General_Interrogation,
                                                       Quality   = QualityConstants.Invalid,
                                                       Timestamp = DateTime.Now.ToFileTimeUtc()
                                               }
                                             ).SetName( "{m}-IntegerEvent" );
                yield return new TestCaseData( new MeasuredValue
                                               {
                                                       Name      = "Sample MeasuredValue",
                                                       Value     = 3.14F,
                                                       Orcat     = OrcatConstants.Remote_Control,
                                                       Quality   = QualityConstants.Bad_Reference,
                                                       Timestamp = DateTime.Now.ToFileTimeUtc()
                                               }
                                             ).SetName( "{m}-MeasuredValue" );
                yield return new TestCaseData( new ComplexMeasuredValue
                                               {
                                                       Name      = "Sample ComplexMeasuredValue",
                                                       Value     = 6.67430F,
                                                       Angle     = 90.2F,
                                                       Orcat     = OrcatConstants.Remote_Control,
                                                       Quality   = QualityConstants.Bad_Reference,
                                                       Timestamp = DateTime.Now.ToFileTimeUtc()
                                               }
                                             ).SetName( "{m}-ComplexMeasuredValue" );
                yield return new TestCaseData( new MeasuredValuesArray50
                                               {
                                                       Name      = "Sample MeasuredValuesArray50",
                                                       Value     = new[] { 1F, 2F, 3F },
                                                       Orcat     = OrcatConstants.Remote_Control,
                                                       Quality   = QualityConstants.Bad_Reference,
                                                       Timestamp = DateTime.Now.ToFileTimeUtc()
                                               }
                                             ).SetName( "{m}-MeasuredValuesArray50" );
                yield return new TestCaseData( new CounterValue
                                               {
                                                       Name      = "Sample CounterValue",
                                                       Value     = 987654321,
                                                       Orcat     = OrcatConstants.Unknown,
                                                       Quality   = QualityConstants.Old_Data,
                                                       Timestamp = DateTime.Now.ToFileTimeUtc()
                                               }
                                             ).SetName( "{m}-CounterValue" );
                yield return new TestCaseData( new StringEvent
                                               {
                                                       Name      = "Sample CounterValue",
                                                       Value     = "Hello World",
                                                       Orcat     = OrcatConstants.Maintenance,
                                                       Quality   = QualityConstants.Test,
                                                       Timestamp = DateTime.Now.ToFileTimeUtc()
                                               }
                                             ).SetName( "{m}-StringEvent" );
            }
        }

        [OneTimeSetUp]
        public void Setup() { }

        [TestCaseSource( nameof(SingleItemTestCases) )]
        public void TestDeltaFrame( ProcessDataPointValue dataPoint )
        {
            ProcessDataSet dataSet = new ProcessDataSet( "test-publisher", "test001", 123, ProcessDataSet.DataSetType.TimeSeries );
            dataSet.AddDataPoint( dataPoint );
            DecodeMessage  decoder            = new DecodeMessage();
            byte[]         encodedMeta        = dataSet.GetEncodedMetaFrame( new EncodingOptions(), 1, true );
            byte[]         encodedKey         = dataSet.GetEncodedDeltaFrame( 2 );
            NetworkMessage metaNetworkMessage = decoder.ParseBinaryMessage( encodedMeta );
            Assert.That( metaNetworkMessage, Is.Not.Null );
            Assert.That( metaNetworkMessage, Is.InstanceOf( typeof(MetaFrame) ) );
            NetworkMessage deltaNetworkMessage = decoder.ParseBinaryMessage( encodedKey );
            Assert.That( deltaNetworkMessage, Is.Not.Null );
            Assert.That( deltaNetworkMessage, Is.InstanceOf( typeof(DeltaFrame) ) );
            DeltaFrame decodedDeltaMessage = (DeltaFrame)deltaNetworkMessage;
            Assert.That( decodedDeltaMessage,             Is.Not.Null );
            Assert.That( decodedDeltaMessage.Items,       Is.Not.Empty );
            Assert.That( decodedDeltaMessage.Items.Count, Is.EqualTo( 1 ) );
            ProcessDataPointValue decodedDataPoint = (ProcessDataPointValue)decodedDeltaMessage.Items[0];
            Common.AssertDataPointsAreEqual( dataPoint, decodedDataPoint );
        }

        [TestCaseSource( nameof(SingleItemTestCases) )]
        public void TestKeyFrame( ProcessDataPointValue dataPoint )
        {
            ProcessDataSet dataSet = new ProcessDataSet( "test-publisher", "test001", 123, ProcessDataSet.DataSetType.TimeSeries );
            dataSet.AddDataPoint( dataPoint );
            DecodeMessage  decoder            = new DecodeMessage();
            byte[]         encodedMeta        = dataSet.GetEncodedMetaFrame( new EncodingOptions(), 1, true );
            byte[]         encodedKey         = dataSet.GetEncodedKeyFrame( 2 );
            NetworkMessage metaNetworkMessage = decoder.ParseBinaryMessage( encodedMeta );
            Assert.That( metaNetworkMessage, Is.Not.Null );
            Assert.That( metaNetworkMessage, Is.InstanceOf( typeof(MetaFrame) ) );
            NetworkMessage keyNetworkMessage = decoder.ParseBinaryMessage( encodedKey );
            Assert.That( keyNetworkMessage, Is.Not.Null );
            Assert.That( keyNetworkMessage, Is.InstanceOf( typeof(KeyFrame) ) );
            KeyFrame              decodedKeyMessage = (KeyFrame)keyNetworkMessage;
            ProcessDataPointValue decodedDataPoint  = (ProcessDataPointValue)decodedKeyMessage.Items[0];
            Common.AssertDataPointsAreEqual( dataPoint, decodedDataPoint );
            Assert.That( decodedKeyMessage,             Is.Not.Null );
            Assert.That( decodedKeyMessage.Items,       Is.Not.Empty );
            Assert.That( decodedKeyMessage.Items.Count, Is.EqualTo( 1 ) );
        }
    }
}