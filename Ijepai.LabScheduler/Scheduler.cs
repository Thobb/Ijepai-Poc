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
        public void Init()
        {
            Timer checkLabUptime = new Timer();
            checkLabUptime.Elapsed += new ElapsedEventHandler(checkLabUptime_Elapsed);
            checkLabUptime.Interval = 60000;
            checkLabUptime.Enabled = true;
        }

        async void checkLabUptime_Elapsed(object sender, ElapsedEventArgs e)
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=(LocalDb)\v11.0;AttachDbFilename=|DataDirectory|\aspnet-Ijepai.Web-20140505095652.mdf;Initial Catalog=aspnet-Ijepai.Web-20140505095652;Integrated Security=True";
            conn.Open();
            SqlCommand labs = new SqlCommand("Select * from Labs where ((datediff(minute, start_time, getdate()) <= 30) and (status = 'Scheduled'))");
            SqlDataReader labsReader = labs.ExecuteReader();
            while (labsReader.Read())
            {
                string labName = labsReader.GetString(1);
                int labID = labsReader.GetInt32(0);
                SqlCommand participantList = new SqlCommand("Select * from LabParticipants where LabID = " + labID);
                SqlDataReader participantReader = participantList.ExecuteReader();
                while (participantReader.Read())
                {
                    string email = participantReader.GetString(1);
                    Mailer mail = new Mailer("rahulkarn@gmail.com","Ijepai");
                    mail.Compose(" ", "bhagwati.indoria@gmail.com");
                    mail.SendMail();
                    bool status = await CreateVM(labName, labName, "ijepai@1", "", "").ConfigureAwait(continueOnCapturedContext: false);
                }
                SqlCommand updateLabsStatus = new SqlCommand("update labs set status='Provisioning' where id = " + labID);
                updateLabsStatus.ExecuteNonQuery();
            }
            conn.Close();
        }

        async private Task<bool> CreateVM(string serviceName, string vmName, string password, string Machine_Size, string OS)
        {
            VMManager vmm = new VMManager("195686de-146a-4f9a-96c5-cd4071185af8", "AADE7A7D7E4992425FF0E882A621D33B3AD160D5");

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