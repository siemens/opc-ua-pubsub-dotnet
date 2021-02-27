// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using opc.ua.pubsub.dotnet.binary.DataPoints;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using opc.ua.pubsub.dotnet.binary.Messages.Meta.Structure;
using log4net;
using File = opc.ua.pubsub.dotnet.binary.DataPoints.File;
using String = opc.ua.pubsub.dotnet.binary.String;

namespace opc.ua.pubsub.dotnet.visualizer.UI
{
    public class UpdateValues
    {
        public static Dictionary<ushort, Dictionary<int, List<ProcessDataPoint>>> m_GroupedRowItem = new Dictionary<ushort, Dictionary<int, List<ProcessDataPoint>>>();
        private static readonly ILog Logger = LogManager.GetLogger( MethodBase.GetCurrentMethod()
                                                                              .DeclaringType
                                                                  );
        private readonly VisualizerForm                                                            m_Form;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<ushort, BindingSource>> m_PublisherBindings;
        private readonly BlockingCollection<DataPointCollection>                                   m_ValueQueue;
        private          CancellationTokenSource                                                   m_CancellationTokenSource;

        public UpdateValues( BlockingCollection<DataPointCollection>                                   queue,
                             ConcurrentDictionary<string, ConcurrentDictionary<ushort, BindingSource>> bindingSources,
                             VisualizerForm                                                            form
                )
        {
            m_ValueQueue        = queue;
            m_Form              = form;
            m_PublisherBindings = bindingSources;
        }

        public MetaFrame MetaMessage { get; set; }

        public void Start()
        {
            m_CancellationTokenSource = new CancellationTokenSource();
            try
            {
                foreach ( DataPointCollection values in m_ValueQueue.GetConsumingEnumerable( m_CancellationTokenSource.Token ) )
                {
                    if ( m_CancellationTokenSource.Token.IsCancellationRequested )
                    {
                        Logger.Info( "Cancellation requested for updating values." );
                        return;
                    }
                    InvokeUpdate( values );
                }
            }
            catch ( OperationCanceledException e )
            {
                Logger.Info( "Cancellation via exception for updating values.", e );
            }
        }

        public void Stop()
        {
            m_CancellationTokenSource.Cancel();
        }

        private int GetGridListItemRowIndex( BindingSource bs, DataPointBase dp )
        {
            if ( bs == null || bs.Count == 0 )
            {
                return -1;
            }
            for ( int i = 0; i < bs.List.Count; i++ )
            {
                ProcessDataPoint tempDp = bs.List[i] as ProcessDataPoint;
                if ( tempDp.Name == dp.Name )
                {
                    return i;
                }
            }
            return -1;
        }

        private string GetValueFromDataPointValue( DataPointValue dpv )
        {
            string text = string.Empty;
            if ( dpv is File file )
            {
                return $"NAME [{file.Path}] [{file.Content.Length}]";
            }
            if ( dpv is ProcessDataPointValue pdpv )
            {
                if ( dpv.EnumDescription != null )
                {
                    StructureDescription desc = null; //DataPointsManager.GetEnumStructureDescription(dpv.EnumDescription.Name.Name.Value);
                    if ( desc != null )
                    {
                        string enumName = desc.Name.Name.Value.Substring( 10 );
                        foreach ( EnumField enumField in dpv.EnumDescription.Fields )
                        {
                            if ( enumField.Value == (int)pdpv.Value )
                            {
                                text = string.Concat( enumField.Name.Value, " [ ", enumField.Value, " ]" );
                                break;
                            }
                        }
                    }
                }
                else
                {
                    text = string.Concat( text, "[ ", pdpv.Value, " ]" );
                    if ( pdpv is CounterValue )
                    {
                        text = string.Concat( text,
                                              "-[ ",
                                              pdpv.GetAttributeValue( CounterValue.QuantityAttributeName )
                                                  .ToString(),
                                              " ]"
                                            );
                    }
                    if ( dpv is StepPosValue || dpv is StepPosEvent )
                    {
                        text = string.Concat( text,
                                              "-[ ",
                                              pdpv.GetAttributeValue( StepPosValue.TransientAttributeName )
                                                  .ToString(),
                                              " ]"
                                            );
                    }
                    if ( pdpv is ComplexMeasuredValue )
                    {
                        text = string.Concat( text,
                                              "-[ ",
                                              pdpv.GetAttributeValue( ComplexMeasuredValue.AngleAttributeName )
                                                  .ToString(),
                                              " ]"
                                            );
                    }
                }
            }
            if ( string.IsNullOrEmpty( text ) )
            {
                text = "Could not parse value";
            }
            return text;
        }

        private void InvokeUpdate( DataPointCollection values )
        {
            if ( m_Form.InvokeRequired )
            {
                m_Form.Invoke( new Action<DataPointCollection>( InvokeUpdate ), values );
            }
            else
            {
                if ( !m_PublisherBindings.TryGetValue( values.PublisherID, out ConcurrentDictionary<ushort, BindingSource> writerDictionary ) )
                {
                    return;
                }
                if ( !writerDictionary.TryGetValue( values.DataSetWriterID, out BindingSource bs ) )
                {
                    return;
                }
                if ( bs == null || bs.Count == 0 )
                {
                    return;
                }
                Dictionary<int, List<ProcessDataPoint>> dataPoints = new Dictionary<int, List<ProcessDataPoint>>();
                if ( m_GroupedRowItem.ContainsKey( values.DataSetWriterID ) )
                {
                    dataPoints = m_GroupedRowItem[values.DataSetWriterID];
                }
                else
                {
                    m_GroupedRowItem[values.DataSetWriterID] = dataPoints;
                }
                foreach ( DataPointValue dataPointValue in values.Values )
                {
                    foreach ( object item in bs.List )
                    {
                        if ( !( item is DataPointBase dp ) )
                        {
                            continue;
                        }
                        if ( item is FileDataPoint )
                        {
                            string fileName = Path.GetFileName( dataPointValue.Name );
                            if ( dp.Name != fileName )
                            {
                                continue;
                            }
                        }
                        else if ( dp.Name != dataPointValue.Name )
                        {
                            continue;
                        }
                        if ( item is ProcessDataPoint pdp )
                        {
                            if ( dataPointValue is ProcessDataPointValue pdv )
                            {
                                if ( pdv.Properties != null )
                                {
                                    foreach ( binary.Messages.Meta.KeyValuePair keyValuePair in pdv.Properties )
                                    {
                                        string key = keyValuePair.Name.Name.Value;
                                        if ( key.Equals( "Unit", StringComparison.InvariantCultureIgnoreCase ) )
                                        {
                                            pdp.Unit = ( keyValuePair.Value as String ).Value;
                                        }
                                        if ( key.Equals( "Prefix", StringComparison.InvariantCultureIgnoreCase ) )
                                        {
                                            pdp.Prefix = ( keyValuePair.Value as String ).Value;
                                        }
                                    }
                                }
                                string        value       = GetValueFromDataPointValue( pdv );
                                StringBuilder sb          = new StringBuilder( value );
                                bool          insertBlank = true;
                                if ( !string.IsNullOrWhiteSpace( pdp.Prefix ) )
                                {
                                    if ( insertBlank )
                                    {
                                        sb.Append( " " );
                                        insertBlank = false;
                                    }
                                    sb.Append( pdp.Prefix );
                                }
                                if ( !string.IsNullOrWhiteSpace( pdp.Unit ) )
                                {
                                    if ( insertBlank )
                                    {
                                        sb.Append( " " );
                                    }
                                    sb.Append( pdp.Unit );
                                }
                                pdp.Value             = sb.ToString();
                                pdp.MDSPQuality       = pdv.MDSPQuality;
                                pdp.Orcat             = pdv.Orcat;
                                pdp.Quality           = pdv.Quality;
                                pdp.Custom            = pdv.Custom;
                                pdp.OriginalTimeStamp = DateTime.FromFileTimeUtc( pdv.Timestamp );
                                pdp.TimeStamp         = DateTime.FromFileTimeUtc( values.Timestamp );
                            }
                        }
                        if ( item is FileDataPoint fdp )
                        {
                            if ( dataPointValue is File file )
                            {
                                fdp.Path         = file.Path.Value;
                                fdp.LastModified = DateTime.FromFileTimeUtc( file.Time );
                                if ( file.Content != null )
                                {
                                    fdp.Size = file.Content.Length;
                                }
                                fdp.Content  = file.Content;
                                fdp.FileType = file.FileType.Value;
                            }
                        }
                    }
                }
            }
        }
    }
}