// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Globalization;
using System.Net.Security;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using log4net;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Diagnostics;
using MQTTnet.Diagnostics.Logger;
using MQTTnet.Protocol;
using MQTTnet.Server;
using opc.ua.pubsub.dotnet.client.common;
using opc.ua.pubsub.dotnet.client.common.Settings;

namespace opc.ua.pubsub.dotnet.broker
{
    public static class MQTTBroker
    {
        private static readonly ILog Logger = LogManager.GetLogger( MethodBase.GetCurrentMethod()
                                                                              .DeclaringType
                                                                  );
        private static Settings Settings { get; set; }

        public static string GetSANFromCert( X509Certificate2 cert )
        {
            string san = string.Empty;
            if ( cert == null )
            {
                return san;
            }
            X509ExtensionCollection extensions = cert.Extensions;
            foreach ( X509Extension extension in extensions )
            {
                if ( extension.Oid.Value == "2.5.29.17" )
                {
                    san = extension.Format( true );
                    break;
                }
            }
            return san;
        }

        private static bool CustomCertificateValidationCallback( X509Certificate      certificate,
                                                                 X509Chain            chain,
                                                                 SslPolicyErrors      sslPolicyErrors,
                                                                 MqttClientTcpOptions arg4
                )
        {
            Logger.Info( $"SSL Policy Erros: {sslPolicyErrors}" );
            if ( certificate == null )
            {
                Logger.Info( "No client certificate received." );
            }
            else
            {
                Logger.Info( "Received Client Certificate:" );
                Logger.Info( certificate.ToString() );
            }
            if ( chain == null )
            {
                Logger.Info( "No certificate chain." );
            }
            else
            {
                Logger.Info( "Certificate Chain:" );
                foreach ( X509ChainElement element in chain.ChainElements )
                {
                    Logger.Info( $"Chain Element Information: {element.Information}" );
                    foreach ( X509ChainStatus elementStatus in element.ChainElementStatus )
                    {
                        Logger.Info( $"Status: {elementStatus.Status}" );
                        Logger.Info( $"Status Information: {elementStatus.StatusInformation}" );
                    }
                    Logger.Info( $"Certificate: {element.Certificate}" );
                }
            }
            Logger.Info( "Accepting Client Certifiacte" );
            return true;
        }

        private static (X509Certificate2, string) GetBrokerCertificate()
        {
            X509Certificate2 clientCert = null;
            string           password   = Settings.Broker.BrokerP12Password;
            if(string.IsNullOrWhiteSpace(Settings.Broker.BrokerP12))
                return (clientCert, password);
            try
            {
                clientCert = new X509Certificate2( Settings.Broker.BrokerP12, password );
            }
            catch ( CryptographicException ex )
            {
                Logger.Error( ex );
            }
            if ( clientCert == null )
            {
                Logger.Info( $"No Certificate found by using file '{Settings.Broker.BrokerP12}' and password '{Settings.Broker.BrokerP12Password}'." );
                clientCert = GetMachineCertifacte();
            }
            return ( clientCert, password );
        }

        private static X509Certificate2 GetMachineCertifacte()
        {
            X509Certificate2 machineCertificate = null;
            using ( X509Store store = new X509Store( StoreName.My, StoreLocation.LocalMachine ) )
            {
                store.Open( OpenFlags.ReadOnly );
                X509Certificate2Collection certs = store.Certificates.Find( X509FindType.FindBySubjectName, Environment.MachineName, true );
                if ( certs.Count > 0 )
                {
                    machineCertificate = certs[0];
                }
            }
            return machineCertificate;
        }

        private static void Main( string[] args )
        {
            Log4Net.Init();
            //TODO: Logging...
            //MqttNetGlobalLogger.LogMessagePublished += OnLogMessagePublished;
            MainAsync( args )
                   .GetAwaiter()
                   .GetResult();
        }

        private static async Task MainAsync( string[] args )
        {
            Settings = SettingManager.ReadConfiguration( args );
            IMqttServer              broker         = null;
            MqttServerOptionsBuilder optionsBuilder = new MqttServerOptionsBuilder();

            // TLS Support
            (X509Certificate2, string) tuple = GetBrokerCertificate();

            //MqttTcpChannel.CustomCertificateValidationCallback += CustomCertificateValidationCallback;
            if ( Settings.Broker.UseTLS )
            {
                Logger.Info( "TLS v1.2 requested. Trying to get Broker Certifacte." );
            }
            if ( Settings.Broker.UseTLS && tuple.Item1 != null )
            {
                Logger.Info( "Found certificate:" );
                string san = GetSANFromCert( tuple.Item1 );
                Logger.Info( "Subject Alternative Names from Certifacate:" );
                Logger.Info( san );
                Logger.Info( tuple.Item1 );
                optionsBuilder = optionsBuilder
                                .WithEncryptedEndpoint()
                                .WithEncryptedEndpointPort( 8883 )
                                .WithEncryptionSslProtocol( SslProtocols.Tls12)
                                .WithEncryptionCertificate( tuple.Item1.Export( X509ContentType.SerializedCert, tuple.Item2 ) );
                if ( Settings.Broker.UseMutualAuth )
                {
                    //broker = new MqttBroker(brokerCertificate, MqttSslProtocols.TLSv1_2, ValidateClientCertificate, UserCertificateSelectionCallback);
                }
            }
            else
            {
                Logger.Info( "No encryption used." );
                optionsBuilder = optionsBuilder.WithDefaultEndpointPort( 1883 );
            }
            optionsBuilder = optionsBuilder.WithConnectionValidator( ValidateUserAndPassword );
            broker         = new MqttFactory().CreateMqttServer();
            IMqttServerOptions options = optionsBuilder.Build();
            await broker.StartAsync( options );
            Console.ReadKey();
        }

        private static void OnLogMessagePublished( object sender, MqttNetLogMessagePublishedEventArgs eventArgs )
        {
            switch ( eventArgs.LogMessage.Level )
            {
                case MqttNetLogLevel.Error:
                    Logger.Error( eventArgs.LogMessage.Message, eventArgs.LogMessage.Exception );
                    break;

                case MqttNetLogLevel.Info:
                    Logger.Info( eventArgs.LogMessage.Message, eventArgs.LogMessage.Exception );
                    break;

                case MqttNetLogLevel.Warning:
                    Logger.Warn( eventArgs.LogMessage.Message, eventArgs.LogMessage.Exception );
                    break;

                case MqttNetLogLevel.Verbose:
                    Logger.Debug( eventArgs.LogMessage.Message, eventArgs.LogMessage.Exception );
                    break;

                default:
                    Logger.Fatal( eventArgs.LogMessage.Message, eventArgs.LogMessage.Exception );
                    break;
            }
        }

        private static void TraceListener( string format, params object[] args )
        {
            Logger.DebugFormat( CultureInfo.InvariantCulture, format, args );

            //Console.WriteLine(format, args);
        }

        /// <summary>
        ///     Check if JWT is enabled and validate the JWT if required.
        ///     If JWT is not enabled, authorization is granted if username & password are emtpy.
        /// </summary>
        private static void ValidateUserAndPassword( MqttConnectionValidatorContext mqttConnectionValidatorContext )
        {
            mqttConnectionValidatorContext.ReasonCode = MqttConnectReasonCode.Success;
            Logger.Info( $"Connection from client '{mqttConnectionValidatorContext.ClientId}'" );
            bool validateJWT = true;
            if ( string.IsNullOrEmpty( mqttConnectionValidatorContext.Username ) )
            {
                validateJWT = false;
                Logger.Info( "Connection request: username is null." );
            }
            else
            {
                Logger.Info( $"Username: {mqttConnectionValidatorContext.Username}" );
            }
            if ( string.IsNullOrEmpty( mqttConnectionValidatorContext.Password ) )
            {
                validateJWT = false;
                Logger.Info( "Connection request: username is null." );
            }
            else
            {
                Logger.Info( $"Password: {mqttConnectionValidatorContext.Password}" );
            }
            if ( validateJWT )
            {
                JSONWebToken tokenHandler = new JSONWebToken();
                tokenHandler.IsValid( mqttConnectionValidatorContext.Password );
            }
        }
    }
}