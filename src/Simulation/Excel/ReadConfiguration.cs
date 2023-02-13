// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using opc.ua.pubsub.dotnet.binary;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using opc.ua.pubsub.dotnet.binary.Messages.Meta.Structure;
using ClosedXML.Excel;
using log4net;
using opc.ua.pubsub.dotnet.simulation.Excel.Model;
using String = opc.ua.pubsub.dotnet.binary.String;

namespace opc.ua.pubsub.dotnet.simulation.Excel
{
    public class ReadConfiguration
    {
        private static readonly ILog Logger = LogManager.GetLogger( typeof(ReadConfiguration) );

        public ReadConfiguration( string filePath )
        {
            FilePath = filePath;
        }

        public  CommonConfig CommonConfig { get; set; }
        public  string       FilePath     { get; }
        private XLWorkbook   Workbook     { get; set; }

        public ParsedData Read()
        {
            try
            {
                Workbook = new XLWorkbook( FilePath );
            }
            catch ( Exception exception )
            {
                Logger.Error( "Error while parsing input file.", exception );
                return null;
            }
            CommonConfig = new CommonConfig();
            ParseDataTypeRange();
            ParseEnumSheet();
            ReadCommon();
            ReadKey          readKey      = new ReadKey( CommonConfig, Workbook );
            List<KeyEntry>   keyEntries   = readKey.Read( "Key" );
            ReadDelta        readDelta    = new ReadDelta( CommonConfig, Workbook, keyEntries );
            List<DeltaEntry> deltaEntries = readDelta.Read( "Delta" );
            ParsedData       parsedData   = new ParsedData();
            parsedData.CommonConfig = CommonConfig;
            parsedData.KeyEntries   = keyEntries;
            parsedData.DeltaEntries = deltaEntries;
            return parsedData;
        }

        internal void ParseDataTypeRange()
        {
            IXLWorksheet dataTypeSheet = Workbook.Worksheets.Worksheet( "DataTypes" );
            IXLRows      rows          = dataTypeSheet.RowsUsed();
            bool         firstRow      = true;
            foreach ( IXLRow row in rows )
            {
                if ( firstRow )
                {
                    firstRow = false;
                    continue;
                }
                IXLCell typeNameCell  = row.Cell( "A" );
                IXLCell nameSpaceCell = row.Cell( "B" );
                IXLCell idCell        = row.Cell( "C" );
                string  key           = typeNameCell.GetString();
                if ( string.IsNullOrWhiteSpace( key ) )
                {
                    Logger.Error( $"Empty type name in row {row.RowNumber()}" );
                    continue;
                }
                byte nsByte;
                try
                {
                    nsByte = nameSpaceCell.GetValue<byte>();
                }
                catch ( FormatException exception )
                {
                    Logger.Error( $"Unable to parse Namespace of DataType {key} | {nameSpaceCell.GetString()} | {idCell.GetString()}", exception );
                    continue;
                }
                byte idByte;
                try
                {
                    idByte = idCell.GetValue<byte>();
                }
                catch ( FormatException exception )
                {
                    Logger.Error( $"Unable to parse ID of DataType {key} | {nameSpaceCell.GetString()} | {idCell.GetString()}", exception );
                    continue;
                }
                NodeID dataTypeID = new NodeID
                                    {
                                            Namespace = nsByte,
                                            Value     = idByte
                                    };
                if ( CommonConfig.KnownDataTypes.ContainsKey( key ) )
                {
                    Logger.Warn( $"DataType with name '{key}' already exists. Value will be overwritten from row {row.RowNumber()}" );
                }
                CommonConfig.KnownDataTypes[key] = dataTypeID;
            }
        }

        internal void ParseEnumSheet()
        {
            IXLWorksheet dataTypeSheet = Workbook.Worksheets.Worksheet( "Enums" );
            IXLRows      rows          = dataTypeSheet.RowsUsed();
            bool         firstRow      = true;
            foreach ( IXLRow row in rows )
            {
                if ( firstRow )
                {
                    firstRow = false;
                    continue;
                }
                IXLCell dataTypeCell      = row.Cell( "A" );
                IXLCell qualifiedNameCell = row.Cell( "B" );

                //IXLCell enumDescriptionDataTypeCell = row.Cell("C");
                IXLCell   valueCell       = row.Cell( "C" );
                IXLCell   displayNameCell = row.Cell( "D" );
                IXLCell   descriptionCell = row.Cell( "E" );
                IXLCell   valueNameCell   = row.Cell( "F" );
                EnumEntry entry           = new EnumEntry();
                if ( !TryGetNodeID( dataTypeCell.GetString(), out NodeID tempID, CommonConfig ) )
                {
                    continue;
                }
                entry.DataType      = tempID;
                entry.QualifiedName = qualifiedNameCell.GetString();
                int intValue;
                try
                {
                    intValue = valueCell.GetValue<int>();
                }
                catch ( FormatException exception )
                {
                    Logger.Error( $"Unable to parse integer value for enum '{valueCell.GetString()}' in row {row.RowNumber()}", exception );
                    continue;
                }
                entry.Value       = intValue;
                entry.DisplayName = displayNameCell.GetString();
                entry.Description = descriptionCell.GetString();
                entry.ValueName   = valueNameCell.GetString();
                EnumDescription enumDescription;
                if ( CommonConfig.EnumDescriptions.ContainsKey( entry.DataType ) )
                {
                    enumDescription = CommonConfig.EnumDescriptions[entry.DataType];
                }
                else
                {
                    enumDescription = new EnumDescription
                                      {
                                              Name       = new QualifiedName( entry.QualifiedName ),
                                              DataTypeID = entry.DataType,
                                              Fields     = new List<EnumField>()
                                      };
                    CommonConfig.EnumDescriptions.Add( entry.DataType, enumDescription );
                }
                EnumField enumField = new EnumField
                                      {
                                              Value       = entry.Value,
                                              Name        = new String( entry.ValueName ),
                                              Description = new LocalizedText(),
                                              DisplayName = new LocalizedText()
                                      };
                if ( !string.IsNullOrEmpty( entry.DisplayName ) )
                {
                    enumField.DisplayName.Locale = new String( "en-US" );
                    enumField.DisplayName.Text   = new String( entry.DisplayName );
                }
                if ( !string.IsNullOrEmpty( entry.Description ) )
                {
                    enumField.Description.Locale = new String( "en-US" );
                    enumField.Description.Text   = new String( entry.Description );
                }

                // TODO: Add check for duplicates
                enumDescription.Fields.Add( enumField );
            }
        }

        internal CommonConfig ReadCommon()
        {
            if ( Workbook == null )
            {
                return null;
            }
            IXLWorksheet commonSheet = Workbook.Worksheets.Worksheet( "Common" );
            if ( commonSheet == null )
            {
                Logger.Error( "Unable to find \"Common\" sheet." );
                return null;
            }
            IXLRows rows = commonSheet.RowsUsed();
            CommonConfig.MetaConfig.ConfigurationVersion = new ConfigurationVersion();
            DateTime now                  = DateTime.UtcNow;
            TimeSpan time                 = now - ConfigurationVersion.Base;
            uint     defaultConfigVersion = (uint)time.TotalSeconds;
            foreach ( IXLRow row in rows )
            {
                IXLCell cell      = row.Cell( "A" );
                IXLCell valueCell = row.Cell( "B" );
                Logger.Debug( $"Cell value: {cell.Value}" );
                switch ( cell.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) )
                {
                    case "Publisher ID":
                        CommonConfig.PublisherID = valueCell.GetString();
                        break;

                    case "DataSetWriterId":
                        ushort id;
                        try
                        {
                            id = valueCell.GetValue<ushort>();
                        }
                        catch ( FormatException exception )
                        {
                            Logger.Error( $"Unable to parse DataSetWriterID: '{valueCell.GetString()}'" );
                            Logger.Debug( "FormatException:", exception );
                            break;
                        }
                        CommonConfig.DataSetWriterID = id;
                        break;

                    case "MetaData - Name":
                        CommonConfig.MetaConfig.MetaDataName = valueCell.GetString();
                        break;

                    case "MetaData - Description":
                        CommonConfig.MetaConfig.MetaDataDescription = valueCell.GetString();
                        break;

                    case "ConfigurationVersion - Major":
                        uint major;
                        try
                        {
                            major = valueCell.GetValue<uint>();
                        }
                        catch ( FormatException exception )
                        {
                            Logger.Info( $"Unable to parse ConfigurationVersion - Major: '{valueCell.GetString()}', using default value {defaultConfigVersion}" );
                            major = defaultConfigVersion;
                            Logger.Debug( "FormatException:", exception );
                        }
                        CommonConfig.MetaConfig.ConfigurationVersion.Major = major;
                        break;

                    case "ConfigurationVersion - Minor":
                        uint minor;
                        try
                        {
                            minor = valueCell.GetValue<uint>();
                        }
                        catch ( FormatException exception )
                        {
                            Logger.Info( $"Unable to parse ConfigurationVersion - Minor: '{valueCell.GetString()}', using default value {defaultConfigVersion}" );
                            minor = defaultConfigVersion;
                            Logger.Debug( "FormatException:", exception );
                        }
                        CommonConfig.MetaConfig.ConfigurationVersion.Minor = minor;
                        break;
                }
            }
            return CommonConfig;
        }

        internal static bool TryGetNodeID( string dataType, out NodeID nodeID, CommonConfig commonConfig )
        {
            nodeID = null;
            if ( string.IsNullOrWhiteSpace( dataType ) )
            {
                Logger.Error( "DataType is empty." );
                return false;
            }
            if ( commonConfig.KnownDataTypes.TryGetValue( dataType, out NodeID tempID ) )
            {
                nodeID = tempID;
                return true;
            }
            Logger.Error( $"Unable to find NodeID for DataType name '{dataType}'." );
            return false;
        }
    }
}