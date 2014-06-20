using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure;
using Thobb.Ijepai.ConfigManager;

namespace Thobb.Ijepai.Logging
{

    public static class Log
    {
        private const string EVENTSOURCE = "Ijepai";
        private const string EVENTLOG = "Application";
        public static void Write(EventKind eventKind, string message, params object[] args)
        {
            try
            {
                //Check Whether the Loggiing is Enabled in the Configurations
                int loggingLevel = 4;
                try
                {
                    string loggingLevelValue = RoleEnvironment.GetConfigurationSettingValue("LoggingLevel");
                    if (!string.IsNullOrEmpty(loggingLevelValue))
                        int.TryParse(loggingLevelValue, out loggingLevel);
                }
                catch
                {

                }

                switch (eventKind)
                {
                    case EventKind.Error:
                        if (loggingLevel >= 0)
                        {
                            Trace.TraceError(message, args);
                            LogInEventLog(EventLogEntryType.Error, message, args);
                        }
                        break;
                    case EventKind.Critical:
                        if (loggingLevel >= 1)
                        {
                            Trace.TraceError(message, args);
                            LogInEventLog(EventLogEntryType.Error, message, args);
                        }
                        break;
                    case EventKind.Warning:
                        if (loggingLevel >= 2)
                        {
                            Trace.TraceWarning(message, args);
                            LogInEventLog(EventLogEntryType.Warning, message, args);
                        }
                        break;
                    case EventKind.Information:
                        if (loggingLevel >= 3)
                        {
                            Trace.TraceInformation(message, args);
                            LogInEventLog(EventLogEntryType.Information, message, args);
                        }
                        break;
                    case EventKind.Verbose:
                        if (loggingLevel == 4)
                        {
                            Trace.TraceInformation(message, args);
                            LogInEventLog(EventLogEntryType.Information, message, args);
                        }
                        break;
                }
            }
            catch
            {
                LogInEventLog(EventLogEntryType.Error, "Failed to log message");
                LogInEventLog(EventLogEntryType.Error, message, args);
            }
        }

        private static void LogInEventLog(EventLogEntryType type, string message, params object[] paramsObjects)
        {
            try
            {
                message = string.Format(message, paramsObjects);
            }
            catch
            {

            }
            try
            {
                if (!EventLog.SourceExists(EVENTSOURCE))
                    EventLog.CreateEventSource(EVENTSOURCE, EVENTLOG);
                EventLog.WriteEntry(EVENTSOURCE, message, type);
            }
            catch
            {
                // Do nothing... this is a last resort
            }
        }

        public static string FormatExceptionInfo(Exception ex)
        {
            StackTrace stackTrace = new StackTrace();
            MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
            string result = string.Empty;
            //result = result + methodBase.Module + "\t";
            result = result + "Error Detected: " + methodBase.ReflectedType + "." + methodBase.Name + "()\t";
            result = result + "Error Msg: " + ex.Message + "\t";
            if (ex.InnerException != null)
            {
                result = result + "Inner Exception Msg: " + ex.InnerException.Message + "\t";
            }
            if (ex.StackTrace != string.Empty)
            {
                result = result + "Stack Trace: " + ex.StackTrace;
            }
            return result;
        }
    }
}

