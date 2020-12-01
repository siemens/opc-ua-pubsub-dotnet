// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ClosedXML.Excel;
using log4net;
using opc.ua.pubsub.dotnet.simulation.Excel.Model;

namespace opc.ua.pubsub.dotnet.simulation.Excel
{
    public class ReadDelta : ReadBase
    {
        private static readonly ILog Logger = LogManager.GetLogger( MethodBase.GetCurrentMethod()
                                                                              .DeclaringType
                                                                  );

        public ReadDelta( CommonConfig commonConfig, XLWorkbook workbook, List<KeyEntry> keyEntries ) : base( commonConfig, workbook )
        {
            KeyEntries = keyEntries;
        }

        public List<KeyEntry> KeyEntries { get; }

        public List<DeltaEntry> Read( string Name, bool isGrouped = false )
        {
            List<DeltaEntry> deltaEntries = new List<DeltaEntry>();
            if ( CommonConfig == null || Workbook == null )
            {
                return deltaEntries;
            }
            IXLWorksheet keySheet = Workbook.Worksheets.Worksheet( Name );
            IXLRows      rows     = keySheet.RowsUsed();
            bool         firstRow = true;
            foreach ( IXLRow row in rows )
            {
                if ( firstRow )
                {
                    firstRow = false;
                    continue;
                }
                DeltaEntry entry = new DeltaEntry();

                // Index
                IXLCell indexCell = row.Cell( "A" );
                if ( string.IsNullOrWhiteSpace( indexCell.GetString() ) )
                {
                    Logger.Error( $"Empty index cell in row {row.RowNumber()}" );
                    continue;
                }
                ushort index = ushort.MinValue;
                try
                {
                    index = indexCell.GetValue<ushort>();
                }
                catch ( FormatException e )
                {
                    Logger.Error( $"Unable to parse index '{indexCell.GetString()}' as ushort in row {row.RowNumber()}", e );
                    continue;
                }
                entry.Index = index;

                //Grouped Delta Entry
                if ( isGrouped )
                {
                    entry.DataType = KeyEntries.First()
                                               .DataType;
                }
                else
                {
                    // Map DataType from KeyList
                    if ( KeyEntries == null || KeyEntries.Count == 0 )
                    {
                        Logger.Error( "Cannot parse value of DeltaEntry, because KeyEntries are not available." );
                        continue;
                    }
                    if ( KeyEntries.Count < entry.Index )
                    {
                        Logger.Error( $"Cannot parse value of DeltaEntry, because index [{entry.Index}] of Delta Entry is not available in KeyEntries List (count is {KeyEntries.Count})."
                                    );
                        continue;
                    }
                    KeyEntry keyEntry = KeyEntries[entry.Index];
                    entry.DataType = keyEntry.DataType;
                }

                // Orcat
                IXLCell orcatCell = row.Cell( "B" );
                if ( ParseOrcat( row, orcatCell, out byte? orcatValue ) && orcatValue.HasValue )
                {
                    entry.Orcat = orcatValue.Value;
                }

                // Quality
                IXLCell qualityCell = row.Cell( "C" );
                if ( ParseQuality( row, qualityCell, out ushort? qualityValue ) && qualityValue.HasValue )
                {
                    entry.Quality = qualityValue.Value;
                }

                // Timestamp
                IXLCell timeStampCell = row.Cell( "D" );
                if ( ParseTimeStamp( row, timeStampCell, out long? timeStampValue ) && timeStampValue.HasValue )
                {
                    entry.TimeStamp = timeStampValue.Value;
                }

                // Value
                IXLCell valueCell  = row.Cell( "E" );
                IXLCell value2Cell = row.Cell( "F" );
                IEntry  baseEntry  = entry;
                if ( !TryParse( valueCell, value2Cell, ref baseEntry ) )
                {
                    continue;
                }

                //Name
                if ( isGrouped )
                {
                    IXLCell cellName = row.Cell( "F" );
                    if ( !string.IsNullOrWhiteSpace( cellName.GetString() ) )
                    {
                        try
                        {
                            entry.Name = cellName.GetValue<string>();
                        }
                        catch ( FormatException exception )
                        {
                            Logger.Error( $"Unable to parse '{qualityCell.GetString()}' in row {row.RowNumber()} as UInt16", exception );
                        }
                    }
                }
                deltaEntries.Add( entry );
            }
            return deltaEntries;
        }
    }
}