// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using log4net;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace opc.ua.pubsub.dotnet.common
{
    public class JSONWebToken
    {
        private static readonly ILog Logger = LogManager.GetLogger( MethodBase.GetCurrentMethod()
                                                                              .DeclaringType
                                                                  );

        public string CreateJWT( X509Certificate2   clientCertificate,
                                 X509Certificate2[] caCertificates,
                                 string             publisherID,
                                 string             tenantName   = null,
                                 bool               forSouthGate = false
                )
        {
            SigningCredentials sigCreds = new SigningCredentials( new X509SecurityKey( clientCertificate ),
                                                                  SecurityAlgorithms.RsaSha256Signature
                                                                );
            string tokenID = Guid.NewGuid()
                                 .ToString();
            JwtHeader jwtHeader = new JwtHeader( sigCreds );
            jwtHeader.Clear();
            jwtHeader.Add( "alg", "RS256" );
            /* The "x5c" (X.509 certificate chain) Header Parameter contains the
               X.509 public key certificate or certificate chain [RFC5280]
               corresponding to the key used to digitally sign the JWS.  The
               certificate or certificate chain is represented as a JSON array of
               certificate value strings.  Each string in the array is a
               base64-encoded (Section 4 of [RFC4648] -- not base64url-encoded) DER
               [ITU.X690.2008] PKIX certificate value.  The certificate containing
               the public key corresponding to the key used to digitally sign the
               JWS MUST be the first certificate.  This MAY be followed by
               additional certificates, with each subsequent certificate being the
               one used to certify the previous one.  The recipient MUST validate
               the certificate chain according to RFC 5280 [RFC5280] and consider
               the certificate or certificate chain to be invalid if any validation
               failure occurs.  Use of this Header Parameter is OPTIONAL.
             */
            List<string> exportedCerts = new List<string>();
            exportedCerts.Add( ExportToPEM( clientCertificate ) );
            foreach ( X509Certificate2 caCert in caCertificates )
            {
                exportedCerts.Add( ExportToPEM( caCert ) );
            }
            jwtHeader.Add( "x5c", exportedCerts.ToArray() );
            jwtHeader.Add( "typ", "JWT" );
            JwtPayload jwtPayload = new JwtPayload( new List<Claim>
                                                    {
                                                            new Claim( "jti", tokenID ),     // Unique Token ID
                                                            new Claim( "sub", publisherID ), // Publisher ID
                                                            new Claim( "ten",
                                                                       string.IsNullOrEmpty( tenantName ) ? "MQTTbroker" : tenantName
                                                                     ),                     // Will not be used by broker, but must be present in Mindsphere
                                                            new Claim( "iss", publisherID ) // Issuer
                                                    }
                                                  );
            DateTime utcNow   = DateTime.UtcNow;
            DateTime validity = utcNow.AddMinutes( 30 );
            DateTime unixTime = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
            int iat = (int)Math.Floor( utcNow.Subtract( unixTime )
                                             .TotalSeconds
                                     );
            int exp = (int)Math.Floor( validity.Subtract( unixTime )
                                               .TotalSeconds
                                     );

            //jwtPayload.Add("nbf", iat); // not valid before
            jwtPayload.Add( "exp", exp ); // valid until
            jwtPayload.Add( "iat", iat ); // issue time
            jwtPayload.Add( "aud",
                            new List<string>
                            {
                                    forSouthGate ? "southgate" : "MQTTBroker"
                            }
                          );
            jwtPayload.Add( "schemas",
                            new List<string>
                            {
                                    "urn:siemens:mindsphere:v1"
                            }
                          );
            JwtSecurityToken        securityToken           = new JwtSecurityToken( jwtHeader, jwtPayload );
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            string                  jwt                     = jwtSecurityTokenHandler.WriteToken( securityToken );
            return jwt;
        }

        public static string ExportToPEM( X509Certificate cert )
        {
            return Convert.ToBase64String( cert.Export( X509ContentType.Cert ) );
        }

        public bool IsValid( string token )
        {
            Logger.Info( "Validating JWT..." );
            if ( string.IsNullOrWhiteSpace( token ) )
            {
                Logger.Info( "JWT is empty." );
                return false;
            }
            if ( Logger.IsDebugEnabled )
            {
                Logger.Debug( $"Token: {token}" );
            }
            JwtSecurityToken jwtSecurityToken = null;
            try
            {
                jwtSecurityToken = new JwtSecurityToken( token );
            }
            catch ( ArgumentException argumentException )
            {
                Logger.Error( "Unable to parse JWT." );
                Logger.Error( argumentException );
                Logger.Error( $"Received string: {token}" );
                return false;
            }
            Dictionary<string, string> plainClaims = new Dictionary<string, string>();
            List<X509Certificate2>     certs       = new List<X509Certificate2>();
            if ( jwtSecurityToken.Header == null )
            {
                Logger.Error( "No headers found in JWT. Aborting." );
                return false;
            }
            Logger.Info( "Searching for certificates in JWT header (x5c)" );
            foreach ( KeyValuePair<string, object> pair in jwtSecurityToken.Header )
            {
                if ( pair.Key.Equals( "x5c", StringComparison.OrdinalIgnoreCase ) )
                {
                    Logger.Debug( $"{pair.Key}:\t\t{pair.Value}" );
                    if ( pair.Value is JArray jsonArray )
                    {
                        foreach ( JToken jToken in jsonArray )
                        {
                            byte[]           rawBytes = Encoding.UTF8.GetBytes( jToken.Value<string>() );
                            X509Certificate2 cert     = null;
                            try
                            {
                                cert = new X509Certificate2( rawBytes );
                            }
                            catch ( CryptographicException cryptographicException )
                            {
                                Logger.Error( "Unable to parse certificate from x5c array." );
                                Logger.Error( cryptographicException );
                            }
                            if ( cert != null )
                            {
                                certs.Add( cert );
                                Logger.Info( "Found certificate in JWT:" );
                                Logger.Info( cert );
                            }
                        }
                    }
                }
            }
            Logger.Info( "Claims from JWT:" );
            foreach ( Claim claim in jwtSecurityToken.Claims )
            {
                if ( plainClaims.ContainsKey( claim.Type ) )
                {
                    continue;
                }
                plainClaims.Add( claim.Type, claim.Value );
                Logger.Info( $"{claim.Type}:\t\t{claim.Value}" );
            }
            return ValidateClaims( plainClaims ) && ValidateCertificates( certs );
        }

        private bool ValidateCertificates( List<X509Certificate2> certificates )
        {
            return true;
        }

        private bool ValidateClaims( Dictionary<string, string> claims )
        {
            return true;
        }
    }
}