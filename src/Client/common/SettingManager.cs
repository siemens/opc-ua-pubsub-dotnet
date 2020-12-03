// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.IO;
using Microsoft.Extensions.Configuration;

namespace opc.ua.pubsub.dotnet.client.common
{
    public static class SettingManager
    {
        private const  string         SettingsFileName = "settings.json";
        private static IConfiguration Configuration { get; set; }

        public static Settings.Settings ReadConfiguration( string[] args )
        {
            string currentDirectory       = Directory.GetCurrentDirectory();
            string settingsFileSameFolder = Path.Combine( currentDirectory, SettingsFileName );
            string libFolder              = Path.Combine( currentDirectory, "lib" );
            string settingsFileLibFolder  = Path.Combine( libFolder,        SettingsFileName );
            string settingsFile;
            string baseFolder;
            if ( File.Exists( settingsFileSameFolder ) )
            {
                baseFolder   = currentDirectory;
                settingsFile = settingsFileSameFolder;
            }
            else
            {
                baseFolder   = libFolder;
                settingsFile = settingsFileLibFolder;
            }
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                                                 .SetBasePath( baseFolder )
                                                 .AddJsonFile( settingsFile )
                                                 .AddCommandLine( args );
            Configuration = configBuilder.Build();
            return Configuration.GetSection( "Settings" )
                                .Get<Settings.Settings>();
        }

        public static bool TryGetCertificateAsArray( string certPathAndFileName, out byte[] certificateContent )
        {
            certificateContent = null;
            try
            {
                certificateContent = File.ReadAllBytes( certPathAndFileName );
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}