using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Configuration;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure;

[assembly: CLSCompliant(true)]

namespace Thobb.Ijepai.ConfigManager
{
    public static class ConfigManager
    {
        public const string CSConfigStringPrefix = "CSConfigName";
        //Jean: These settings refer to web.config of Portal
        public const string DefaultMembershipTableNameConfigurationString = "DefaultMembershipTableName";
        public const string DefaultRoleTableNameConfigurationString = "DefaultRoleTableName";
        public const string DefaultSessionTableNameConfigurationString = "DefaultSessionTableName";
        public const string DefaultSessionContainerNameConfigurationString = "DefaultSessionContainerName";
        public const string DefaultProfileContainerNameConfigurationString = "DefaultProfileContainerName";
        public const string DefaultProviderApplicationNameConfigurationString = "DefaultProviderApplicationName";
        public const string DefaultApplicationModeConfigurationString = "ApplicationMode";

        //Jean: These settings refer to Windows Azure Storage
        public const string DefaultProviderApplicationName = "Ijepai";
        public const string DefaultMembershipTableName = "AppUserEntity";
        public const string DefaultRoleTableName = "RoleEntity";
        public const string DefaultSessionTableName = "SessionEntity";
        //Containers
        public const string DefaultSessionContainerName = "user-sessions"; //all lowercase
        public const string DefaultProfileContainerName = "user-profiles"; //all lowercase
        public const string SystemContainerName = "system";
        public const string SearchCatalogContainerNamePrefix = "search";
        public const string CacheContainerName = "cache";
        //Queues
        public const string DefaultQueueName = "repository-events";//all lowercase, between 3 and 63 chars,
        public const string SearchIndexerQueueName = "search-indexer-requests";
        public const string TableIndexerQueueName = "table-indexer-requests";
        public const string PriorityIndexerQueueName = "indexer-requests-priority";
        public const string StandardIndexerQueueName = "indexer-requests-standard";
        public const string MediumIndexerQueueName = "indexer-requests-medium";
        public const string FTPIndexerQueueName = "indexer-requests-ftp";

        public const string StatsUpdaterQueueName = "stats-updater-requests";
        //These settings refer to the CloudService configuration service
        public const string DefaultTableStorageConnString = "StorageConnectionString";//"DataRepository";
        public const string DefaultBlobStorageConnString = "StorageConnectionString";
        public const string DefaultQueueStorageConnString = "StorageConnectionString";
        public static readonly string DefaultTableStorageEndpointConfigurationString = "TableStorageEndpoint";
        public static readonly string DefaultAccountNameConfigurationString = "AccountName";
        public static readonly string DefaultAccountSharedKeyConfigurationString = "AccountSharedKey";
        public static readonly string DefaultBlobStorageEndpointConfigurationString = "BlobStorageEndpoint";
        public static readonly string UseSharedAccessSignature = "UseSharedAccessSignatureForSharing";

        //public static readonly DateTime MinSupportedDateTime = DateTime.FromFileTime(0).ToUniversalTime().AddYears(200);
        // 1/1/1753 is the minimum supported date by Azure Table Service.  Even if you save null, this is what is going to be saved.
        public static readonly DateTime MinSupportedDateTime = DateTime.Parse("1753-01-01T00:00:00.0000000Z ").ToUniversalTime();
        public static readonly int MaxStringPropertySizeInBytes = 64 * 1024;
        public static readonly int MaxStringPropertySizeInChars = MaxStringPropertySizeInBytes / 2;

        //system & userfolder Paths
        public const string ActionsFolder = "actions";
        public const string ToolsFolder = "Apps";
        public const string CommonAssets = "assets";
        public const string IccProfiles = "icc-profiles";
        public const string CommonFiles = "CommonFiles";
        public const string UserDropFolder = "uploads";
        public const string UserPickupFolder = "downloads";
        public const string WorkFolder = "work-in-progress";
        public const string PWCFolder = "pwc";
        //Action Event Parameters
        public const string ActionEventParamWidth = "width";
        public const string ActionEventParamHeight = "height";
        public const string ActionEventParamZipFileName = "zipFileName";
        public const string ActionEventParamZipAndEmail = "zipAndEmail";
        public const int MaxEmailAttachmentSize = 10;

        public static readonly string SystemAdminRoleName = "System Administrator";
        public static readonly string OrgUnitAdminRoleName = "Administrators";
        //public static readonly string StandardUserRoleName = "User";
        public static readonly string StandardUserRoleName = "Member";
        public static readonly string GuestUserRoleName = "Guest";

        //If Email From Address is not Configured use this
        public static readonly string MediavaletDefaultFromEmailAddress = "service@thobb.com";
        //Serialization
        public const string DefaultXMLNamespace = "http://www.w3.org/2001/XMLSchema";


        //specify the GUID to be used by the IjepaiServiceAccount
        public static readonly Guid MediaValetServiceAccountID = new Guid("66bc4396-b7e1-40a8-a495-ac7ce5799230");

        public static readonly string CommonPassword = "Ch@ngM3!";

        public static readonly string IndexerBlobContainer = "indexer-transactions";

        public static string GetConfigurationSetting(string configurationString)
        {
            return GetConfigurationSetting(configurationString, string.Empty, true);
        }

        public static string GetConfigurationSetting(string configurationString, string defaultValue)
        {
            return GetConfigurationSetting(configurationString, defaultValue, false);
        }

        /// <summary>
        /// Gets a configuration setting from application settings in the Web.config or App.config file. 
        /// When running in a hosted environment, configuration settings are read from the settings specified in 
        /// .cscfg files (i.e., the settings are read from the fabrics configuration system).
        /// </summary>
        public static string GetConfigurationSetting(string configurationString, string defaultValue, bool throwIfNull)
        {
            if (string.IsNullOrEmpty(configurationString))
            {
                throw new ArgumentException("The parameter configurationString cannot be null or empty.");
            }

            string ret = null;

            // first, try to read from appsettings
            ret = TryGetAppSetting(configurationString);

            // settings in the csc file overload settings in Web.config

            try
            {
                if (RoleEnvironment.IsAvailable)
                {
                    string cscRet = TryGetConfigurationSetting(configurationString);
                    if (!string.IsNullOrEmpty(cscRet))
                    {
                        ret = cscRet;
                    }

                    // if there is a csc config name in the app settings, this config name even overloads the 
                    // setting we have right now
                    string refWebRet = TryGetAppSetting(CSConfigStringPrefix + configurationString);
                    if (!string.IsNullOrEmpty(refWebRet))
                    {
                        cscRet = TryGetConfigurationSetting(refWebRet);
                        if (!string.IsNullOrEmpty(cscRet))
                        {
                            ret = cscRet;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Compatibility with visual studio 2013
            }

            // if we could not retrieve any configuration string set return value to the default value
            if (string.IsNullOrEmpty(ret) && defaultValue != null)
            {
                ret = defaultValue;
            }

            if (string.IsNullOrEmpty(ret) && throwIfNull)
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.InstalledUICulture, "Cannot find configuration string {0}.", configurationString));
            }
            return ret;
        }

        public static string GetConfigurationSettingFromNameValueCollection(NameValueCollection config, string valueName)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (valueName == null)
            {
                throw new ArgumentNullException("valueName");
            }

            string sValue = config[valueName];

            if (RoleEnvironment.IsAvailable)
            {
                // settings in the hosting configuration are stronger than settings in app config
                string cscRet = TryGetConfigurationSetting(valueName);
                if (!string.IsNullOrEmpty(cscRet))
                {
                    sValue = cscRet;
                }

                // if there is a csc config name in the app settings, this config name even overloads the 
                // setting we have right now
                string refWebRet = config[CSConfigStringPrefix + valueName];
                if (!string.IsNullOrEmpty(refWebRet))
                {
                    cscRet = TryGetConfigurationSetting(refWebRet);
                    if (!string.IsNullOrEmpty(cscRet))
                    {
                        sValue = cscRet;
                    }
                }
            }
            return sValue;
        }

        public static bool GetBooleanValue(NameValueCollection config, string valueName, bool defaultValue)
        {
            string sValue = GetConfigurationSettingFromNameValueCollection(config, valueName);

            if (string.IsNullOrEmpty(sValue))
            {
                return defaultValue;
            }

            bool result;
            if (bool.TryParse(sValue, out result))
            {
                return result;
            }
            else
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.InstalledUICulture, "The value must be boolean (true or false) for property '{0}'.", valueName));
            }
        }

        public static int GetIntValue(NameValueCollection config, string valueName, int defaultValue, bool zeroAllowed, int maxValueAllowed)
        {
            string sValue = GetConfigurationSettingFromNameValueCollection(config, valueName);

            if (string.IsNullOrEmpty(sValue))
            {
                return defaultValue;
            }

            int iValue;
            if (!Int32.TryParse(sValue, out iValue))
            {
                if (zeroAllowed)
                {
                    throw new ConfigurationErrorsException(string.Format(CultureInfo.InstalledUICulture, "The value must be a non-negative 32-bit integer for property '{0}'.", valueName));
                }

                throw new ConfigurationErrorsException(string.Format(CultureInfo.InstalledUICulture, "The value must be a positive 32-bit integer for property '{0}'.", valueName));
            }

            if (zeroAllowed && iValue < 0)
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.InstalledUICulture, "The value must be a non-negative 32-bit integer for property '{0}'.", valueName));
            }

            if (!zeroAllowed && iValue <= 0)
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.InstalledUICulture, "The value must be a positive 32-bit integer for property '{0}'.", valueName));
            }

            if (maxValueAllowed > 0 && iValue > maxValueAllowed)
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.InstalledUICulture, "The value '{0}' can not be greater than '{1}'.", valueName, maxValueAllowed.ToString(CultureInfo.InstalledUICulture)));
            }

            return iValue;
        }

        public static string GetStringValue(NameValueCollection config, string valueName, string defaultValue, bool nullAllowed)
        {
            string sValue = GetConfigurationSettingFromNameValueCollection(config, valueName);

            if (string.IsNullOrEmpty(sValue) && nullAllowed)
            {
                return null;
            }
            else if (string.IsNullOrEmpty(sValue) && defaultValue != null)
            {
                return defaultValue;
            }
            else if (string.IsNullOrEmpty(sValue))
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.InstalledUICulture, "The parameter '{0}' must not be empty.", valueName));
            }

            return sValue;
        }

        public static string GetStringValueWithGlobalDefault(NameValueCollection config, string valueName, string defaultConfigString, string defaultValue, bool nullAllowed)
        {
            string sValue = GetConfigurationSettingFromNameValueCollection(config, valueName);

            if (string.IsNullOrEmpty(sValue))
            {
                sValue = GetConfigurationSetting(defaultConfigString, null);
            }

            if (string.IsNullOrEmpty(sValue) && nullAllowed)
            {
                return null;
            }
            else if (string.IsNullOrEmpty(sValue) && defaultValue != null)
            {
                return defaultValue;
            }
            else if (string.IsNullOrEmpty(sValue))
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.InstalledUICulture, "The parameter '{0}' must not be empty.", valueName));
            }

            return sValue;
        }

        public static string GetInitExceptionDescription(StorageCredentials table, Uri tableBaseUri, StorageCredentials blob, Uri blobBaseUri)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(GetInitExceptionDescription(table, tableBaseUri, "table storage configuration"));
            builder.Append(GetInitExceptionDescription(blob, blobBaseUri, "blob storage configuration"));
            return builder.ToString();
        }

        public static string GetInitExceptionDescription(StorageCredentials info, Uri baseUri, string desc)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("The reason for this exception is typically that the endpoints are not correctly configured. " + Environment.NewLine);
            if (info == null)
            {
                builder.Append("The current " + desc + " is null. Please specify a table endpoint!" + Environment.NewLine);
            }
            else
            {
                builder.Append("The current " + desc + " is: " + baseUri + Environment.NewLine);
                builder.Append("Please also make sure that the account name and the shared key are specified correctly. This information cannot be shown here because of security reasons.");
            }
            return builder.ToString();
        }

        private static string TryGetConfigurationSetting(string configName)
        {
            string ret = null;
            try
            {
                ret = RoleEnvironment.GetConfigurationSettingValue(configName);
            }
            catch (RoleEnvironmentException)
            {
                return null;
            }
            return ret;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Make sure that no error condition prevents environment from reading service configuration.")]
        public static string TryGetAppSetting(string configName)
        {
            string ret = null;
            try
            {
                ret = ConfigurationManager.AppSettings[configName];
            }
            // some exception happened when accessing the app settings section
            // most likely this is because there is no app setting file
            // this is not an error because configuration settings can also be located in the cscfg file, and explicitly 
            // all exceptions are captured here
            catch (Exception)
            {
                return null;
            }
            return ret;
        }

        //Jean: Refactored, moved from Data Management component
        public static Action<string, Func<string, bool>> GetConfigurationSettingPublisher()
        {
            try
            {
                if (RoleEnvironment.IsAvailable)
                    return (configName, configSetter) =>
                            configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));
            }
            catch (Exception)
            {
                // Compatibility with VisualStudio 2013
            }
            return (configName, configSetter) =>
            configSetter(ConfigurationManager.AppSettings[configName]);
        }

        public static XmlSerializerNamespaces GetNamespaces()
        {

            XmlSerializerNamespaces ns;
            ns = new XmlSerializerNamespaces();
            ns.Add("xs", "http://www.w3.org/2001/XMLSchema");
            ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            ns.Add("xs", "http://www.w3.org/2001/XMLSchema" + "/AC/");
            ns.Add("xs", "http://www.w3.org/2001/XMLSchema" + "/ACList/");

            return ns;

        }

        public static string GetCMISServiceBaseUri(Uri uriInfo)
        {
            string serviceUri = ConfigurationManager.AppSettings["RaServiceBaseUri"];

            if (ConfigurationManager.AppSettings["ApplicationMode"] == "development")
            {
                serviceUri = String.Format("{0}", ConfigurationManager.AppSettings["RaServiceBaseUri"] + ":" + uriInfo.Port.ToString());
            }
            return serviceUri;
        }
        public static string GetCMISServiceUri(Uri uriInfo)
        {
            string serviceUri = String.Format("{0}{1}", ConfigurationManager.AppSettings["RaServiceBaseUri"], "/MediaValetCmisService.svc/");

            if (ConfigurationManager.AppSettings["ApplicationMode"] == "development")
            {
                serviceUri = String.Format("{0}{1}", ConfigurationManager.AppSettings["RaServiceBaseUri"] + ":" + uriInfo.Port.ToString(), "/MediaValetCmisService.svc/");
            }
            return serviceUri;
        }
        public static string GetSearchServiceUri()
        {
            string serviceUri = string.Empty;
            try
            {
                serviceUri = RoleEnvironment.GetConfigurationSettingValue("SearchServiceEndPoint");
            }
            catch (Exception e)
            {
                serviceUri = ConfigManager.GetConfigurationSetting("SearchServiceEndPoint");
                if (string.IsNullOrEmpty(serviceUri))
                {
                    System.Diagnostics.Trace.WriteLine("ConfigManager.GetSearchServiceUri() error: " + e.Message);
                    throw e;
                }
            }

            //No More Referred from Web.config
            //if (ConfigurationManager.AppSettings["ApplicationMode"] == "development")
            //{
            //    serviceUri = ConfigurationManager.AppSettings["SearchServiceEndPoint"];
            //}
            return serviceUri;
        }

        public static string GetTableIndexerServiceUri()
        {
            string serviceUri = string.Empty;
            try
            {
                serviceUri = RoleEnvironment.GetConfigurationSettingValue("TableIndexerServiceEndPoint");
            }
            catch (Exception e)
            {
                serviceUri = ConfigManager.GetConfigurationSetting("TableIndexerServiceEndPoint");
                if (string.IsNullOrEmpty(serviceUri))
                {
                    System.Diagnostics.Trace.WriteLine("ConfigManager.GetTableIndexerServiceUri() error: " + e.Message);
                    throw e;
                }
            }
            //No More Referred from Web.config
            //if (ConfigurationManager.AppSettings["ApplicationMode"] == "development")
            //{
            //    serviceUri = ConfigurationManager.AppSettings["TableIndexerServiceEndPoint"];
            //}
            return serviceUri;
        }

        public static string GetBlobContainerBaseUri()
        {
            string blobBaseUri = ConfigManager.TryGetAppSetting("BlobContainerBaseUri");
            return blobBaseUri;
        }


        public static long GetEventSyncTimeOutSeconds()
        {
            long timeoutSeconds = 30;
            string timeoutValue = ConfigManager.TryGetAppSetting("EventSyncTimeOutSeconds");
            if (!string.IsNullOrEmpty(timeoutValue))
            {
                int timeoutSec = 0;
                int.TryParse(timeoutValue, out timeoutSec);
                if (timeoutSec > 0) timeoutSeconds = timeoutSec;
            }

            return timeoutSeconds;
        }

        /// <summary>
        /// Get DaylightSaving time
        /// //Persistent:Minal:1660 :Polling empty queue sleeptime
        /// </summary>
        /// <param name="TimeZoneName"></param>
        /// <returns></returns>
        public static DateTime GetDaylightSavingTime(string TimeZoneName)
        {
            DateTime localDate = System.DateTime.Now.ToUniversalTime();

            // Get time zone info 
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneName);

            TimeSpan timeDiffUtcClient = tz.BaseUtcOffset;
            localDate = System.DateTime.Now.ToUniversalTime().Add(timeDiffUtcClient);

            if (tz.SupportsDaylightSavingTime && tz.IsDaylightSavingTime(localDate))
            {
                TimeZoneInfo.AdjustmentRule[] rules = tz.GetAdjustmentRules();
                foreach (var adjustmentRule in rules)
                {
                    if (adjustmentRule.DateStart <= localDate && adjustmentRule.DateEnd >= localDate)
                    {
                        localDate = localDate.Add(adjustmentRule.DaylightDelta);
                    }
                }

            }

            return localDate;
        }


        /// <summary>
        /// Get the sleep time based on zone and business days and hours 
        /// //Persistent:Minal:1660 :Polling empty queue sleeptime
        /// </summary>
        /// <returns></returns>
        public static int GetQueuePollingSleepTime()
        {
            int sleepTime = 3000;
            try
            {
                string timeZone = RoleEnvironment.GetConfigurationSettingValue("TimeZone");
                DateTime currentDnT = GetDaylightSavingTime(timeZone);


                string[] businessHours = RoleEnvironment.GetConfigurationSettingValue("BusinessHours").Split('-');
                int businessHourStart = Convert.ToInt32(businessHours[0]);
                int businessHourEnd = Convert.ToInt32(businessHours[1]);

                //business Days
                if (currentDnT.DayOfWeek.ToString() != "Sunday" && currentDnT.DayOfWeek.ToString() != "Saturday")
                {
                    if (currentDnT.Hour >= businessHourStart && currentDnT.Hour <= businessHourEnd)
                    {
                        string sleepPeriod = RoleEnvironment.GetConfigurationSettingValue("BusinessHoursSleepTime");
                        sleepTime = Convert.ToInt32(sleepPeriod) * 1000; //milliseconds
                    }
                    else
                    {
                        string sleepPeriod = RoleEnvironment.GetConfigurationSettingValue("OffHoursSleepTime");
                        sleepTime = Convert.ToInt32(sleepPeriod) * 1000; //milliseconds
                    }
                }
                else
                {
                    //off day 
                    string sleepPeriod = RoleEnvironment.GetConfigurationSettingValue("WeekendSleepTime");
                    sleepTime = Convert.ToInt32(sleepPeriod) * 1000; //milliseconds

                }

            }
            catch (RoleEnvironmentException ex)
            {
                //if the config settings are missing default is 3 sec
                sleepTime = 3000; //milliseconds
            }
            catch (Exception general)
            {
                //if any general exception default sleeptime is 3 sec
                sleepTime = 3000;
            }
            return sleepTime;
        }
    }
}
