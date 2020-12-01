// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.IO;

namespace Binary
{
    public class ConfigurationVersion : IEquatable<ConfigurationVersion>, IComparable<ConfigurationVersion>
    {
        public static DateTime Base  { get; } = new DateTime( 2000, 1, 1, 0, 0, 0, DateTimeKind.Utc );
        public        uint     Major { get; set; }

        public DateTime MajorTime
        {
            get
            {
                return Base.AddSeconds( Major );
            }
        }

        public uint Minor { get; set; }

        public DateTime MinorTime
        {
            get
            {
                return Base.AddSeconds( Minor );
            }
        }

        public static ConfigurationVersion Decode( Stream inputStream )
        {
            ConfigurationVersion instance = new ConfigurationVersion();
            if ( inputStream == null || !inputStream.CanRead )
            {
                return instance;
            }
            uint? major = BaseType.ReadUInt32( inputStream );
            if ( major != null )
            {
                instance.Major = major.Value;
            }
            uint? minor = BaseType.ReadUInt32( inputStream );
            if ( minor != null )
            {
                instance.Minor = minor.Value;
            }
            return instance;
        }

        public void Encode( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( Major ) );
            BaseType.WriteToStream( outputStream, BitConverter.GetBytes( Minor ) );
        }

        /// <summary>
        ///     Returns the Major and Minor part of the
        ///     Configuration as a string so that it can be
        ///     used as a RowKey in Azure Table Storage.
        /// </summary>
        public string GetAsRowKey()
        {
            return $"{Major}-{Minor}";
        }

        /// <summary>
        ///     Returns a human readable version of Major and Minor.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{MajorTime}[{Major}]-{MinorTime}[{Minor}]";
        }

        #region Overrides of Object

        public override bool Equals( object obj )
        {
            if ( ReferenceEquals( null, obj ) )
            {
                return false;
            }
            if ( ReferenceEquals( this, obj ) )
            {
                return true;
            }
            if ( obj.GetType() != GetType() )
            {
                return false;
            }
            return Equals( (ConfigurationVersion)obj );
        }

        #region Equality members

        public bool Equals( ConfigurationVersion other )
        {
            if ( ReferenceEquals( null, other ) )
            {
                return false;
            }
            if ( ReferenceEquals( this, other ) )
            {
                return true;
            }
            return Major == other.Major && Minor == other.Minor;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ( (int)Major * 397 ) ^ (int)Minor;
            }
        }

        public int CompareTo( ConfigurationVersion other )
        {
            long majorDif = Major - other.Major;
            if ( majorDif != 0 )
            {
                return majorDif > 0 ? 1 : -1;
            }
            long minorDif = Minor - other.Minor;
            if ( minorDif != 0 )
            {
                return minorDif > 0 ? 1 : -1;
            }
            return 0;
        }

        public static bool operator ==( ConfigurationVersion left, ConfigurationVersion right )
        {
            return Equals( left, right );
        }

        public static bool operator !=( ConfigurationVersion left, ConfigurationVersion right )
        {
            return !Equals( left, right );
        }

        public static bool operator <( ConfigurationVersion left, ConfigurationVersion right )
        {
            if ( left.Major < right.Major )
            {
                return true;
            }
            if ( left.Major == right.Major && left.Minor < right.Minor )
            {
                return true;
            }
            return false;
        }

        public static bool operator >( ConfigurationVersion left, ConfigurationVersion right )
        {
            if ( left.Major > right.Major )
            {
                return true;
            }
            if ( left.Major == right.Major && left.Minor > right.Minor )
            {
                return true;
            }
            return false;
        }

        #endregion

        #endregion
    }
}