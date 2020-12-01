// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.ComponentModel;
using System.Globalization;

namespace opc.ua.pubsub.dotnet.visualizer.UI
{
    public class SingleConfigVersion
    {
        public static readonly DateTime Base = new DateTime( 2000, 1, 1, 0, 0, 0, DateTimeKind.Utc );
        public                 uint     Raw { get; set; }

        public DateTime Time
        {
            get
            {
                return Base.AddSeconds( Raw );
            }
        }

        public override string ToString()
        {
            return $"{Time}[{Raw}]";
        }
    }

    public class SingleConfigVersionTypeConverter : TypeConverter
    {
        public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType )
        {
            SingleConfigVersion instance = value as SingleConfigVersion;
            if ( instance == null )
            {
                return null;
            }
            if ( destinationType == typeof(string) )
            {
                return instance.ToString();
            }
            return null;
        }
    }
}