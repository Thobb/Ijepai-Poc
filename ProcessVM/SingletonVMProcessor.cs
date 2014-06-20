using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Blob;
using Thobb.Ijepai.Logging;
using Thobb.Ijepai.Messaging;

namespace Thobb.Ijepai.ProcessVM
{
    public sealed class SingletonVMProcessor
    {
        static readonly SingletonVMProcessor _instance = new SingletonVMProcessor();
        private string _id;
        private int _threadCount = 10;
        private int _waveHandEvery = 50;
        private List<Thread> _threads = new List<Thread>();
        private int _sleepingTime;

        private SingletonVMProcessor()
        {
            _sleepingTime = ConfigManager.ConfigManager.GetQueuePollingSleepTime();
            Log.Write(EventKind.Verbose, "SingletonVMProcessor.SingletonVMProcessor(): Rule Server initializing.");

            //Jean: Relocate this notification to after verifying the configuration
            //SendServerStartedNotification();


            _id = RoleEnvironment.CurrentRoleInstance.Id + "__" + RoleEnvironment.DeploymentId;
            if (!_id.ToLower().StartsWith("VMProcessorRole"))
            {
                int pos = _id.ToLower().IndexOf("VMProcessorRole");
                _id = _id.Substring(pos, _id.Length - pos);
            }
            Log.Write(EventKind.Verbose, "SingletonVMProcessor.SingletonVMProcessor(): Running Rule server as ID: " + _id);

            /// Reading thread configuration
            try
            {
                _threadCount = int.Parse(RoleEnvironment.GetConfigurationSettingValue("Threads"));
                if (_threadCount < 1)
                    _threadCount = 1;
            }
            catch (Exception)
            {
                _threadCount = 10;
                Log.Write(EventKind.Error, "SingletonVMProcessor.SingletonVMProcessor(): Error reading threading configuration, falling back to 10 threads");
            }
        }

        static SingletonVMProcessor()
        {
        }

        public static SingletonVMProcessor Instance
        {
            get
            {
                return _instance;
            }
        }
        public void Run()
        {
            Log.Write(EventKind.Verbose, "Singleton rule server running on instance " + _id);

            if (!IsServerConfigured())
            {
                Log.Write(EventKind.Error, string.Format("MediaProcessor Instance ID {0} is not properly configured.  Requesting a role recycle...", RoleEnvironment.CurrentRoleInstance.Id));
                SendServerNotStartedNotification();
            }
            else
            {
                Log.Write(EventKind.Information, string.Format("MediaProcessor Instance ID {0} configuration self-test passed.  Initiating service...", RoleEnvironment.CurrentRoleInstance.Id));
                SendServerStartedNotification();
            }


            //Logics
            //Action Events
            //Establish 2 Ques for each Action
            //Check for Priority Action Events and Process Them First
            //If there are No Priority Items in the Que then Process the Standard Items
            _activeMediaActionItems = MediaActionItem.GetAllMediaActionItems(); //Load action Assemble Info Prior to Initialization
            string[] defaultActionItemNames = GetAllActionItemCodes();
            //Neeraj Verma 2437: Need not call CreateActionItemQueues as Queues are gettign created in ProcessQueueItemsByPriority which is called from InitializeActionItemThreads
            // CreateActionItemQueues(defaultActionItemNames);  //Initialize the Queues Names

            InitializeActionItemThreads(defaultActionItemNames); //Start Thread for Processing Action Queues

            /// Launch threads
            for (int i = 0; i < _threadCount; i++) // Charles - 2468 - Multithreading.
            {
                Thread t = new Thread(() =>
                {
                    //Neeraj Verma: 2437, changing the logic to check the message queue.
                    ProcessRepositoryEventQueue();
                });
                _threads.Add(t);
                t.Start();
            }

            /// Monitor worker threads and perform maintenance
            while (true)
            {
                List<Thread> removedThreads = new List<Thread>();
                foreach (Thread thread in _threads)
                {
                    if (thread.ThreadState == System.Threading.ThreadState.Aborted || thread.ThreadState == System.Threading.ThreadState.Stopped)
                        /// If thread exited, remove it from our list of threads
                        removedThreads.Add(thread);
                }
                foreach (Thread thread in removedThreads)
                {
                    Log.Write(EventKind.Error, "SinglettonRuleServer.Run: One of the worker threads exited unexpectedfully - {0} threads remaining", _threads.Count);
                    _threads.Remove(thread);
                }
                if (_threads.Count == 0)
                {   /// If no threads are remaining, we leave the main loop and let Azure recycle everything by itself.
                    Log.Write(EventKind.Error, "SinglettonRuleServer.Run: Critical: no worker threads remaining, exiting main loop");
                    break;
                }
                Thread.Sleep(60000); /// 60 seconds, we really don't need to monitor very often as nothing should happen.
            }
        }



        private void InitializeActionItemThreads(string[] actionItemCodeNames)
        {
            //Launch a Thread for each Action Code Item
            foreach (string actionCodeName in actionItemCodeNames)
            {
                ThreadStart actionItemParam = delegate { ProcessQueueItemsByPriority(actionCodeName); };
                Thread actionItem = new Thread(actionItemParam);
                actionItem.Name = actionCodeName;
                actionItem.IsBackground = true;
                actionItem.Start();
                Log.Write(EventKind.Verbose, "Thread initialized for " + actionCodeName);
                Thread.Sleep(_sleepingTime);
                //Trace.WriteLine("Thread Started for Processing Items in :" + actionCodeName);
            }

        }

        /// <summary>
        /// Processes Queues by priority
        /// </summary>
        /// <param name="actionCodeName">String ActionCodeName</param>
        private void ProcessQueueItemsByPriority(string actionCodeName)
        {
            //getting queue names based on their priority
            List<AzureMessageQueueProvider> queues = new List<AzureMessageQueueProvider>();
            string priorityQueName = ConfigManager.ConfigManager.DefaultQueueName;
            var msgActionItemQueue = new AzureMessageQueueProvider(priorityQueName);
            

            //adding queue to the list
            queues.Add(msgActionItemQueue);
            
            int count = 0;
            while (true)
            {
                bool somethingProcessed = MonitorQueues(queues, actionCodeName);
                if (!somethingProcessed)
                {
                    if (count++ >= _waveHandEvery)
                    {
                        count = 0;
                        Log.Write(EventKind.Information,
                            "SingletonRuleServer.ProcessQueuesItemsByPriority(): no new requests in the message queue. Watcher will sleep for {0} seconds and log another message in {1} seconds.",
                            (_sleepingTime / 1000),
                            _waveHandEvery * (_sleepingTime / 1000));
                    }
                    Thread.Sleep(_sleepingTime);
                }
            }
        }


        /// <summary>
        /// Process one message from the queues, polling from queues by order of importance:
        /// - Priority
        /// - Medium
        /// - Standard
        /// - FTP
        /// </summary>
        /// <param name="queues">List of azure queues</param>
        /// <param name="actionCodeName">String ActionCodeName</param>
        private bool MonitorQueues(List<AzureMessageQueueProvider> queues, string actionCodeName)
        {
            foreach (var queue in queues)
            /// This will process queues in order. If something is found in one queue, we stop processing and return true, so that process is launched again from the top queue.
            {
                Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage processRequest = queue.Receive();
                if (processRequest != null)
                {
                    try
                    {
                        //Process the Action Item from this Dedicated Action Thread
                        Log.Write(EventKind.Information, string.Format("Processing message from queue using {0}", actionCodeName));
                        RepositoryObjectEvent objectEvent = RepositoryObjectEvent.CreateFromXml(processRequest.AsString);
                        if (objectEvent != null)
                        {
                            objectEvent.MediaActionItem = GetMediaActionItemInfo(objectEvent.RequestedActionItemCode);
                            if (ExecuteActionItem(objectEvent)) //execute dependent objects only if this succeeds
                            {
                                MediaActionPipeline pipeline = MediaActionPipeline.GetMediaActionPipeline(objectEvent.DesignatedPipeline);
                                if (pipeline != null)
                                {
                                    objectEvent.DesignatedPipeline = pipeline.Id.ToString();
                                    foreach (MediaActionItem actionItem in pipeline.GetMediaActionItemByPredecessor(actionCodeName))
                                    {
                                        //only load items with no dependency
                                        Log.Write(EventKind.Verbose, "Successfully executed predecessor of " + actionItem.Code + ".  Adding to pipeline " + objectEvent.DesignatedPipeline + " for execution.");
                                        AddPipelineActionToQueue(pipeline, objectEvent, actionItem);
                                    }
                                }
                            }
                        }
                        else
                            Log.Write(EventKind.Error, string.Format("SingltonRuleServer.MonitorQueues(): Unable to retrieve underlying processor request from XML: {0}", processRequest.AsString));
                    }
                    catch (Exception e)
                    {
                        Log.Write(EventKind.Error, string.Format("SingltonRuleServer.MonitorQueues(): ProcessQueuesItemsByPriority Error: {0}", Log.FormatExceptionInfo(e)));
                    }
                    return true;
                }
            }
            return false;
        }


        //public bool IsPriorityActionItemExistsInQue(string[] actionItemCodeNames)
        //{
        //    bool isExists = false;
        //    foreach (string actionName in actionItemCodeNames)
        //    {
        //        string priorityQueName = RepositoryObjectEvent.GetActionEventQueName(EnumActionEventCallType.Priority, actionName);
        //        AzureMessageQueueProvider msgQueue = new AzureMessageQueueProvider(priorityQueName);
        //        CloudQueueMessage queMessage = msgQueue.Peek();
        //        if (queMessage != null)
        //        {
        //            isExists = true;
        //            break;

        //        }
        //    }
        //    return isExists;

        //}


        /// <summary>
        /// 
        /// </summary>
        /// Neeraj Verma: 2437: 
        private void ProcessRepositoryEventQueue()
        {
            AzureMessageQueueProvider msgQueue = new AzureMessageQueueProvider(ConfigManager.DefaultQueueName);

            while (true)
            {
                //try to fetch message from queue
                Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage msgRepositoryEventItem = msgQueue.Receive();

                if (msgRepositoryEventItem != null)
                {
                    try
                    {
                        RepositoryObjectEvent objectEventMsg = RepositoryObjectEvent.CreateFromXml(msgRepositoryEventItem.AsString);//Using same RepositoryObjectEvent to Track any of the Que Items( repository-events,transfer_upload etc)
                        RepositoryObjectEvent objectEvent = new RepositoryObjectEvent();
                        objectEvent.LoadByKeys(TableServiceUtility.GetYearMonthDayHourString(objectEventMsg.EventTimeUtc), objectEventMsg.EventID.ToString());
                        objectEvent.assetIndexExist = objectEventMsg.assetIndexExist;//Persistent: Deepti. defect 1865. assigning assetIndexExist bool value from request object

                        if (String.IsNullOrEmpty(objectEvent.RequestedActionItemCode)) //Add Individual Action Items to Que from the Main repository-events que
                        {
                            foreach (ObjectRule rule in GetMatchingRules(objectEvent))
                            {
                                MediaActionPipeline pipeline = MediaActionPipeline.GetMediaActionPipeline(rule.PipelineID.ToString());
                                if (pipeline != null)
                                {
                                    Trace.WriteLine("Adding Pipeline Items to Que :" + rule.PipelineID.ToString());
                                    objectEvent.DesignatedPipeline = pipeline.Id.ToString();
                                    //Jean: There is an issue here.  This only saves the last matching pipeline to the table.  
                                    //Needs to be fixed for Release 1.1. Should still work since this sends the objectevent with the 
                                    //correct pipeline ID to the queue. 
                                    objectEvent.Update();
                                    List<MediaActionItem> pipelineActions = pipeline.GetMediaActionItemByPredecessor();
                                    ActionEventParameter result = ActionEventParameter.CreateFromXml(objectEvent.ActionEventParameters);

                                    //Persistent: Shweta : Adding the check for defect 1894. Flag check is used to decide if we need to generate renditions or not
                                    if (result.Params.ContainsKey("extractFlag"))
                                    {
                                        if (result.Params["extractFlag"] == "true")
                                        {
                                            foreach (MediaActionItem actionItem in pipelineActions)
                                            {
                                                if (actionItem.Code.Equals("extract_metadata"))
                                                {
                                                    //only load items with no dependency
                                                    AddPipelineActionToQueue(pipeline, objectEvent, actionItem);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (MediaActionItem actionItem in pipelineActions)
                                        {
                                            //only load items with no dependency
                                            AddPipelineActionToQueue(pipeline, objectEvent, actionItem);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Write(EventKind.Error, "Failed to load RepositoryObjectEvent from queue.  Error:" + e.Message, null);
                    }
                }
                else
                {
                    //no message found , sleep for some time
                    Thread.Sleep(_sleepingTime);
                }
            }
        }


        /// <summary>
        /// Executes the Individual Action Item
        /// </summary>
        /// <param name="objectEvent"></param>
        private bool ExecuteActionItem(RepositoryObjectEvent objectEvent)
        {
            bool success = false;
            try
            {
                Trace.WriteLine("Action Item Execution Started :" + objectEvent.RequestedActionItemCode);
                MediaActionItem actionItem = (MediaActionItem)objectEvent.MediaActionItem;
                string dllFullPath = _mountedDrive.Trim() + Path.Combine(ConfigManager.MediaActionsFolder, actionItem.Code, actionItem.AssemblyName);
                Assembly assembly = Assembly.LoadFrom(dllFullPath);
                Type type = assembly.GetType(actionItem.MediaActionItemClassName);
                //Interface - "MediaValet.ServiceLibrary.Rules.IMediaActionItem"
                if (type.GetInterface("MediaValet.ServiceLibrary.Rules.IMediaActionItem") != null)
                {
                    object ibaseObject = Activator.CreateInstance(type);
                    object[] arguments = new object[] { objectEvent, _mountedDrive };
                    Trace.WriteLine("Mounted drive:" + _mountedDrive);
                    object result;

                    ActionExecutionLog actionExecutionLog = new ActionExecutionLog();

                    if (!actionExecutionLog.LoadByKeys(objectEvent.EventID, objectEvent.DesignatedPipeline, actionItem.Code))
                    {
                        actionExecutionLog = new ActionExecutionLog(objectEvent.EventID, objectEvent.DesignatedPipeline, actionItem.Code);
                        actionExecutionLog.RepositoryObjectEventId = objectEvent.EventID;
                        if (string.IsNullOrEmpty(objectEvent.DesignatedPipeline))
                        {
                            actionExecutionLog.PipelineId = Guid.Empty.ToString();
                        }
                        else
                        {
                            actionExecutionLog.PipelineId = objectEvent.DesignatedPipeline;
                        }
                        actionExecutionLog.ActionCode = actionItem.Code;
                        actionExecutionLog.ActionStatus = EnumActionExecutionStatus.Open;
                        actionExecutionLog.StartTime = DateTime.UtcNow;
                        actionExecutionLog.EndTime = DateTime.UtcNow;
                        actionExecutionLog.Add();
                    }

                    try
                    {
                        result = type.InvokeMember("Execute", BindingFlags.Default | BindingFlags.InvokeMethod, null, ibaseObject, arguments);
                    }
                    catch (Exception e)
                    {
                        result = EnumMediaActionResult.Fail;
                        Log.Write(EventKind.Error, string.Format("SingletonRuleServer.ExecuteActionItem {0} Unhandled Exception: {1}", actionItem.Code, Log.FormatExceptionInfo(e)));
                    }
                    EnumMediaActionResult actionResult = (EnumMediaActionResult)result;
                    if (actionResult == EnumMediaActionResult.Fail)
                    {

                        Log.Write(EventKind.Error, string.Format("Error executing  action item {1} for RO EventId :{0}", objectEvent.EventID.ToString(), actionItem.Code));
                        actionExecutionLog.EndTime = DateTime.UtcNow;
                        actionExecutionLog.ActionStatus = EnumActionExecutionStatus.InProgress;
                        if (actionExecutionLog.FailedCount >= 3)
                        {
                            actionExecutionLog.ActionStatus = EnumActionExecutionStatus.Failed;
                            actionExecutionLog.Update();
                            if (actionItem.Code == "create_standard_renditions" && objectEvent.ObjectEventType == EnumObjectEventType.ActionRequested)
                            {
                                UserPickUpFile.UpdateBatchStatus(objectEvent.BatchId, objectEvent.UserId, "Failed to process renditions.");
                            }
                        }
                        else
                        {
                            actionExecutionLog.FailedCount = actionExecutionLog.FailedCount + 1;

                            actionExecutionLog.Update();

                            string actionQueName = RepositoryObjectEvent.GetActionEventQueueName(objectEvent.ActionEventCallType, objectEvent.RequestedActionItemCode);
                            //Re-Que the Object Event
                            Log.Write(EventKind.Error, string.Format("Sending failed action item {1} for RO EventId {0} back to the queue. ", objectEvent.EventID.ToString(), actionItem.Code));
                            AzureMessageQueueProvider msgQueue = new AzureMessageQueueProvider(actionQueName);
                            msgQueue.Send(objectEvent.ToXml());
                        }
                    }
                    else if (actionResult == EnumMediaActionResult.Success || actionResult == EnumMediaActionResult.Unknown)
                    {
                        //Jean: added objecteventtype since errors are getting generated from default ingestion, this should only work during download actions
                        if (actionItem.Code == "create_standard_renditions" && objectEvent.ObjectEventType == EnumObjectEventType.ActionRequested)
                        {
                            //Asset.CreateZipFileByBatchId(objectEvent);
                            //Check the Batch Status and Then Trigger Zip File


                            if (UserPickUpFile.UpdateBatchStatus(objectEvent.BatchId, objectEvent.UserId))
                            {

                                //Some action event parameter flag to check the if it is sharing as link
                                // add entries to Mapping table and then send mail
                                //if (  share as link)
                                if (objectEvent.ActionEventParameterObject.Params.ContainsKey("SharingMode"))
                                {
                                    string value = objectEvent.ActionEventParameterObject.Params["SharingMode"];
                                    if (value.Equals("External"))
                                    {
                                        if (objectEvent.ActionEventParameterObject.Params.ContainsKey("sharingType"))
                                        {

                                            string sharingType = objectEvent.ActionEventParameterObject.Params["sharingType"];
                                            if (sharingType.Equals(EnumShareType.Link.ToString()))
                                            {
                                                Asset.SharingCompleted(objectEvent);
                                            }
                                        }
                                        else
                                        {
                                            //Initiate the Zip only when all the Batch Items are Implemented
                                            Asset.CreateZipFileByBatchId(objectEvent);
                                        }
                                        //else asset.update mappings for sharing( sharecode, batchID, userID) and send mail
                                    }
                                    else
                                    {
                                        //Initiate the Zip only when all the Batch Items are Implemented
                                        Asset.CreateZipFileByBatchId(objectEvent);
                                    }
                                }
                                else
                                {
                                    Asset.CreateZipFileByBatchId(objectEvent);
                                }

                            }

                        }

                        actionExecutionLog.ActionStatus = EnumActionExecutionStatus.Completed;
                        actionExecutionLog.EndTime = DateTime.UtcNow;
                        actionExecutionLog.Update();
                        //If this is the last action in the pipline, reindex the asset
                        MediaActionPipeline pipeline = new MediaActionPipeline();
                        if (pipeline.LoadByKey(objectEvent.DesignatedPipeline))
                        {
                            //Get completed actions
                            List<ActionExecutionLog> completedActions = ActionExecutionLog.GetCompletedActions(objectEvent.EventID, objectEvent.DesignatedPipeline);
                            if (completedActions != null && objectEvent.BaseObjectType == EnumBaseObjectType.Asset)
                            {
                                if (pipeline.IsPipelineCompleted(completedActions))
                                {
                                    //clear the workfolders
                                    string eventworkfolder = Path.Combine(_mountedDrive, ConfigManager.WorkFolder, objectEvent.RepositoryID.ToString(), objectEvent.EventID.ToString());
                                    if (Directory.Exists(eventworkfolder))
                                    {
                                        Directory.Delete(eventworkfolder, true);
                                    }
                                    //reindex the asset. 
                                    Asset assetToReindex = new Asset();//Asset.GetAsset(objectEvent.RepositoryID, EnumRepositoryType.ActiveLibrary, objectEvent.ObjectID);
                                    if (assetToReindex.LoadByKeys(objectEvent.RepositoryID, EnumRepositoryType.ActiveLibrary, objectEvent.ObjectID, false) &&
                                        assetToReindex != null && assetToReindex.Repository.RepositoryType == EnumRepositoryType.ActiveLibrary)
                                    {
                                        Log.Write(EventKind.Verbose, "Rule Server: Pipepline " + pipeline.Name + " was completed for Event Id:" + objectEvent.EventID.ToString());
                                        Log.Write(EventKind.Verbose, "SourceAppCode in singletonrule server " + objectEvent.SourceAppCode);

                                        //Persistent: Deepti. defect 1865. checking if its new asset request, then send Add request to search service
                                        if (objectEvent.assetIndexExist)
                                        {
                                            //Amita: defect 2061 FTP upload queue
                                            assetToReindex.Update(false, true, false, objectEvent.SourceAppCode);
                                        }
                                        //else send update request to search service
                                        else
                                        {
                                            //Persistent: Amita: Modified to send additional parameter
                                            //Amita: defect 2061 FTP upload queue - passed sourceappcode
                                            assetToReindex.Update(false, true, true, objectEvent.SourceAppCode);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (actionResult != EnumMediaActionResult.Fail)
                    {
                        success = true;
                    }
                }
                else  //this class does not implement the interface, throw an error
                {
                    throw new Exception(actionItem.MediaActionItemClassName + " does not implement the IMediaActionItem interface.");
                }
            }
            catch (Exception e)
            {
                Log.Write(EventKind.Error, string.Format("Event ID {0} Pipeline Action Item execution error.  Details: {1}", objectEvent.EventID.ToString(), Log.FormatExceptionInfo(e)));
            }
            return success;
        }

        /// <summary>
        /// Push the Individual Action Item to the Que by using the Same RepositoryObjectEvent Object with a different value in ActionItemCode to identify the Action to be executed for the Item in the Que.
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="objectEvent"></param>
        /// <param name="actionItem"></param>
        private void AddPipelineActionToQueue(MediaActionPipeline pipeline, RepositoryObjectEvent objectEvent, MediaActionItem actionItem)
        {

            string actionQueName = RepositoryObjectEvent.GetActionEventQueueName(objectEvent.ActionEventCallType, actionItem.Code);
            Trace.WriteLine("Adding Standard Action Item to Que :" + actionQueName);
            objectEvent.RequestedActionItemCode = actionItem.Code;
            AzureMessageQueueProvider msgQueue = new AzureMessageQueueProvider(actionQueName);
            msgQueue.Send(objectEvent.ToXml());
        }

        private List<ObjectRule> GetMatchingRules(RepositoryObjectEvent objectEvent)
        {
            List<ObjectRule> matchingRules = new List<ObjectRule>();

            if (objectEvent.RepositoryID != Guid.Empty)
            {
                //Get RepositoryID + BaseObjectType + ObjectEventType
                List<ObjectRule> specificRules = ObjectRule.GetRepositoryEventRules(objectEvent.RepositoryID, objectEvent.ObjectEventType, objectEvent.BaseObjectType, objectEvent.SourceAppCode);
                matchingRules.AddRange(specificRules.OrderBy(r => r.Sequence));
            }

            //Get RepositoryType matches
            ContentRepository repository = new ContentRepository();
            if (repository.LoadContentRepository(objectEvent.RepositoryID.ToString()))
            {
                //Get RepositoryType + BaseObjectType + ObjectEventType
                List<ObjectRule> genericRules = ObjectRule.GetRepositoryTypeEventRules(repository.RepositoryType, objectEvent.ObjectEventType, objectEvent.BaseObjectType, objectEvent.SourceAppCode);
                matchingRules.AddRange(genericRules.OrderBy(r => r.Sequence));
            }
            else
            {
                Log.Write(EventKind.Error, "Cannot find repository " + objectEvent.RepositoryID.ToString());
            }
            return matchingRules;
        }

        private MediaActionItem GetMediaActionItemInfo(string actionCode)
        {
            MediaActionItem actionItem = _activeMediaActionItems.Find(a => { return a.Code == actionCode; });
            return actionItem;

        }

        //Persistent:Tushar: Send and email notification about the critical errrors that might crash the worker role.
        private static void SendErrorNotification(string errorMessage)
        {
            try
            {
                string toEmail = RoleEnvironment.GetConfigurationSettingValue("ToAddress");

                Hashtable tokenValues = new Hashtable();
                tokenValues.Add("ToEmail", toEmail);
                tokenValues.Add("ErrorMessage", errorMessage);
                tokenValues.Add("InstanceId", RoleEnvironment.CurrentRoleInstance.Id);
                tokenValues.Add("DeploymentEnvironment", ConfigManager.GetConfigurationSetting("DeploymentEnvironment"));
                Mailer mailer = new Mailer();
                mailer.Compose(Guid.Empty, "SERVER_ERROR", tokenValues, toEmail);
                mailer.SendMail();
            }
            catch (Exception ex)
            {
                Log.Write(EventKind.Error, string.Format("Error in sending email : {0}", Log.FormatExceptionInfo(ex)));
            }
        }

        //Neeraj verma:2437 if server is not configured properly
        private static void SendServerNotStartedNotification()
        {
            StringBuilder errMsg = new StringBuilder();
            errMsg.AppendLine(string.Format("MediaProcessor Instance ID {0} is not properly configured.  Requesting a role recycle...", RoleEnvironment.CurrentRoleInstance.Id));
            string appRoot = Environment.GetEnvironmentVariable("RoleRoot");
            if (appRoot != null)
            {
                string logFile = Path.Combine(appRoot + @"\", @"approot\MediaTools\mvtinstlog.txt");
                if (System.IO.File.Exists(logFile))
                {
                    string logText = System.IO.File.ReadAllText(logFile);
                    errMsg.AppendLine("----------------------------");
                    errMsg.AppendLine(logText);
                }
            }
            SendErrorNotification(errMsg.ToString());
            RoleEnvironment.RequestRecycle();
        }

        private static void SendServerStartedNotification()
        {
            try
            {
                string toEmail = RoleEnvironment.GetConfigurationSettingValue("ToAddress");

                Hashtable tokenValues = new Hashtable();
                tokenValues.Add("ToEmail", toEmail);
                tokenValues.Add("InstanceId", RoleEnvironment.CurrentRoleInstance.Id + " on " + RoleEnvironment.DeploymentId);
                tokenValues.Add("DateTime", DateTime.UtcNow.ToString("g"));   //g provides date time in "2/27/2009 12:12 PM" format
                tokenValues.Add("DeploymentEnvironment", ConfigManager.GetConfigurationSetting("DeploymentEnvironment"));
                Mailer mailer = new Mailer();
                mailer.Compose(Guid.Empty, "SERVER_STARTUP", tokenValues, toEmail);
                mailer.SendMail();
            }

            catch (Exception ex)
            {
                Log.Write(EventKind.Error, string.Format("Error in sending email : {0}", Log.FormatExceptionInfo(ex)));
            }
        }
      }
    }

