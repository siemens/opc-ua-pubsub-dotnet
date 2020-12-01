// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;

namespace opc.ua.pubsub.dotnet.visualizer.UI
{
    public class ProcessDataPoint : DataPointBase
    {
        private byte     m_Custom;
        private uint     m_MdspQuality;
        private byte     m_Orcat;
        private DateTime m_OrginalTimeStamp;
        private ushort   m_Quality;
        private DateTime m_TimeStamp;
        private object   m_Value;

        public byte Custom
        {
            get
            {
                return m_Custom;
            }
            set
            {
                if ( value == m_Custom )
                {
                    return;
                }
                m_Custom = value;
                OnPropertyChanged();
            }
        }

        public uint MDSPQuality
        {
            get
            {
                return m_MdspQuality;
            }
            set
            {
                if ( value == m_MdspQuality )
                {
                    return;
                }
                m_MdspQuality = value;
                OnPropertyChanged();
            }
        }

        public int NumberOfSubValues { get; set; }

        public byte Orcat
        {
            get
            {
                return m_Orcat;
            }
            set
            {
                if ( value == m_Orcat )
                {
                    return;
                }
                m_Orcat = value;
                OnPropertyChanged();
            }
        }

        public DateTime OriginalTimeStamp
        {
            get
            {
                return m_OrginalTimeStamp;
            }
            set
            {
                if ( value.Equals( m_OrginalTimeStamp ) )
                {
                    return;
                }
                m_OrginalTimeStamp = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     SI Prefix (e.g. k, M, G, ...)
        /// </summary>
        public string Prefix { get; set; }

        public ushort Quality
        {
            get
            {
                return m_Quality;
            }
            set
            {
                if ( value == m_Quality )
                {
                    return;
                }
                m_Quality = value;
                OnPropertyChanged();
            }
        }

        public DateTime TimeStamp
        {
            get
            {
                return m_TimeStamp;
            }
            set
            {
                if ( value.Equals( m_TimeStamp ) )
                {
                    return;
                }
                m_TimeStamp = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     SI Unit
        /// </summary>
        public string Unit { get; set; }

        public object Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                Updates++;
                if ( Equals( value, m_Value ) )
                {
                    return;
                }
                m_Value = value;
                OnPropertyChanged();
            }
        }
    }
}