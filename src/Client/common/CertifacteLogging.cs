// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Security.Cryptography.X509Certificates;
using log4net;

namespace opc.ua.pubsub.dotnet.client.common
{
    public static class CertifacteLogging
    {
        public static void LogCertifacte( X509Certificate2 certificate, ILog logger )
        {
            logger.Error( $"Subject: {certificate.Subject}" );
            logger.Error( $"Issuer: {certificate.Issuer}" );
            logger.Error( $"Version: {certificate.Version}" );
            logger.Error( $"Valid Date: {certificate.NotBefore}" );
            logger.Error( $"Expiry Date: {certificate.NotAfter}" );
            logger.Error( $"Thumbprint: {certificate.Thumbprint}" );
            logger.Error( $"Serial Number: {certificate.SerialNumber}" );
            logger.Error( $"Friendly Name: {certificate.PublicKey.Oid.FriendlyName}" );
            logger.Error( $"Public Key Format: {certificate.PublicKey.EncodedKeyValue.Format( true )}" );
            logger.Error( $"Raw Data Length: {certificate.RawData.Length}" );
            logger.Error( $"Certificate to string: {certificate.ToString( true )}" );

            // Does not work with .NET Core 2.2 :-(
            //logger.Error($"Certificate to XML String: {certificate.PublicKey.Key.ToXmlString(false)}");
        }

        public static void LogCertificateChain( X509Chain chain, ILog logger )
        {
            logger.Error( "Chain Information" );
            logger.Error( $"Chain revocation flag: {chain.ChainPolicy.RevocationFlag}" );
            logger.Error( $"Chain revocation mode: {chain.ChainPolicy.RevocationMode}" );
            logger.Error( $"Chain verification flag: {chain.ChainPolicy.VerificationFlags}" );
            logger.Error( $"Chain verification time: {chain.ChainPolicy.VerificationTime}" );
            logger.Error( $"Chain status length: {chain.ChainStatus.Length}" );
            logger.Error( $"Chain application policy count: {chain.ChainPolicy.ApplicationPolicy.Count}" );
            logger.Error( $"Chain certificate policy count: {chain.ChainPolicy.CertificatePolicy.Count}" );
            logger.Error( "Chain Element Information" );
            logger.Error( $"Number of chain elements: {chain.ChainElements.Count}" );
            logger.Error( $"Chain elements synchronized? {chain.ChainElements.IsSynchronized}" );
            foreach ( X509ChainElement element in chain.ChainElements )
            {
                logger.Error( $"Element issuer name: {element.Certificate.Issuer}" );
                logger.Error( $"Element certificate valid until: {element.Certificate.NotAfter}" );
                logger.Error( $"Element certificate is valid: {element.Certificate.Verify()}" );
                logger.Error( $"Element error status length: {element.ChainElementStatus.Length}" );
                logger.Error( $"Element information: {element.Information}" );
                logger.Error( $"Number of element extensions: {element.Certificate.Extensions.Count}{1}" );
                if ( chain.ChainStatus.Length > 1 )
                {
                    for ( int index = 0; index < element.ChainElementStatus.Length; index++ )
                    {
                        logger.Error( element.ChainElementStatus[index]
                                             .Status
                                    );
                        logger.Error( element.ChainElementStatus[index]
                                             .StatusInformation
                                    );
                    }
                }
            }
        }
    }
}