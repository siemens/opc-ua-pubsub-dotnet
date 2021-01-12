// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Reflection;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using ClosedXML.Excel;
using log4net;
using opc.ua.pubsub.dotnet.simulation.Excel.Model;

namespace opc.ua.pubsub.dotnet.simulation.Excel
{
    public class ReadKey : ReadBase
    {
        private static readonly ILog Logger = LogManager.GetLogger( MethodBase.GetCurrentMethod()
                                                                              .DeclaringType
                                                                  );
        public ReadKey( CommonConfig commonConfig, XLWorkbook workbook ) : base( commonConfig, workbook ) { }

        public List<KeyEntry> Read( string Name )
        {
            List<KeyEntry> keyEntries = new List<KeyEntry>();
            if ( CommonConfig == null || Workbook == null )
            {
                return keyEntries;
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
                KeyEntry entry = new KeyEntry();

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
                    Logger.Error( $"Unable to parse index '{indexCell.GetString()}' as int in row {row.RowNumber()}", e );
                    continue;
                }
                entry.Index = index;

                // Name
                IXLCell nameCell = row.Cell( "B" );
                if ( string.IsNullOrWhiteSpace( nameCell.GetString() ) )
                {
                    Logger.Error( $"Name is empty in row {row.RowNumber()}" );
                    continue;
                }
                entry.Name = nameCell.GetString();

                // Description
                IXLCell descriptionCell = row.Cell( "C" );
                if ( string.IsNullOrWhiteSpace( descriptionCell.GetString() ) )
                {
                    Logger.Info( $"Description is empty in row {row.RowNumber()}" );
                }
                entry.Description = descriptionCell.GetString();

                // DataType
                IXLCell dataTypeCell = row.Cell( "D" );
                if ( ReadConfiguration.TryGetNodeID( dataTypeCell.GetString(), out NodeID tempID, CommonConfig ) )
                {
                    entry.DataType = tempID;
                }
                else
                {
                    continue;
                }

                // Field ID
                IXLCell fieldIDCell = row.Cell( "E" );
                string  fieldID     = fieldIDCell.GetString();
                if ( !string.IsNullOrWhiteSpace( fieldID ) )
                {
                    if ( Guid.TryParse( fieldID, out Guid tempGUID ) )
                    {
                        entry.FieldID = tempGUID;
                    }
                    else
                    {
                        Logger.Warn( $"Unable to parse '{fieldID}' in row {row.RowNumber()} as GUID." );
                    }
                }
                if ( entry.FieldID == Guid.Empty )
                {
                    Logger.Info( "No GUID specified. Creating new GUID." );
                    entry.FieldID = Guid.NewGuid();
                }

                // Prefix
                IXLCell prefixCell = row.Cell( "F" );
                entry.Prefix = prefixCell.GetString();

                // Unit
                IXLCell unitCell = row.Cell( "G" );
                entry.Unit = unitCell.GetString();

                // Orcat
                IXLCell orcatCell = row.Cell( "H" );
                if ( ParseOrcat( row, orcatCell, out byte? orcatValue ) && orcatValue.HasValue )
                {
                    entry.Orcat = orcatValue.Value;
                }

                // Quality
                IXLCell qualityCell = row.Cell( "I" );
                if ( ParseQuality( row, qualityCell, out ushort? qualityValue ) && qualityValue.HasValue )
                {
                    entry.Quality = qualityValue.Value;
                }

                // Timestamp
                IXLCell timeStampCell = row.Cell( "J" );
                if ( ParseTimeStamp( row, timeStampCell, out long? timeStampValue ) && timeStampValue.HasValue )
                {
                    entry.TimeStamp = timeStampValue.Value;
                }

                // Value
                IXLCell valueCell  = row.Cell( "K" );
                IXLCell value2Cell = row.Cell( "L" );
                IEntry  baseEntry  = entry;
                if ( !TryParse( valueCell, value2Cell, ref baseEntry ) )
                {
                    continue;
                }
                keyEntries.Add( entry );
            }
            return keyEntries;
        }
    }
}