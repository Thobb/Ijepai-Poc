using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using SMLibrary;
using System.Xml;
using System.Xml.Linq;
using IjepaiMailer;
using Ijepai.Encrypt;

//using Ijepai.Logging;

namespace Ijepai.LabScheduler
{
    public class Scheduler
    {
        private const string ConnectionString = @"Server=tcp:ja9acfyfaa.database.windows.net,1433;Database=ijpie;User ID=thobb@ja9acfyfaa;Password=th0bb@123;Trusted_Connection=False;Encrypt=True;Connection Timeout=30; MultipleActiveResultSets=true";
        private const string SubscriptionID = "195686de-146a-4f9a-96c5-cd4071185af8";
        private const string CertThumbPrint = "2A82DC33E49F9CB2C7F12DE64859868387A7C69C";
        string password = "1234Test!";


        SqlConnection conn = new SqlConnection();


        public void Init()
        {
            conn.ConnectionString = ConnectionString;
            conn.Open();  
            Timer checkLabUptime = new Timer();
            checkLabUptime.Elapsed += new ElapsedEventHandler(checkLabUptime_Elapsed);
            checkLabUptime.Interval = 60000;
            checkLabUptime.Enabled = true;

            Timer checkLabEndTime = new Timer();
            checkLabEndTime.Elapsed += new ElapsedEventHandler(checkLabEndTime_Elapsed);
            checkLabEndTime.Interval = 60000;
            checkLabEndTime.Enabled = true;
        }
        async void checkLabUptime_Elapsed(object sender, ElapsedEventArgs e)
        {
            try

            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.ConnectionString = ConnectionString;

                    conn.Open();
                }
            }
            catch (Exception ex)
            {
                //Log.Write(EventKind.Critical, Log.FormatExceptionInfo(ex), null);
               

            }
            SqlCommand labs = new SqlCommand("Select * from Labs where ((datediff(minute, start_time, getdate()) = -6) and (status = 'Scheduled'))", conn);

            SqlDataReader labsReader = labs.ExecuteReader();
            if (labsReader != null)
            {
                try

                {
                    while (labsReader.Read())
                    {
                        string labName = labsReader.GetString(1);
                        int labID = labsReader.GetInt32(0);                       
                        SqlCommand UserNameCmd = new SqlCommand("Select * from AspNetUsers where Id='" + labsReader.GetString(7) + "'", conn);
                        SqlDataReader UserNameReader = UserNameCmd.ExecuteReader();
                        UserNameReader.Read();
                        string UserName = UserNameReader.GetString(1);                       
                        SqlCommand participantList = new SqlCommand("Select * from LabParticipants where LabID = " + labID, conn);
                        SqlDataReader participantReader = participantList.ExecuteReader();                     
                        SqlCommand labConfigOb = new SqlCommand("Select * from LabConfigurations where LabID = " + labID, conn);
                        SqlDataReader labConfigReader = labConfigOb.ExecuteReader();
                        labConfigReader.Read();
                        string MachineSize = labConfigReader.GetString(6);
                        string OS = labConfigReader.GetString(4);
                        String serviceName = string.Empty;
                        while (participantReader.Read())
                        {
                            string passPhrase = "th0bb@123";
                            SqlCommand updateLabsStatus = new SqlCommand("update labs set status='Provisioning' where id = " + labID, conn);
                            updateLabsStatus.ExecuteNonQuery();
                            string email = participantReader.GetString(1);
                            serviceName = CreateServiceName(labName, email);
                            string encService = StringCipher.Encrypt(serviceName+ ".cloudapp.net",passPhrase);
                            string encUserName = StringCipher.Encrypt("administrator",passPhrase);
                            string encPassword = StringCipher.Encrypt(password,passPhrase);

                            string machineLink = "http://vmengine.azurewebsites.net/?" + StringCipher.initVector + "/" + "LB" + "/" + encService + "/" + encUserName + "/" + encPassword;
                            Mailer mail = new Mailer("rahulkarn@gmail.com", "Ijepai");
                            mail.Compose(machineLink, email);
                            bool status = await CreateVM(serviceName, "VM1", password, MachineSize, OS).ConfigureAwait(continueOnCapturedContext: false);
                            mail.SendMail();
                        }
                        SqlCommand updateLabsStatusRunning = new SqlCommand("update labs set status='Available' where id = " + labID, conn);
                        updateLabsStatusRunning.ExecuteNonQuery();
                        
                        SqlCommand addVMPath = new SqlCommand("insert into LabVMs VALUES ('" + labID + "','" + serviceName + "')", conn);
                        addVMPath.ExecuteNonQuery();
                      }
                }
                catch (Exception exc)
                {
                    //Log.Write(EventKind.Critical, Log.FormatExceptionInfo(exc), null);

                    //Log Exception
                }
            }
        }

        async void checkLabEndTime_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.ConnectionString = ConnectionString;
                    conn.Open();
                }
            }
            catch (Exception ex)
            {
               // Log.Write(EventKind.Critical, Log.FormatExceptionInfo(ex), null);
                //Log exception message

            }
            SqlCommand closeLabs = new SqlCommand("Select * from Labs where ((datediff(minute, end_time, getdate())) = 0) ", conn);
            SqlDataReader closeLabsReader = closeLabs.ExecuteReader();
            if (closeLabsReader != null)
            {
                try
                {
                    while (closeLabsReader.Read())
                    {
                        string labName = closeLabsReader.GetString(1);
                        int labID = closeLabsReader.GetInt32(0);                        
                        SqlCommand VMList = new SqlCommand("Select * from LabVMs where Lab_ID = " + labID, conn);
                        SqlDataReader VMListReader = VMList.ExecuteReader();
                        while (VMListReader.Read())
                        {
                            SqlCommand updateLabsStatus = new SqlCommand("update labs set status='Deleting' where id = " + labID, conn);
                            updateLabsStatus.ExecuteNonQuery();

                            string serviceName = VMListReader.GetString(1);
                            SqlCommand closeParticipantList = new SqlCommand("Delete from LabParticipants where LabID = " + labID, conn);
                            SqlDataReader closeParticipantReader = closeParticipantList.ExecuteReader();

                            SqlCommand closeLabConfigOb = new SqlCommand("Delete from LabConfigurations where LabID = " + labID, conn);
                            SqlDataReader closeLabConfigReader = closeLabConfigOb.ExecuteReader();

                            SqlCommand deleteVMPath = new SqlCommand("Delete from LabVMs where Lab_ID = " + labID, conn);
                            deleteVMPath.ExecuteNonQuery();

                            SqlCommand deleteLabsStatus = new SqlCommand("Delete from Labs where id = " + labID, conn);
                            deleteLabsStatus.ExecuteNonQuery();
                            
                            VMManager vmm = new VMManager(SubscriptionID, CertThumbPrint);
                            string status = await vmm.DeleteQCVM(serviceName).ConfigureAwait(continueOnCapturedContext: false);
                        }
                    }
                }
                catch (Exception exc)
                {
                    //Log Exception
                   // Log.Write(EventKind.Critical, Log.FormatExceptionInfo(exc), null);
                }
            }
        }

        async private Task<bool> CreateVM(string serviceName, string vmName, string password, string Machine_Size, string OS)
        {
            VMManager vmm = new VMManager(SubscriptionID, CertThumbPrint);

            if (await vmm.IsServiceNameAvailable(serviceName).ConfigureAwait(continueOnCapturedContext: false) == false)
            {
                return false;
            }

            XDocument vm = vmm.NewAzureVMConfig(vmName, Machine_Size, OS, GetOSDiskMediaLocation(vmName), true);

            vm = vmm.NewWindowsProvisioningConfig(vm, vmName, password);
            vm = vmm.NewNetworkConfigurationSet(vm);
            vm = vmm.AddNewInputEndpoint(vm, "web", "TCP", 80, 80);
            vm = vmm.AddNewInputEndpoint(vm, "rdp", "TCP", 3389, 3389);

            var builder = new StringBuilder();
            var settings = new XmlWriterSettings()
            {
                Indent = true
            };
            using (var writer = XmlWriter.Create(builder, settings))
            {
                vm.WriteTo(writer);
            }

            String requestID_cloudService = await vmm.NewAzureCloudService(serviceName, "West US", String.Empty).ConfigureAwait(continueOnCapturedContext: false);

            OperationResult result = await vmm.PollGetOperationStatus(requestID_cloudService, 5, 120).ConfigureAwait(continueOnCapturedContext: false); ;
            String requestID_createDeployment;
            if (result.Status == OperationStatus.Succeeded)
            {
                // VM creation takes too long so we'll check it later
                requestID_createDeployment = await vmm.NewAzureVMDeployment(serviceName, vmName, String.Empty, vm, null).ConfigureAwait(continueOnCapturedContext: false);
            }
            else
            {
                requestID_createDeployment = "";
            }
            return true;
        }
        private String GetOSDiskMediaLocation(string vmName)
        {
            String osdiskmedialocation = String.Format("https://{0}.blob.core.windows.net/vhds/{1}-OS-{2}.vhd", "Ijepai", vmName, Guid.NewGuid().ToString());
            return osdiskmedialocation;
        }
        private String CreateServiceName(string Labname, string Username)
        {
            String[] emailComp = Username.Split('@');
            String[] domainComp = emailComp[1].Split('.');
            return Labname + emailComp[0] + "-" + domainComp[0];
        }
    }
}
