// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using opc.ua.pubsub.dotnet.common;
using opc.ua.pubsub.dotnet.common.Settings;

namespace opc.ua.pubsub.dotnet.visualizer
{
    internal static class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger( MethodBase.GetCurrentMethod()
                                                                              .DeclaringType
                                                                  );
        private static Settings Settings { get; set; }

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main( string[] args )
        {
            Logger.Info( "Visualizer starting..." );
            Settings = SettingManager.ReadConfiguration( args );
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run( new VisualizerForm( Settings ) );
        }
    }
}