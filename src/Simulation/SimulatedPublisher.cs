// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using opc.ua.pubsub.dotnet.binary.DataPoints;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using log4net;
using opc.ua.pubsub.dotnet.client;
using opc.ua.pubsub.dotnet.client.common;
using opc.ua.pubsub.dotnet.client.common.Settings;
using opc.ua.pubsub.dotnet.simulation.Excel;
using opc.ua.pubsub.dotnet.simulation.Excel.Model;
using static opc.ua.pubsub.dotnet.client.ProcessDataSet;
using Client = opc.ua.pubsub.dotnet.client.Client;
using String = opc.ua.pubsub.dotnet.binary.String;

namespace opc.ua.pubsub.dotnet.simulation
{
    internal class SimulatedPublisher
    {
        private static Client s_Client;
        private static readonly ILog Logger =
                LogManager.GetLogger( typeof(SimulatedPublisher) );
        private static Settings Settings { get; set; }

        [SuppressMessage( "Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "This is just a demo. No need for globalization." )]
        private static void Main( string[] args )
        {
            Log4Net.Init();

            Settings                      = SettingManager.ReadConfiguration( args );
            Settings.Client.EnableLogging = true;

            // connect to broker and send data 
            MQTT();
            try
            {
                s_Client.Disconnect();
            }
            catch ( Exception e )
            {
                Logger.Error( "Exception during disconnect. ", e );
            }

            Console.WriteLine( "Press ENTER to exit." );
            Console.ReadLine();
        }

        private static void CreateClient()
        {
            s_Client = new Client( Settings, Settings.Simulation.PublisherID );

            // there was a decoding standard violation in a part of the meta frame
            // which has been fixed and we enable it here to use this fix
            s_Client.Options.LegacyFieldFlagEncoding = false;
        }

        private static void MQTT()
        {
            // read data sets configuration and data points from XML file
            ReadConfiguration configReader  = new ReadConfiguration( Settings.Simulation.InputFile );
            ParsedData        configuration = configReader.Read();
            if ( configuration == null )
            {
                return;
            }

            // replace publisher ID with entry in from settings.json
            if ( !string.IsNullOrWhiteSpace( Settings.Simulation.PublisherID ) )
            {
                configuration.CommonConfig.PublisherID = Settings.Simulation.PublisherID;
            }
            else
            {
                // PublisherID is not overridden in settings.json, use the value from the configuration
                Settings.Simulation.PublisherID = configuration.CommonConfig.PublisherID;
            }
            CreateClient();


            // delay before start?
            if ( Settings.Simulation.WaitBeforeStarting > 0 )
            {
                Thread.Sleep( Settings.Simulation.WaitBeforeStarting );
            }

            // Broker CA certificate
            if ( Settings.Client.UseTls )
            {
                if ( SettingManager.TryGetCertificateAsArray( Settings.Simulation.BrokerCACertDER, out byte[] brokerCaCert ) )
                {
                    s_Client.BrokerCACert = brokerCaCert;
                }
                else
                {
                    Logger.Error( $"Certificate file not found {Settings.Simulation.BrokerCACertDER}" );
                    return;
                }

                // client certificate and client CA 
                using ( MindsphereClientCredentials credentials = new MindsphereClientCredentials() )
                {
                    if ( SettingManager.TryGetCertificateAsArray( Settings.Simulation.ClientCertP12, out byte[] clientPkcs12Content ) )
                    {
                        try
                        {
                            credentials.Import( clientPkcs12Content, Settings.Simulation.ClientCertPassword );
                        }
                        catch ( Exception )
                        {
                            Logger.Error( "Exception during creation of client credentials" );
                            return;
                        }
                    }
                    else
                    {
                        Logger.Error( $"Certificate file not found {Settings.Simulation.ClientCertP12}" );
                        return;
                    }

                    // connect to broker
                    try
                    {
                        Logger.Info( "Connecting to the broker" );
                        s_Client.Connect( credentials );
                    }
                    catch ( Exception ex )
                    {
                        Logger.Error( "Unable to establish MQTT connection with broker", ex );
                        return;
                    }
                }
            }
            else
            {
                // connect to broker
                try
                {
                    Logger.Info( "Connecting to the broker" );
                    s_Client.Connect( );
                }
                catch ( Exception ex)
                {
                    Logger.Error( "Unable to establish MQTT connection with broker", ex );
                    return;
                }
            }

            // generate time series data set and publish time series messages
            try
            {
                Logger.Info( "Sending Time Series data set" );
                PublishTimeSeriesMessage( configuration );
            }
            catch ( Exception e )
            {
                Logger.Error( $"Unable to publish Time Series data set: {e.Message}" );
            }
            if ( Settings.Simulation.WaitAfterMetaMessage > 0 )
            {
                Thread.Sleep( Settings.Simulation.WaitAfterMetaMessage );
            }

            // generate events data set and publish events 
            try
            {
                Logger.Info( "Sending Events data set" );
                PublishEventMessage( configuration );
            }
            catch ( Exception e )
            {
                Logger.Error( $"Unable to publish Events data set: {e.Message}" );
            }
        }

        private static void PublishEventMessage( ParsedData configuration )
        {
            configuration.CommonConfig.DataSetWriterID = 1001;
            string         name        = "EventsDataSet";
            string         topicPrefix = Settings.Simulation.TopicPrefix;
            ProcessDataSet dataset     = s_Client.GenerateDataSet( name, configuration.CommonConfig.DataSetWriterID, DataSetType.Event );
            dataset.Description = new LocalizedText
                                  {
                                          Locale = new String( "en-US" ),
                                          Text   = new String( configuration.CommonConfig.MetaConfig.MetaDataDescription )
                                  };
            if ( !Settings.Simulation.SkipKey )
            {
                foreach ( KeyEntry item in configuration.KeyEntries )
                {
                    if ( !item.Name.Contains( "Event", StringComparison.InvariantCulture ) )
                    {
                        continue;
                    }
                    if ( CreateKeyMessage.GetDataPointValue( item, DataSetType.Event ) is ProcessDataPointValue datapoint )
                    {
                        datapoint.Name = item.Name;
                        dataset.UpdateDataPoint( datapoint );
                        if ( configuration.CommonConfig.EnumDescriptions.ContainsKey( item.DataType ) )
                        {
                            datapoint.EnumDescription = configuration.CommonConfig.EnumDescriptions[item.DataType];
                        }
                    }
                }
                if ( dataset.ProcessValueCount > 0 )
                {
                    s_Client.SendDataSet( dataset, topicPrefix, false );
                }
            }
            if ( Settings.Simulation.WaitAfterMetaMessage > 0 )
            {
                Thread.Sleep( Settings.Simulation.WaitAfterMetaMessage );
            }

            // DeltaChunks
            if ( !Settings.Simulation.SkipDelta && dataset.ProcessValueCount > 0 )
            {
                int i = 0;
                foreach ( DeltaEntry item in configuration.DeltaEntries )
                {
                    i++;
                    ProcessDataPointValue datapoint = CreateKeyMessage.GetDataPointValue( item, DataSetType.Event ) as ProcessDataPointValue;
                    if ( datapoint != null )
                    {
                        datapoint.Name = configuration.KeyEntries[i - 1]
                                                      .Name;
                        dataset.UpdateDataPoint( datapoint );
                    }
                }
                s_Client.SendDataSet( dataset, true );
            }
            if ( Settings.Simulation.WaitAfterMetaMessage > 0 )
            {
                Thread.Sleep( Settings.Simulation.WaitAfterMetaMessage );
            }

            // keep alive Chunks
            if ( !Settings.Simulation.SkipKeepAlive )
            {
                s_Client.SendKeepAlive( dataset, topicPrefix );
            }
        }

        private static void PublishTimeSeriesMessage( ParsedData configuration )
        {
            configuration.CommonConfig.DataSetWriterID = 1000;
            const string   name        = "TimeSeriesDataSet";
            string         topicPrefix = Settings.Simulation.TopicPrefix;
            ProcessDataSet dataset     = s_Client.GenerateDataSet( name, configuration.CommonConfig.DataSetWriterID, DataSetType.TimeSeries );
            dataset.Description = new LocalizedText
                                  {
                                          Locale = new String( "en-US" ),
                                          Text   = new String( configuration.CommonConfig.MetaConfig.MetaDataDescription )
                                  };

            // KeyEntries
            if ( !Settings.Simulation.SkipKey )
            {
                foreach ( KeyEntry item in configuration.KeyEntries )
                {
                    if ( item.Name.Contains( "Event", StringComparison.InvariantCulture ) )
                    {
                        continue;
                    }
                    ProcessDataPointValue datapoint = CreateKeyMessage.GetDataPointValue( item, DataSetType.TimeSeries ) as ProcessDataPointValue;
                    if ( datapoint != null )
                    {
                        datapoint.Name = item.Name; // configuration.KeyEntries[i].Name;

                        // add the data point to data set if it still not exists or 
                        // update the data point if it already exists in the data set
                        dataset.UpdateDataPoint( datapoint );
                        if ( configuration.CommonConfig.EnumDescriptions.ContainsKey( item.DataType ) )
                        {
                            datapoint.EnumDescription = configuration.CommonConfig.EnumDescriptions[item.DataType];
                        }
                    }
                }
                if ( dataset.ProcessValueCount > 0 )
                {
                    s_Client.SendDataSet( dataset, topicPrefix, false );
                }
            }
            if ( Settings.Simulation.WaitAfterMetaMessage > 0 )
            {
                Thread.Sleep( Settings.Simulation.WaitAfterMetaMessage );
            }

            // DeltaChunks
            if ( !Settings.Simulation.SkipDelta && dataset.ProcessValueCount > 0 )
            {
                int i = 0;
                foreach ( DeltaEntry item in configuration.DeltaEntries )
                {
                    ++i;

                    //if ( ProcessValueFactory.ConvertNodeIDSToString( item.DataType )
                    //                        .Contains( "Event" ) )
                    //{
                    //    continue;
                    //}
                    ProcessDataPointValue datapoint = CreateKeyMessage.GetDataPointValue( item, DataSetType.TimeSeries ) as ProcessDataPointValue;
                    if ( datapoint != null )
                    {
                        datapoint.Name = configuration.KeyEntries[i - 1]
                                                      .Name;
                        dataset.UpdateDataPoint( datapoint );
                    }
                }
                s_Client.SendDataSet( dataset, true );
            }
            if ( Settings.Simulation.WaitAfterMetaMessage > 0 )
            {
                Thread.Sleep( Settings.Simulation.WaitAfterMetaMessage );
            }

            // keep alive Chunks
            if ( !Settings.Simulation.SkipKeepAlive )
            {
                s_Client.SendKeepAlive( dataset, topicPrefix );
            }
        }
    }
}