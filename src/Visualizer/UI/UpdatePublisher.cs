// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using log4net;

namespace opc.ua.pubsub.dotnet.visualizer.UI
{
    public class UpdatePublisher
    {
        private static readonly ILog Logger = LogManager.GetLogger( MethodBase.GetCurrentMethod()
                                                                              .DeclaringType
                                                                  );
        private readonly VisualizerForm m_Form;

        //private Dictionary<string, int> KnownPublishersIndex;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<ushort, int>> m_KnownPublisherDictionary;
        private readonly BindingSource                                                   m_PublisherBindingSource;
        private readonly BlockingCollection<Publisher>                                   m_PublisherQueue;
        private          CancellationTokenSource                                         m_CancellationTokenSource;
        private          int                                                             m_LastPublisherIndex;

        public UpdatePublisher( BlockingCollection<Publisher> queue, BindingSource bindingSource, VisualizerForm form )
        {
            m_PublisherQueue         = queue;
            m_PublisherBindingSource = bindingSource;

            //KnownPublishersIndex = new Dictionary<string, int>();
            m_KnownPublisherDictionary = new ConcurrentDictionary<string, ConcurrentDictionary<ushort, int>>();
            m_Form                     = form;
        }

        public void Start()
        {
            m_CancellationTokenSource = new CancellationTokenSource();

            //KnownPublishersIndex.Clear();
            m_KnownPublisherDictionary.Clear();
            try
            {
                foreach ( Publisher publisher in m_PublisherQueue.GetConsumingEnumerable( m_CancellationTokenSource.Token ) )
                {
                    if ( m_CancellationTokenSource.Token.IsCancellationRequested )
                    {
                        Logger.Warn( "Cancellation requested for updating Publisher data. UpdatePublisher will stop listening now." );
                        return;
                    }
                    ParsePublisher( publisher );
                }
            }
            catch ( OperationCanceledException e )
            {
                Logger.Warn( "Cancellation via exception for updating Publisher data. UpdatePublisher will stop listening now.", e );
            }
        }

        public void Stop()
        {
            m_CancellationTokenSource?.Cancel();
        }

        private void Add2BindingSource( Publisher publisher )
        {
            if ( m_Form.InvokeRequired )
            {
                m_Form.Invoke( new Action<Publisher>( Add2BindingSource ), publisher );
            }
            else
            {
                try
                {
                    m_PublisherBindingSource.Add( publisher );
                    m_Form.AddNewPublisher( publisher );
                    m_Form.UpdateTimeFormatting();
                }
                catch ( Exception ex )
                {
                    if ( Logger.IsErrorEnabled )
                    {
                        Logger.Error( "Exception while adding binding source", ex );
                    }
                    if ( Debugger.IsAttached )
                    {
                        Debugger.Break();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private void ParsePublisher( Publisher publisher )
        {
            if ( publisher == null )
            {
                return;
            }
            ConcurrentDictionary<ushort, int> writerDictionary =
                    m_KnownPublisherDictionary.GetOrAdd( publisher.PublisherID, s => new ConcurrentDictionary<ushort, int>() );
            writerDictionary.AddOrUpdate( publisher.DataSetWriterID,
                                          arg =>
                                          {
                                              publisher.FirstTime       = DateTime.Now;
                                              publisher.LastTime        = publisher.FirstTime;
                                              publisher.NumberOfMessage = 1;
                                              Add2BindingSource( publisher );
                                              return m_LastPublisherIndex++;
                                          },
                                          ( writerID, index ) =>
                                          {
                                              publisher.LastTime = DateTime.Now;
                                              UpdateBindingSource( index, publisher );
                                              return index;
                                          }
                                        );
        }

        private void UpdateBindingSource( int index, Publisher publisher )
        {
            if ( m_Form.InvokeRequired )
            {
                m_Form.Invoke( new Action<int, Publisher>( UpdateBindingSource ), index, publisher );
            }
            else
            {
                Publisher old = (Publisher)m_PublisherBindingSource[index];
                if ( old.Major != publisher.Major )
                {
                    old.Major = publisher.Major;
                }
                if ( old.Minor != publisher.Minor )
                {
                    old.Minor = publisher.Minor;
                }
                old.LastTime  = publisher.LastTime;
                old.Timestamp = publisher.Timestamp;
                old.NumberOfMessage++;
                old.NumberOfMetaMessages      += publisher.NumberOfMetaMessages;
                old.NumberOfKeyMessages       += publisher.NumberOfKeyMessages;
                old.NumberOfDeltaMessages     += publisher.NumberOfDeltaMessages;
                old.NumberOfChunkedMessage    += publisher.NumberOfChunkedMessage;
                old.NumberOfKeepAliveMessages += publisher.NumberOfKeepAliveMessages;
            }
        }
    }
}