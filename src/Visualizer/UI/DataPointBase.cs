// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.ComponentModel;
using System.Runtime.CompilerServices;
using opc.ua.pubsub.dotnet.visualizer.Properties;

namespace opc.ua.pubsub.dotnet.visualizer.UI
{
    public abstract class DataPointBase : INotifyPropertyChanged
    {
        private int    m_Index;
        private string m_Name;
        private int    m_Updates;

        public int Index
        {
            get
            {
                return m_Index;
            }
            set
            {
                if ( value == m_Index )
                {
                    return;
                }
                m_Index = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                if ( value == m_Name )
                {
                    return;
                }
                m_Name = value;
                OnPropertyChanged();
            }
        }

        public int Updates
        {
            get
            {
                return m_Updates;
            }
            protected set
            {
                if ( value == m_Updates )
                {
                    return;
                }
                m_Updates = value;
                OnPropertyChanged();
            }
        }

        public virtual event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}