// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using Binary.DataPoints;
using log4net;
using opc.ua.pubsub.dotnet.simulation.Excel.Model;
using static opc.ua.pubsub.dotnet.client.ProcessDataSet;

namespace opc.ua.pubsub.dotnet.simulation.Excel
{
    public class CreateKeyMessage
    {
        private static readonly ILog Logger = LogManager.GetLogger( typeof(CreateKeyMessage) );

        internal static DataPointValue GetDataPointValue( DeltaEntry keyEntry, DataSetType dataSetType )
        {
            ProcessDataPointValue dpv = ProcessValueFactory.CreateValue( keyEntry.DataType );
            if ( dpv == null )
            {
                Logger.Error( $"Unable to create DataPoint for entry: {keyEntry}" );
                return null;
            }
            dpv.Orcat     = keyEntry.Orcat;
            dpv.Quality   = keyEntry.Quality;
            dpv.Timestamp = keyEntry.TimeStamp;
            dpv.Value     = keyEntry.Value;
            if ( keyEntry is KeyEntry key )
            {
                dpv.Prefix  = key.Prefix;
                dpv.Unit    = key.Unit;
                dpv.FieldID = key.FieldID;
            }
            if ( dpv is CounterValue )
            {
                dpv.SetAttributeValue( CounterValue.QuantityAttributeName, keyEntry.Value2 );
            }
            if ( dpv is StepPosValue || dpv is StepPosEvent )
            {
                dpv.SetAttributeValue( StepPosValue.TransientAttributeName, keyEntry.Value2 );
            }
            if ( dpv is ComplexMeasuredValue )
            {
                dpv.SetAttributeValue( ComplexMeasuredValue.AngleAttributeName, keyEntry.Value2 );
            }
            if ( dpv.Timestamp == 0 )
            {
                dpv.Timestamp = DateTime.Now.ToFileTimeUtc();
            }
            return dpv;
        }
    }
}