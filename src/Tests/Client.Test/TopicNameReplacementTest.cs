// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Collections;
using NUnit.Framework;
using static opc.ua.pubsub.dotnet.client.ProcessDataSet;

namespace opc.ua.pubsub.dotnet.client.test
{
    /// <summary>
    ///     Tests of client functions
    /// </summary>
    public class TopicNameReplacementTests
    {
        private const ushort DatasetWriterID = 1000;
        private const string PublisherID     = "Test_Publisher_12345678";
        private const string VersionMS       = "v3";

        private static IEnumerable AzureTestCases
        {
            get
            {
                yield return new TestCaseData( "Meta", DataSetType.TimeSeries ).Returns( $"devices/{PublisherID}/messages/events/" )
                                                                               .SetName( "Azure {a}" );
                yield return new TestCaseData( "Meas", DataSetType.TimeSeries ).Returns( $"devices/{PublisherID}/messages/events/" )
                                                                               .SetName( "Azure {a}" );
                yield return new TestCaseData( "Meta", DataSetType.Event ).Returns( $"devices/{PublisherID}/messages/events/" )
                                                                          .SetName( "Azure {a}" );
                yield return new TestCaseData( "Meas", DataSetType.Event ).Returns( $"devices/{PublisherID}/messages/events/" )
                                                                          .SetName( "Azure {a}" );
            }
        }

        private static IEnumerable MDSPTestCases
        {
            get
            {
                yield return new TestCaseData( "Meta", DataSetType.TimeSeries ).Returns( $"c/{PublisherID}/o/opcua/{VersionMS}/u/m/t" )
                                                                               .SetName( "MDSP {a}" );
                yield return new TestCaseData( "Meas", DataSetType.TimeSeries ).Returns( $"c/{PublisherID}/o/opcua/{VersionMS}/u/d/t" )
                                                                               .SetName( "MDSP {a}" );
                yield return new TestCaseData( "Meta", DataSetType.Event ).Returns( $"c/{PublisherID}/o/opcua/{VersionMS}/u/m/e" )
                                                                          .SetName( "MDSP {a}" );
                yield return new TestCaseData( "Meas", DataSetType.Event ).Returns( $"c/{PublisherID}/o/opcua/{VersionMS}/u/d/e" )
                                                                          .SetName( "MDSP {a}" );
                yield return new TestCaseData( "File", DataSetType.TimeSeriesEventFile ).Returns( $"c/{PublisherID}/o/opcua/{VersionMS}/u/d/f" )
                                                                                        .SetName( "MDSP {a}" );
            }
        }

        [TestCaseSource( nameof(AzureTestCases) )]
        public string TopicNameReplacementTestAzure( string messageType, DataSetType dataSetType )
        {
            string topicNamePattern = "devices/{ClientID}/messages/events/";
            return Client.CreateTopicName( topicNamePattern, PublisherID, DatasetWriterID, messageType, dataSetType );
        }

        [TestCaseSource( nameof(MDSPTestCases) )]
        public string TopicNameReplacementTestMDSP( string messageType, DataSetType dataSetType )
        {
            string topicNamePattern = "c/{ClientID}/o/opcua/{VersionMS}/u/{T:d/t}{TM:m/t}{F:d/f}{FM:m/f}{E:d/e}{EM:m/e}";
            return Client.CreateTopicName( topicNamePattern, PublisherID, DatasetWriterID, messageType, dataSetType );
        }
    }
}