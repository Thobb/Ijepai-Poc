using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.Diagnostics.Management;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;

namespace Processor
{
    public class WorkerRole : RoleEntryPoint
    {
                
        /// <summary>ServiceHost object for internal and external endpoints.</summary>
        public override void Run()
        {
            try 
            {

            }
            catch (Exception e)
            {
                Trace.TraceError("ProcessorRole Error Launching Rule Server.  Error: " + e.Message +
                    "\r\n View Stack: " + e.StackTrace + (e.InnerException == null ? "" : "\r\n Inner Exception: " + e.InnerException.Message));
            }

            
        }

        public override bool OnStart()
        {
            string wadConnectionString = "Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString";

            RoleInstanceDiagnosticManager roleInstanceDiagnosticManager =
                           CloudAccountDiagnosticMonitorExtensions.CreateRoleInstanceDiagnosticManager(RoleEnvironment.GetConfigurationSettingValue(wadConnectionString),
                               RoleEnvironment.DeploymentId, RoleEnvironment.CurrentRoleInstance.Role.Name,
                               RoleEnvironment.CurrentRoleInstance.Id);


            DiagnosticMonitorConfiguration config = roleInstanceDiagnosticManager.GetCurrentConfiguration();
            // should never be null, but this is a fallback in case it is. I recommend removing this in a production
            // deployment
            if (null == config)
            {
                config = DiagnosticMonitor.GetDefaultInitialConfiguration();
            }

            config.Logs.ScheduledTransferPeriod = TimeSpan.FromMinutes(15);
            config.Logs.ScheduledTransferLogLevelFilter =  Microsoft.WindowsAzure.Diagnostics.LogLevel.Undefined;
            config.DiagnosticInfrastructureLogs.ScheduledTransferLogLevelFilter = Microsoft.WindowsAzure.Diagnostics.LogLevel.Warning;
            config.DiagnosticInfrastructureLogs.ScheduledTransferPeriod = TimeSpan.FromMinutes(15);
            config.Directories.ScheduledTransferPeriod = TimeSpan.FromMinutes(15);

            roleInstanceDiagnosticManager.SetCurrentConfiguration(config);
            System.Diagnostics.Trace.TraceInformation("Diagnostics Running");

            #region Setup CloudStorageAccount Configuration Setting Publisher

            // This code sets up a handler to update CloudStorageAccount instances when their corresponding
            // configuration settings change in the service configuration file.
            RoleEnvironment.Changed += (sender, arg) =>
            {
                var key = CloudConfigurationManager.GetSetting(arg.Changes.OfType<RoleEnvironmentConfigurationSettingChange>().ToString());
                if (arg.Changes.OfType<RoleEnvironmentConfigurationSettingChange>()
                    .Any((change) => (change.ConfigurationSettingName == key.ToString())))
                {
                    // The corresponding configuration setting has changed, propagate the value
                    RoleEnvironment.GetConfigurationSettingValue(key.ToString());
                   // {
                        // In this case, the change to the storage account credentials in the
                        // service configuration is significant enough that the role needs to be
                        // recycled in order to use the latest settings. (for example, the 
                        // endpoint has changed)
                        RoleEnvironment.RequestRecycle();
                    //}
                }
            };
            //});
            #endregion
            return base.OnStart();
        }

        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            // If a configuration setting is changing
            if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
            {
                // Set e.Cancel to true to restart this role instance
                e.Cancel = true;
            }
        }

    }
}
