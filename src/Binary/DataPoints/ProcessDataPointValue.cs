// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Binary.Messages.Meta;
using Binary.Messages.Meta.Structure;
using log4net;

namespace Binary.DataPoints
{
    public abstract class ProcessDataPointValue : DataPointValue, IEquatable<ProcessDataPointValue>
    {
        private static readonly ILog Logger = LogManager.GetLogger( MethodBase.GetCurrentMethod()
                                                                              .DeclaringType
                                                                  );
        protected Dictionary<string, object> m_AttributeValues = new Dictionary<string, object>();
        protected string                     m_QualityPropertyName;
        protected string                     m_TimeStampPropertyName;

        public ProcessDataPointValue( string qualityPropertyName, string timeStampPropertyName )
        {
            ArraySize               = 0;
            m_QualityPropertyName   = qualityPropertyName;
            m_TimeStampPropertyName = timeStampPropertyName;
            FieldID                 = new Guid();
            Properties              = new List<KeyValuePair>();
            Prefix                  = "";
            Unit                    = "";
            ArraySize               = 0;
            MDSPQuality             = 0;
            Quality                 = 0xFF;
            Orcat                   = 0;
            Timestamp               = 0;
        }

        public int ArraySize { get; protected set; }

        public byte Custom
        {
            get
            {
                byte[] mdspQualityBytes = BitConverter.GetBytes( MDSPQuality );
                byte   custom           = mdspQualityBytes[3];
                return custom;
            }
            private set
            {
                const uint mask     = 0x00FFFFFF;
                uint       intValue = value;
                MDSPQuality = ( MDSPQuality & mask ) + ( intValue << 24 );
            }
        }

        public uint MDSPQuality
        {
            get
            {
                object val = GetAttributeValue( m_QualityPropertyName );
                return (uint?)val ?? 0;
            }
            set
            {
                SetAttributeValue( m_QualityPropertyName, value );
            }
        }

        public virtual NodeIDType NodeIDType { get; protected set; } = NodeIDType.None;

        public byte Orcat
        {
            get
            {
                byte[] mdspQualityBytes = BitConverter.GetBytes( MDSPQuality );
                byte   orcat            = mdspQualityBytes[2];
                return orcat;
            }
            set
            {
                const uint mask     = 0xFF00FFFF;
                uint       intValue = value;
                MDSPQuality = ( MDSPQuality & mask ) + ( intValue << 16 );
            }
        }

        public string Prefix
        {
            get
            {
                // find a key/value pair in Properties list with key equal to "Prefix" 
                foreach ( KeyValuePair keyValuePair in Properties )
                {
                    if ( keyValuePair.Name.Name.Value.Equals( "Prefix", StringComparison.InvariantCulture ) )
                    {
                        // the Value in KeyValuePair is of type object but here we 
                        // must have an UADP.String stored
                        String uadpString = keyValuePair.Value as String;
                        if ( uadpString == null )
                        {
                            // null or not an UADP.String object
                            return null;
                        }

                        // return the string in the UADP.String
                        return uadpString.Value;
                    }
                }

                // key/value pair not found
                return null;
            }
            set
            {
                int index = Properties.FindIndex( x => x.Name.Name.Value == "Prefix" );
                if ( index == -1 )
                {
                    // create a new "Prefix" key/value pair if still not exists 
                    // and add it to the Properties list;
                    // note that the Value must have the type UADP.String
                    KeyValuePair prefix = new KeyValuePair( "Prefix", new String( value ) );
                    Properties.Add( prefix );
                }
                else
                {
                    // "Prefix" key/value pair already exists; then update;
                    // note that the Value must have the type UADP.String
                    Properties[index]
                           .Value = new String( value );
                }
            }
        }

        public ushort Quality
        {
            get
            {
                byte[] mdspQualityBytes = BitConverter.GetBytes( MDSPQuality );
                ushort q                = BitConverter.ToUInt16( mdspQualityBytes, 0 );
                return q;
            }
            set
            {
                uint mask = 0xFFFF0000;
                MDSPQuality = ( MDSPQuality & mask ) + value;
                BitArray bitArray = new BitArray( BitConverter.GetBytes( MDSPQuality ) );
                bool     bit0     = bitArray.Get( 15 );
                bool     bit1     = bitArray.Get( 14 );
                if ( bit0 ) // 1
                {
                    if ( bit1 ) // 11
                    {
                        Custom = 0x40;
                    }
                    else // 10
                    {
                        Custom = 0x7F;
                    }
                }
                else // 0
                {
                    if ( bit1 ) // 0 1
                    {
                        Custom = 0xFF;
                    }
                    else // 0 0
                    {
                        Custom = 0;
                    }
                }
            }
        }

        public long Timestamp
        {
            get
            {
                object val = GetAttributeValue( m_TimeStampPropertyName );
                return val == null ? 0 : Convert.ToInt64( val, CultureInfo.InvariantCulture );
            }
            set
            {
                try
                {
                    DateTime.FromFileTimeUtc( value );
                    SetAttributeValue( m_TimeStampPropertyName, value );
                }
                catch
                {
                    Logger.Error( $"Erroneous filestamp received {value} - Using Epoch Time" );
                    SetDateTime( new DateTime( 1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc ) );
                }
            }
        }

        public string Unit
        {
            get
            {
                // find a key/value pair in Properties list with key equal to "Unit" 
                foreach ( KeyValuePair keyValuePair in Properties )
                {
                    if ( keyValuePair.Name.Name.Value.Equals( "Unit", StringComparison.InvariantCulture ) )
                    {
                        // the Value in KeyValuePair is of type object but here we 
                        // must have an UADP.String stored
                        String uadpString = keyValuePair.Value as String;
                        if ( uadpString == null )
                        {
                            // null or not an UADP.String object
                            return null;
                        }

                        // return the string in the UADP.String
                        return uadpString.Value;
                    }
                }

                // key/value pair not found
                return null;
            }
            set
            {
                int index = Properties.FindIndex( x => x.Name.Name.Value == "Unit" );
                if ( index == -1 )
                {
                    // create a new "Unit" key/value pair if still not exists 
                    // and add it to the Properties list;
                    // note that the Value must have the type UADP.String
                    KeyValuePair unit = new KeyValuePair( "Unit", new String( value ) );
                    Properties.Add( unit );
                }
                else
                {
                    // "Unit" key/value pair already exists; then update;
                    // note that the Value must have the type UADP.String
                    Properties[index]
                           .Value = new String( value );
                }
            }
        }

        // Just some place to store user specific data at this object...
        public object UserData { get; set; }

        public object Value
        {
            get
            {
                if ( ArraySize == 0 )
                {
                    return GetAttributeValue( "Value" );
                }
                object[] retVal = new object[ArraySize];
                for ( int i = 0; i < ArraySize; i++ )
                {
                    retVal[i] = GetAttributeValue( GetArrayElementName( i ) );
                }
                return retVal;
            }
            set
            {
                if ( ArraySize == 0 )
                {
                    SetAttributeValue( "Value", value );
                }
                else
                {
                    if ( value is Array arrayValue && arrayValue.Length <= ArraySize )
                    {
                        for ( int i = 0; i < arrayValue.Length; i++ )
                        {
                            SetAttributeValue( GetArrayElementName( i ), arrayValue.GetValue( i ) );
                        }
                    }
                }
            }
        }

        public bool Equals( ProcessDataPointValue other )
        {
            if ( ReferenceEquals( null, other ) )
            {
                return false;
            }
            if ( ReferenceEquals( this, other ) )
            {
                return true;
            }
            bool areEqual = base.Equals( other )
                         && m_AttributeValues.ContentEquals( other.m_AttributeValues )
                         && m_TimeStampPropertyName == other.m_TimeStampPropertyName
                         && m_QualityPropertyName   == other.m_QualityPropertyName
                         && ArraySize               == other.ArraySize
                         && NodeIDType              == other.NodeIDType
                         && Equals( UserData, other.UserData );
            return areEqual;
        }

        public override void Decode( Stream inputStream )
        {
            if ( inputStream == null || StructureDescription == null )
            {
                Logger.Info( "Either inputStream is null or StructureDescription is not set" );
                return;
            }
            foreach ( StructureField field in StructureDescription.Fields )
            {
                DataPointEncoderDecoder.Decode( inputStream, m_AttributeValues, field.DataType, field.Name.Value );
            }
        }

        public override void Encode( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            foreach ( StructureField field in StructureDescription.Fields )
            {
                if ( m_AttributeValues.TryGetValue( field.Name.Value, out object val ) )
                {
                    DataPointEncoderDecoder.Encode( outputStream, field.DataType, val );
                }
                else
                {
                    throw new Exception( $"Mandatory field missing: \"{field.Name}\" ({field.DataType})" );
                }
            }
        }

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
            return Equals( (ProcessDataPointValue)obj );
        }

        public virtual string GetArrayElementName( int index )
        {
            return null;
        }

        /// <summary>
        ///     Get attribute value other than "Value"
        /// </summary>
        /// <param name="attributeName">CounterQuantityName or StepPosTransientName</param>
        /// <returns></returns>
        public object GetAttributeValue( string attributeName )
        {
            m_AttributeValues.TryGetValue( attributeName, out object val );
            return val;
        }

        public DateTime GetDateTime()
        {
            DateTime refDate = DateTime.FromFileTimeUtc( Timestamp );
            return refDate;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = ( hashCode * 397 ) ^ ( m_AttributeValues       != null ? m_AttributeValues.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ ( m_TimeStampPropertyName != null ? m_TimeStampPropertyName.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ ( m_QualityPropertyName   != null ? m_QualityPropertyName.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ ArraySize;
                hashCode = ( hashCode * 397 ) ^ (int)NodeIDType;
                hashCode = ( hashCode * 397 ) ^ ( UserData != null ? UserData.GetHashCode() : 0 );
                return hashCode;
            }
        }

        public uint GetSize()
        {
            return 0;
        }

        public static bool operator ==( ProcessDataPointValue left, ProcessDataPointValue right )
        {
            return Equals( left, right );
        }

        public static bool operator !=( ProcessDataPointValue left, ProcessDataPointValue right )
        {
            return !Equals( left, right );
        }

        /// <summary>
        ///     Set attributes other than "Value"
        ///     eg: ValueQuantity or Transient
        /// </summary>
        /// <param name="attributeName">CounterQuantityName or StepPosTransientName </param>
        /// <param name="value">value of the attribute</param>
        public void SetAttributeValue( string attributeName, object value )
        {
            m_AttributeValues[attributeName] = value;
        }

        public void SetDateTime( DateTime dateTime )
        {
            Timestamp = dateTime.ToFileTimeUtc();
        }

        #region Overrides of Object

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append( $"{Orcat,10} | {Quality,10} | {Custom,10} | {MDSPQuality,10} | {GetDateTime(),20:s} " );
            sb.Append( " |" );
            foreach ( KeyValuePair<string, object> value in m_AttributeValues )
            {
                sb.Append( $"{value.Value,10}" );
                sb.Append( ',' );
                sb.Append( " |" );
            }
            return sb.ToString();
        }

        #endregion

        public virtual bool Update( ProcessDataPointValue newValue )
        {
            bool valueChanged = false;
            if ( Quality != newValue.Quality )
            {
                Quality      = newValue.Quality;
                valueChanged = true;
            }
            if ( Orcat != newValue.Orcat )
            {
                Orcat        = newValue.Orcat;
                valueChanged = true;
            }
            if ( Timestamp != newValue.Timestamp )
            {
                Timestamp    = newValue.Timestamp;
                valueChanged = true;
            }
            return valueChanged;
        }
    }
}