// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Globalization;
using System.Reflection;
using opc.ua.pubsub.dotnet.binary.DataPoints;
using ClosedXML.Excel;
using log4net;
using opc.ua.pubsub.dotnet.simulation.Excel.Model;

namespace opc.ua.pubsub.dotnet.simulation.Excel
{
    public abstract class ReadBase
    {
        private static readonly ILog Logger = LogManager.GetLogger( MethodBase.GetCurrentMethod()
                                                                              .DeclaringType
                                                                  );

        public ReadBase( CommonConfig commonConfig, XLWorkbook workbook )
        {
            CommonConfig = commonConfig;
            Workbook     = workbook;
        }

        public CommonConfig CommonConfig { get; }
        public XLWorkbook   Workbook     { get; }

        protected bool ParseOrcat( IXLRow row, IXLCell orcatCell, out byte? value )
        {
            value = null;
            if ( !string.IsNullOrWhiteSpace( orcatCell.GetString() ) )
            {
                try
                {
                    value = orcatCell.GetValue<byte>();
                }
                catch ( FormatException exception )
                {
                    Logger.Error( $"Unable to parse '{orcatCell.GetString()}' in row {row.RowNumber()} as byte", exception );
                    return false;
                }
            }
            return true;
        }

        protected bool ParseQuality( IXLRow row, IXLCell qualityCell, out ushort? value )
        {
            value = null;
            if ( !string.IsNullOrWhiteSpace( qualityCell.GetString() ) )
            {
                try
                {
                    value = qualityCell.GetValue<ushort>();
                }
                catch ( FormatException exception )
                {
                    Logger.Error( $"Unable to parse '{qualityCell.GetString()}' in row {row.RowNumber()} as UInt16", exception );
                    return false;
                }
            }
            return true;
        }

        protected bool ParseTimeStamp( IXLRow row, IXLCell timeStampCell, out long? value )
        {
            value = null;
            if ( !string.IsNullOrWhiteSpace( timeStampCell.GetString() ) )
            {
                try
                {
                    value = long.Parse( timeStampCell.RichText.Text, CultureInfo.InvariantCulture );
                }
                catch ( FormatException exception )
                {
                    Logger.Error( $"Unable to parse '{timeStampCell.GetString()}' in row {row.RowNumber()} as Int64", exception );
                    return false;
                }
                catch ( ArgumentException exception )
                {
                    Logger.Error( $"Unable to parse '{timeStampCell.GetString()}' in row {row.RowNumber()} as Int64", exception );
                    return false;
                }
                catch ( OverflowException exception )
                {
                    Logger.Error( $"Unable to parse '{timeStampCell.GetString()}' in row {row.RowNumber()} as Int64", exception );
                    return false;
                }
            }
            return true;
        }

        internal bool TryParse( IXLCell cell, IXLCell cell2, ref IEntry entry )
        {
            string stringValue  = cell.GetString();
            string stringValue2 = cell2.GetString();
            if ( string.IsNullOrWhiteSpace( stringValue ) )
            {
                return false;
            }
            if ( entry.DataType == SPSValue.PreDefinedNodeID || entry.DataType == SPSEvent.PreDefinedNodeID )
            {
                bool value;
                try
                {
                    value = cell.GetValue<bool>();
                }
                catch ( FormatException exception )
                {
                    Logger.Error( $"Unable to parse '{stringValue}' in row {cell.WorksheetRow().RowNumber()} as bool.", exception );
                    return false;
                }
                entry.Value = value;
                return true;
            }
            if ( entry.DataType == DPSValue.PreDefinedNodeID || entry.DataType == DPSEvent.PreDefinedNodeID )
            {
                byte value;
                try
                {
                    value = cell.GetValue<byte>();
                }
                catch ( FormatException exception )
                {
                    Logger.Error( $"Unable to parse '{stringValue}' in row {cell.WorksheetRow().RowNumber()} as byte.", exception );
                    return false;
                }
                entry.Value = value;
                return true;
            }
            if ( entry.DataType == MeasuredValue.PreDefinedNodeID || entry.DataType == MeasuredValueEvent.PreDefinedNodeID )
            {
                float value;
                try
                {
                    value = cell.GetValue<float>();
                }
                catch ( FormatException exception )
                {
                    Logger.Error( $"Unable to parse '{stringValue}' in row {cell.WorksheetRow().RowNumber()} as float.", exception );
                    return false;
                }
                entry.Value = value;
                return true;
            }
            if ( entry.DataType == StepPosValue.PreDefinedNodeID || entry.DataType == StepPosEvent.PreDefinedNodeID )
            {
                int  value;
                bool value2;
                try
                {
                    value  = cell.GetValue<int>();
                    value2 = cell2.GetValue<bool>();
                }
                catch ( FormatException exception )
                {
                    Logger.Error( $"Unable to parse '{stringValue}' and '{stringValue2}' in row {cell.WorksheetRow().RowNumber()} as float and bool.", exception );
                    return false;
                }
                entry.Value  = value;
                entry.Value2 = value2;
                return true;
            }
            if ( entry.DataType == CounterValue.PreDefinedNodeID )
            {
                long  value;
                float value2;
                try
                {
                    value  = cell.GetValue<int>();
                    value2 = cell2.GetValue<float>();
                }
                catch ( FormatException exception )
                {
                    Logger.Error( $"Unable to parse '{stringValue}' and '{stringValue2}' in row {cell.WorksheetRow().RowNumber()} as int and float.", exception );
                    return false;
                }
                entry.Value  = value;
                entry.Value2 = value2;
                return true;
            }
            if ( entry.DataType == ComplexMeasuredValue.PreDefinedNodeID )
            {
                float value;
                float value2;
                try
                {
                    value  = cell.GetValue<float>();
                    value2 = cell2.GetValue<float>();
                }
                catch ( FormatException exception )
                {
                    Logger.Error( $"Unable to parse '{stringValue}' and '{stringValue2}' in row {cell.WorksheetRow().RowNumber()} as float and float.", exception );
                    return false;
                }
                entry.Value  = value;
                entry.Value2 = value2;
                return true;
            }
            if ( entry.DataType == StringEvent.PreDefinedNodeID )
            {
                string value;
                try
                {
                    value = cell.GetValue<string>();
                }
                catch ( FormatException exception )
                {
                    Logger.Error( $"Unable to parse '{stringValue}' in row {cell.WorksheetRow().RowNumber()} as string.", exception );
                    return false;
                }
                entry.Value = value;
                return true;
            }
            if ( ProcessValueFactory.GetNodeIDType( entry.DataType ) == NodeIDType.GroupDataTypeTimeSeries )
            {
                float value;
                try
                {
                    value = cell.GetValue<float>();
                }
                catch ( FormatException exception )
                {
                    Logger.Error( $"Unable to parse '{stringValue}' in row {cell.WorksheetRow().RowNumber()} as string.", exception );
                    return false;
                }
                entry.Value = value;
                return true;
            }

            // For everything else we assume it's an Integer or an Enum
            int enumValue;
            try
            {
                enumValue = cell.GetValue<int>();
            }
            catch ( FormatException exception )
            {
                Logger.Error( $"Unable to parse '{stringValue}' in row {cell.WorksheetRow().RowNumber()} as int.", exception );
                return false;
            }
            entry.Value = enumValue;
            return true;
        }
    }
}