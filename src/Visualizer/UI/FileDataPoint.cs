// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.ComponentModel;

namespace opc.ua.pubsub.dotnet.visualizer.UI
{
    public class FileDataPoint : DataPointBase
    {
        private string   m_FileType;
        private DateTime m_LastModified;
        private string   m_Path;
        private int      m_Size;
        [Browsable( false )]
        public byte[] Content { get; set; }

        public string FileType
        {
            get
            {
                return m_FileType;
            }
            set
            {
                if ( value == m_FileType )
                {
                    return;
                }
                m_FileType = value;
                OnPropertyChanged();
            }
        }

        public DateTime LastModified
        {
            get
            {
                return m_LastModified;
            }
            set
            {
                if ( value.Equals( m_LastModified ) )
                {
                    return;
                }
                m_LastModified = value;
                OnPropertyChanged();
            }
        }

        public string Path
        {
            get
            {
                return m_Path;
            }
            set
            {
                if ( value == m_Path )
                {
                    return;
                }
                m_Path = value;
                Updates++;
                OnPropertyChanged();
            }
        }

        public int Size
        {
            get
            {
                return m_Size;
            }
            set
            {
                if ( value == m_Size )
                {
                    return;
                }
                m_Size = value;
                OnPropertyChanged();
            }
        }
    }
}