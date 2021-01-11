// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using log4net;

namespace opc.ua.pubsub.dotnet.binary.Storage
{
    public class LocalMetaStorage
    {
        private const int MaximumMetaMessagePerPublisher = 10;
        private static readonly ILog Logger = LogManager.GetLogger( MethodBase.GetCurrentMethod()
                                                                              .DeclaringType
                                                                  );
        /// <summary>
        ///     Used for storing the meta message only locally, not in Azure.
        ///     This is only used if the corresponding FeatureToggle is set.
        /// </summary>

        //protected readonly ConcurrentDictionary<string, ConcurrentDictionary<ConfigurationVersion, MetaFrame>> m_DeviceMetaMessages = new ConcurrentDictionary<string, ConcurrentDictionary<ConfigurationVersion, MetaFrame>>();
        protected readonly ConcurrentDictionary<string, ConcurrentDictionary<ushort, ConcurrentDictionary<ConfigurationVersion, MetaFrame>>> m_DeviceMetaMessages =
                new ConcurrentDictionary<string, ConcurrentDictionary<ushort, ConcurrentDictionary<ConfigurationVersion, MetaFrame>>>();

        public bool IsMetaMessageAlreadyKnown( string publisherID, ushort writerID, ConfigurationVersion cfg )
        {
            if ( string.IsNullOrWhiteSpace( publisherID ) )
            {
                return false;
            }
            if ( !m_DeviceMetaMessages.ContainsKey( publisherID ) )
            {
                return false;
            }
            ConcurrentDictionary<ushort, ConcurrentDictionary<ConfigurationVersion, MetaFrame>> writerIDs = m_DeviceMetaMessages[publisherID];
            if ( !writerIDs.ContainsKey( writerID ) )
            {
                return false;
            }
            ConcurrentDictionary<ConfigurationVersion, MetaFrame> metaMessages = writerIDs[writerID];
            return metaMessages.ContainsKey( cfg );
        }

        public MetaFrame RetrieveMetaMessageLocally( string publisherId, ushort writerID, ConfigurationVersion cfgVersion )
        {
            if ( string.IsNullOrEmpty( publisherId ) )
            {
                return null;
            }
            if ( !m_DeviceMetaMessages.TryGetValue( publisherId,
                                                    out ConcurrentDictionary<ushort, ConcurrentDictionary<ConfigurationVersion, MetaFrame>> deviceMetaMessages
                                                  ) )
            {
                return null;
            }
            if ( !deviceMetaMessages.TryGetValue( writerID, out ConcurrentDictionary<ConfigurationVersion, MetaFrame> metaDictionary ) )
            {
                return null;
            }
            if ( !metaDictionary.TryGetValue( cfgVersion, out MetaFrame metaMessage ) )
            {
                return null;
            }
            return metaMessage;
        }

        public void StoreMetaMessageLocally( MetaFrame metaFrame )
        {
            string               pubID         = metaFrame.NetworkMessageHeader.PublisherID.Value;
            ConfigurationVersion configVersion = metaFrame.ConfigurationVersion;
            ushort               writerID      = metaFrame.DataSetWriterID;
            if ( IsMetaMessageAlreadyKnown( pubID, writerID, configVersion ) )
            {
                return;
            }
            if ( Logger.IsInfoEnabled )
            {
                Logger.Info( $"Storing meta message for {pubID} with version {configVersion}." );
            }
            if ( !m_DeviceMetaMessages.TryGetValue( pubID, out ConcurrentDictionary<ushort, ConcurrentDictionary<ConfigurationVersion, MetaFrame>> publisherDictionary ) )
            {
                publisherDictionary = new ConcurrentDictionary<ushort, ConcurrentDictionary<ConfigurationVersion, MetaFrame>>();
                m_DeviceMetaMessages.TryAdd( pubID, publisherDictionary );
            }
            if ( !publisherDictionary.TryGetValue( writerID, out ConcurrentDictionary<ConfigurationVersion, MetaFrame> cfgDictionary ) )
            {
                cfgDictionary = new ConcurrentDictionary<ConfigurationVersion, MetaFrame>();
                publisherDictionary.TryAdd( writerID, cfgDictionary );
            }
            if ( cfgDictionary.Count > 10 )
            {
                MetaFrame dummy;
                cfgDictionary.TryRemove( cfgDictionary.Keys.OrderBy( a => a )
                                                      .First(),
                                         out dummy
                                       );
            }

            // Add the Meta Message if it doesn't exist yet or if it's already present, replace it.
            cfgDictionary.AddOrUpdate( configVersion, metaFrame, ( existingVersion, existingFrame ) => metaFrame );
        }
    }
}