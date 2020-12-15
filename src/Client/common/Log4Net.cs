using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace opc.ua.pubsub.dotnet.client.common
{
    public static class Log4Net
    {
        public static void Init()
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if ( entryAssembly == null )
                return;
            string logName = Path.GetFileNameWithoutExtension( entryAssembly.Location ) + ".log";
            GlobalContext.Properties["LogName"] = logName;
            ILoggerRepository logRepository = LogManager.GetRepository( entryAssembly );
            XmlConfigurator.Configure( logRepository, new FileInfo( "log4net.config" ) );

            
        }
    }
}
