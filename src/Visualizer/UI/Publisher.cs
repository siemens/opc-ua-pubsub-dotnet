// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using opc.ua.pubsub.dotnet.visualizer.Properties;

namespace opc.ua.pubsub.dotnet.visualizer.UI
{
    public class Publisher : INotifyPropertyChanged, IEquatable<Publisher>
    {
        private static readonly Regex    PublisherIDRegEx = new Regex( "([^~]+)~([^~]+)~([^~]+)~([^~]+)" );
        private                 DateTime m_FirstTime;
        private                 DateTime m_LastTime;
        private                 uint     m_Major;
        private                 uint     m_Minor;
        private                 ulong    m_NumberOfChunkedMessage;
        private                 ulong    m_NumberOfDeltaMessages;
        private                 ulong    m_NumberOfKeepAliveMessages;
        private                 ulong    m_NumberOfKeyMessages;
        private                 ulong    m_NumberOfMessage;
        private                 ulong    m_NumberOfMetaMessages;
        private                 string   m_PublisherID;
        public                  ushort   DataSetWriterID { get; set; }
        public                  string   Family          { get; private set; }

        public DateTime FirstTime
        {
            get
            {
                return m_FirstTime;
            }
            set
            {
                if ( m_FirstTime != value )
                {
                    m_FirstTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime LastTime
        {
            get
            {
                return m_LastTime;
            }
            set
            {
                if ( m_LastTime != value )
                {
                    m_LastTime = value;
                    OnPropertyChanged();
                }
            }
        }

        [Browsable( false )]
        public uint Major
        {
            get
            {
                return m_Major;
            }
            set
            {
                m_Major = value;
                if ( MajorTime == null )
                {
                    MajorTime = new SingleConfigVersion();
                }
                MajorTime.Raw = value;
            }
        }

        [DisplayName( "Major" )]
        public SingleConfigVersion MajorTime { get; set; }

        [Browsable( false )]
        public uint Minor
        {
            get
            {
                return m_Minor;
            }
            set
            {
                m_Minor = value;
                if ( MinorTime == null )
                {
                    MinorTime = new SingleConfigVersion();
                }
                MinorTime.Raw = value;
            }
        }

        [DisplayName( "Minor" )]
        public SingleConfigVersion MinorTime { get; set; }
        public string Name { get;                   private set; }

        [DisplayName( "Chunks" )]
        public ulong NumberOfChunkedMessage
        {
            get
            {
                return m_NumberOfChunkedMessage;
            }
            set
            {
                if ( value == m_NumberOfChunkedMessage )
                {
                    return;
                }
                m_NumberOfChunkedMessage = value;
                OnPropertyChanged();
            }
        }

        [DisplayName( "Delta" )]
        public ulong NumberOfDeltaMessages
        {
            get
            {
                return m_NumberOfDeltaMessages;
            }
            set
            {
                if ( NumberOfDeltaMessages != value )
                {
                    m_NumberOfDeltaMessages = value;
                    OnPropertyChanged();
                }
            }
        }

        [DisplayName( "Keep Alive" )]
        public ulong NumberOfKeepAliveMessages
        {
            get
            {
                return m_NumberOfKeepAliveMessages;
            }
            set
            {
                if ( m_NumberOfKeepAliveMessages != value )
                {
                    m_NumberOfKeepAliveMessages = value;
                    OnPropertyChanged();
                }
            }
        }

        [DisplayName( "Key" )]
        public ulong NumberOfKeyMessages
        {
            get
            {
                return m_NumberOfKeyMessages;
            }
            set
            {
                if ( m_NumberOfKeyMessages != value )
                {
                    m_NumberOfKeyMessages = value;
                    OnPropertyChanged();
                }
            }
        }

        [DisplayName( "Messages" )]
        public ulong NumberOfMessage
        {
            get
            {
                return m_NumberOfMessage;
            }
            set
            {
                if ( m_NumberOfMessage != value )
                {
                    m_NumberOfMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        [DisplayName( "Meta" )]
        public ulong NumberOfMetaMessages
        {
            get
            {
                return m_NumberOfMetaMessages;
            }
            set
            {
                if ( m_NumberOfMetaMessages != value )
                {
                    m_NumberOfMetaMessages = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PublisherID
        {
            get
            {
                return m_PublisherID;
            }
            set
            {
                m_PublisherID = value;
                ParsePublisherID( m_PublisherID );
            }
        }

        public string   Serial    { get; private set; }
        public DateTime Timestamp { get; set; }
        public string   Type      { get; private set; }

        public bool Equals( Publisher other )
        {
            return other != null && this == other;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override bool Equals( object obj )
        {
            return Equals( obj as Publisher );
        }

        public override int GetHashCode()
        {
            int hashCode = 84185362;
            hashCode = ( hashCode * -1521134295 ) + EqualityComparer<string>.Default.GetHashCode( PublisherID );
            hashCode = ( hashCode * -1521134295 ) + DataSetWriterID.GetHashCode();
            hashCode = ( hashCode * -1521134295 ) + Major.GetHashCode();
            hashCode = ( hashCode * -1521134295 ) + Minor.GetHashCode();
            return hashCode;
        }

        public static bool operator ==( Publisher a, Publisher b )
        {
            if ( ReferenceEquals( null, a ) && ReferenceEquals( null, b ) )
            {
                return true;
            }
            if ( ReferenceEquals( null, a ) || ReferenceEquals( null, b ) )
            {
                return false;
            }
            if ( string.Compare( a.PublisherID, b.PublisherID, StringComparison.InvariantCulture ) != 0 )
            {
                return false;
            }
            if ( a.DataSetWriterID != b.DataSetWriterID )
            {
                return false;
            }
            if ( a.Major != b.Major )
            {
                return false;
            }
            if ( a.Minor != b.Minor )
            {
                return false;
            }
            return true;
        }

        public static bool operator !=( Publisher a, Publisher b )
        {
            return !( a == b );
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        private void ParsePublisherID( string publisherID )
        {
            Match match = PublisherIDRegEx.Match( publisherID );
            if ( !match.Success )
            {
                return;
            }
            Family = match.Groups[1]
                          .Value;
            Type = match.Groups[2]
                        .Value;
            Name = match.Groups[3]
                        .Value;
            Serial = match.Groups[4]
                          .Value;
        }
    }
}