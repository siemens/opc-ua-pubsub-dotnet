// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using opc.ua.pubsub.dotnet.client.common;

namespace opc.ua.pubsub.dotnet.client
{
    /// <summary>
    ///     Base class for client credentials
    /// </summary>
    public abstract class ClientCredentials : IDisposable
    {
        /// <summary>
        ///     Client CA certificates chain
        /// </summary>
        public X509Certificate2[] ClientCaChain { get; private set; }
        /// <summary>
        ///     Client certificate
        /// </summary>
        public X509Certificate2 ClientCert { get; private set; }

        /// <summary>
        ///     Returns both, the client certificate and CA certificates chain, as one array of X509Certificate2
        /// </summary>
        public X509Certificate2[] ClientCertAndCaChain
        {
            get
            {
                if ( HasCertificates() )
                {
                    X509Certificate2[] certAndCaChain = new X509Certificate2[1 + ClientCaChain.Length];
                    certAndCaChain[0] = ClientCert;
                    Array.Copy( ClientCaChain, 0, certAndCaChain, 1, ClientCaChain.Length );
                    return certAndCaChain;
                }
                return null;
            }
        }

        /// <summary>
        ///     Retrieve user name and password for login (depending on the concrete cloud service)
        /// </summary>
        /// <param name="clientId">Client ID (publisher ID)</param>
        /// <param name="userName">out: user name string</param>
        /// <param name="password">out: password string</param>
        public abstract void GetUserNameAndPassword( string clientId, out string userName, out string password );

        /// <summary>
        ///     Returns wheter certificates have been imported
        /// </summary>
        /// <returns>true: certificates exists; false: not</returns>
        public bool HasCertificates()
        {
            return ClientCert != null && ClientCaChain != null;
        }

        /// <summary>
        ///     Import the client certificate and CA certificates chain from a byte array
        ///     containing a PKCS#12 file
        /// </summary>
        /// <param name="pkcs12Content">Content of the PKCS#12 file</param>
        /// <param name="pkcs12Password">Password of the PKCS#12 file</param>
        public void Import( byte[] pkcs12Content, string pkcs12Password = "" )
        {
            // note: handle exceptions during certificate import in the calling function

            // get the client certificate only
            ClientCert = new X509Certificate2( pkcs12Content, pkcs12Password, X509KeyStorageFlags.Exportable );

            // import all certificates (client certificate and all CA certificats) in a collection
            X509Certificate2Collection clientCaCertsCollection = new X509Certificate2Collection();
            clientCaCertsCollection.Import( pkcs12Content, pkcs12Password, X509KeyStorageFlags.Exportable );

            // CA certificates collection shall only contain the CA chain
            if ( clientCaCertsCollection.Contains( ClientCert ) )
            {
                clientCaCertsCollection.Remove( ClientCert );
            }

            // nothing to do if no CA certs
            if ( clientCaCertsCollection.Count == 0 )
            {
                return;
            }

            // copy from collection to X509Certificate2 array;
            // while doing this, check the order of the certificates chain
            ClientCaChain = new X509Certificate2[clientCaCertsCollection.Count];
            if ( clientCaCertsCollection.Count > 1 )
            {
                X509Certificate2 firstCert = clientCaCertsCollection[0];
                X509Certificate2 secondCert = clientCaCertsCollection[1];
                if ( !firstCert.Issuer.Equals( secondCert.Subject, StringComparison.InvariantCulture ) )
                {
                    // we must change the order, reverse order copy 
                    for ( int i = clientCaCertsCollection.Count - 1,
                              j = 0;
                          i >= 0;
                          i--, j++ )
                    {
                        ClientCaChain[j] = clientCaCertsCollection[i];
                    }
                }
                else
                {
                    // copy collection to X509Certificate2 array without changing the order
                    for ( int i = 0; i < clientCaCertsCollection.Count; i++ )
                    {
                        ClientCaChain[i] = clientCaCertsCollection[i];
                    }
                }
            }
            else
            {
                // only one CA cert
                ClientCaChain[0] = clientCaCertsCollection[0];
            }
        }

        /// <summary>
        ///     Returs whether the specific cloud service requires user name and password for login
        /// </summary>
        /// <returns>true: is required; false: not required</returns>
        public abstract bool IsUserNameAndPasswordRequired();

        #region IDisposable

        // Flag: Has Dispose already been called?
        private bool m_Disposed;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose( bool disposing )
        {
            if ( m_Disposed )
            {
                return;
            }
            if ( disposing )
            {
                // Free any other managed objects here.
            }

            // Free any unmanaged objects here.
            if ( ClientCert != null )
            {
                ClientCert.Dispose();
                ClientCert = null;
            }
            if ( ClientCaChain != null )
            {
                foreach ( X509Certificate2 cert in ClientCaChain )
                {
                    cert.Dispose();
                }
                ClientCaChain = null;
            }
            m_Disposed = true;
        }

        /// <summary>
        ///     Destructor
        /// </summary>
        ~ClientCredentials()
        {
            Dispose( false );
        }

        #endregion IDisposable
    }

    /// <summary>
    ///     Concrete implementation of client credentials for custom connection
    /// </summary>
    public class CustomClientCredentials : ClientCredentials
    {
        private readonly SecureString m_Password;
        private readonly string m_UserName;

        public CustomClientCredentials( string userName, SecureString password )
        {
            m_UserName = userName;
            m_Password = password;
        }

        /// <summary>
        ///     Retrieve user name and password for login for custom cloud service
        /// </summary>
        /// <param name="clientId">Client ID (publisher ID)</param>
        /// <param name="userName">out: user name string</param>
        /// <param name="password">out: password string</param>
        public override void GetUserNameAndPassword( string clientId, out string userName, out string password )
        {
            userName = m_UserName;
            password = m_Password?.ToString();
        }

        /// <summary>
        ///     Returs whether custom requires user name and password for login
        /// </summary>
        /// <returns>true: is required; false: not required</returns>
        public override bool IsUserNameAndPasswordRequired()
        {
            return m_UserName != null;
        }
    }

    /// <summary>
    ///     Concrete implementation of client credentials for Mindsphere connection
    /// </summary>
    public class MindsphereClientCredentials : ClientCredentials
    {
        /// <summary>
        ///     Retrieve user name and password for login for Mindsphere cloud service
        /// </summary>
        /// <param name="clientId">Client ID (publisher ID)</param>
        /// <param name="userName">out: user name string</param>
        /// <param name="password">out: password string</param>
        public override void GetUserNameAndPassword( string clientId, out string userName, out string password )
        {
            // UserName = "_CertificBearer"
            // Password = JWT
            JSONWebToken jwt = new JSONWebToken();
            userName = "_CertificateBearer";
            password = jwt.CreateJWT( ClientCert, ClientCaChain, clientId );
        }

        /// <summary>
        ///     Returs whether Mindspher requires user name and password for login
        /// </summary>
        /// <returns>true: is required; false: not required</returns>
        public override bool IsUserNameAndPasswordRequired()
        {
            return true;
        }
    }

    /// <summary>
    ///     Concrete implementation of client credentials for Azure connection
    /// </summary>
    public class AzureClientCredentials : ClientCredentials
    {
        private readonly string m_IoTHubName;

        public AzureClientCredentials( string iotHubName )
        {
            m_IoTHubName = iotHubName;
        }

        /// <summary>
        ///     Retrieve user name and password for login for Azure cloud service
        /// </summary>
        /// <param name="clientId">Client ID (publisher ID)</param>
        /// <param name="userName">out: user name string</param>
        /// <param name="password">out: password string</param>
        public override void GetUserNameAndPassword( string clientId, out string userName, out string password )
        {
            userName = m_IoTHubName + ".azure-devices.net/" + clientId + "/?api-version=2018-06-30";
            password = null;
        }

        /// <summary>
        ///     Returs whether Azure requires user name and password for login
        /// </summary>
        /// <returns>true: is required; false: not required</returns>
        public override bool IsUserNameAndPasswordRequired()
        {
            return true;
        }
    }
    public class AzureDpsClientCredentials : ClientCredentials
    {
        private readonly string m_DpsScopeIdentifier;


        public AzureDpsClientCredentials( string dpsScopeIdentifier )
        {
            m_DpsScopeIdentifier = dpsScopeIdentifier;
        }
        /// <summary>
        ///     Retrieve user name and password for login for Azure Device Provisioning service
        /// </summary>
        /// <param name="clientId">Client ID (publisher ID)</param>
        /// <param name="userName">out: user name string</param>
        /// <param name="password">out: password string</param>
        public override void GetUserNameAndPassword( string clientId, out string userName, out string password )
        {
            userName = m_DpsScopeIdentifier + "/registrations/" + clientId + "/api-version=2019-03-31";
            password = null;
        }

        /// <summary>
        ///     Returs whether Azure requires user name and password for login
        /// </summary>
        /// <returns>true: is required; false: not required</returns>
        public override bool IsUserNameAndPasswordRequired()
        {
            return true;
        }
    }
}