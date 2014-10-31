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

namespace Ijepai.LabScheduler
{
    public class Scheduler
    {
        private const string ConnectionString = @"Data Source=(LocalDb)\v11.0;AttachDbFilename=|DataDirectory|\aspnet-Ijepai.Web-20140505095652.mdf;Initial Catalog=aspnet-Ijepai.Web-20140505095652;Integrated Security=True";
        private const string SubscriptionID = "195686de-146a-4f9a-96c5-cd4071185af8";
        private const string CertThumbPrint = "AADE7A7D7E4992425FF0E882A621D33B3AD160D5-";

        public void Init()
        {
            Timer checkLabUptime = new Timer();
            checkLabUptime.Elapsed += new ElapsedEventHandler(checkLabUptime_Elapsed);
            checkLabUptime.Interval = 600000;
            checkLabUptime.Enabled = true;
        }
        async void checkLabUptime_Elapsed(object sender, ElapsedEventArgs e)
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = ConnectionString;
            conn.Open();
            SqlCommand labs = new SqlCommand("Select * from Labs where ((datediff(minute, start_time, getdate()) <= 30000) and (status = 'Scheduled'))", conn);
            SqlDataReader labsReader = labs.ExecuteReader();
            while (labsReader.Read())
            {
                string labName = labsReader.GetString(1);
                int labID = labsReader.GetInt32(0);
                SqlConnection conn3 = new SqlConnection();
                conn3.ConnectionString = ConnectionString;
                conn3.Open();
                SqlCommand UserNameCmd = new SqlCommand("Select * from AspNetUsers where Id='" + labsReader.GetString(7) + "'", conn3);
                SqlDataReader UserNameReader = UserNameCmd.ExecuteReader();
                UserNameReader.Read();
                string UserName = UserNameReader.GetString(1);
                conn3.Close();
                SqlConnection conn1 = new SqlConnection();
                conn1.ConnectionString = ConnectionString;
                conn1.Open();
                SqlCommand participantList = new SqlCommand("Select * from LabParticipants where LabID = " + labID, conn1);
                SqlDataReader participantReader = participantList.ExecuteReader();
                SqlConnection conn5 = new SqlConnection();
                conn5.ConnectionString = ConnectionString;
                conn5.Open();
                SqlCommand labConfigOb = new SqlCommand("Select * from LabConfigurations where LabID = " + labID, conn5);
                SqlDataReader labConfigReader = labConfigOb.ExecuteReader();
                labConfigReader.Read();
                string MachineSize = labConfigReader.GetString(3);
                string OS = labConfigReader.GetString(4);
                conn5.Close();
                while (participantReader.Read())
                {
                    string email = participantReader.GetString(1);
                    string machineLink = "http://ijepai.azurewebsites.net/" + UserName + "/" + labName + "/" + email.Replace("@", "_");
                    Mailer mail = new Mailer("rahulkarn@gmail.com","Ijepai");
                    mail.Compose(machineLink, email);
                    mail.SendMail();
                    bool status = await CreateVM(labName, email.Replace("@", "_"), "ijepai@1", MachineSize, OS).ConfigureAwait(continueOnCapturedContext: false);
                }
                conn1.Close();
                SqlConnection conn4 = new SqlConnection();
                conn4.ConnectionString = ConnectionString;
                conn4.Open();
                SqlCommand updateLabsStatus = new SqlCommand("update labs set status='Provisioning' where id = " + labID, conn4);
                updateLabsStatus.ExecuteNonQuery();
                conn4.Close();
            }
            conn.Close();
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
    }
}