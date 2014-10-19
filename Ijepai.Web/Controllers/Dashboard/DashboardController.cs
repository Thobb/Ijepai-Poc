using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ijepai.Web.Models;
using SMLibrary;
using System.Configuration;
using System.Xml.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading.Tasks;

namespace Ijepai.Web.Controllers.Dashboard
{
    public class DashboardController : Controller
    {
        static string guid = Guid.NewGuid().ToString();
        string serviceName = "Ijepai" + guid;
        string vmName = "VM1";
        string password = "1234Test!";

        
        [HttpPost]
        // GET: /Dashboard/
        public ActionResult Index()
        {       
            return PartialView("_DashboardPartial");
        }

        public async Task<JsonResult> Getimages()
        {
            VMManager vmm = new VMManager(ConfigurationManager.AppSettings["SubcriptionID"], ConfigurationManager.AppSettings["CertificateThumbprint"]);
            var swr = new StringWriter();
            List<string> imageList = await vmm.GetAzureVMImages();
            TempData["OS"] = new SelectList(imageList);

            return Json(new { Status = 0, MessageTitle = "Success" });
        }

        // POST: /Dashboard/Create
        [HttpPost]
        public ActionResult QuickCreate(DashBoardModel model)
        {        
            GenerateVMConfig(model);
            return View();
        }

        public async Task<JsonResult> GetVMStatus()
        {
            VMManager vmm = GetVMM();
            String requestID = Request.QueryString["requestid"];
            XElement status = await vmm.GetOperationStatus(requestID);
            String strStatus = status.Element(vmm.ns + "Status").Value;
            String strMessage = String.Empty;
            if (status.Descendants(vmm.ns + "Message").FirstOrDefault() != null)
                strMessage = status.Descendants(vmm.ns + "Message").FirstOrDefault().Value;

            String osStatus = String.Format("Status: {0}, Message: {1}", strStatus, strMessage);
            return Json(new { Status = osStatus, MessageTitle = "Success" });

        }


        public ActionResult GetView()
        {
            Task<JsonResult> result = Getimages();            
            return PartialView("_DashboardPartial");
        }

        async public Task<OperationStatus> GenerateVMConfig(DashBoardModel model)
        {
                      
           
            //lblStatus.Text = "";

            VMManager vmm = GetVMM();

            if (await vmm.IsServiceNameAvailable(serviceName).ConfigureAwait(continueOnCapturedContext:false) == false)
            {
                //lblStatus.Text = "Service Name is not available. Must be unique";
                return OperationStatus.Failed;
            }
            

            XDocument vm = vmm.NewAzureVMConfig(vmName,model.Machine_Size , model.OS, GetOSDiskMediaLocation(), true);

            vm = vmm.NewWindowsProvisioningConfig(vm, vmName, password);
            vm = vmm.NewNetworkConfigurationSet(vm);

            //if (chkAddDataDisk.Checked == true)
            //{
            //    vm = vmm.AddNewDataDisk(vm, 10, 0, "MyDataDisk", GetDataDiskMediaLocation());
            //}
            //if (chkAddHTTPEndpoint.Checked == true)
            //{
                vm = vmm.AddNewInputEndpoint(vm, "web", "TCP", 80, 80);
            //}
            //if (chkAddRDPEndpoint.Checked == true)
            //{
                vm = vmm.AddNewInputEndpoint(vm, "rdp", "TCP", 3389, 3389);
            //}

            var builder = new StringBuilder();
            var settings = new XmlWriterSettings()
            {
                Indent = true
            };
            using (var writer = XmlWriter.Create(builder, settings))
            {
                vm.WriteTo(writer);
            }

            
            //lblVMXML.Text = Server.HtmlEncode(builder.ToString());




            String requestID_cloudService = await vmm.NewAzureCloudService(serviceName, "West US", String.Empty).ConfigureAwait(continueOnCapturedContext: false);

            OperationResult result = await vmm.PollGetOperationStatus(requestID_cloudService, 5, 120).ConfigureAwait(continueOnCapturedContext: false); ;

                if (result.Status == OperationStatus.Succeeded)
                {
                    // VM creation takes too long so we'll check it later
                    String requestID_createDeployment = await vmm.NewAzureVMDeployment(serviceName, vmName, String.Empty, vm, null).ConfigureAwait(continueOnCapturedContext:false);

                   // Response.Redirect("/GetOperationStatus.aspx?requestid=" + requestID_createDeployment);
                }
                else
                {
                    //lblStatus.Text = String.Format("Creating Cloud Service Failed. Message: {0}", result.Message);
                }
                return result.Status;
        }

        private String GetOSDiskMediaLocation()
        {
            String osdiskmedialocation = String.Format("https://{0}.blob.core.windows.net/vhds/{1}-OS-{2}.vhd", "Ijepai", vmName, Guid.NewGuid().ToString());
            return osdiskmedialocation;
        }

        private VMManager GetVMM()
        {
            return new VMManager(ConfigurationManager.AppSettings["SubcriptionID"], ConfigurationManager.AppSettings["CertificateThumbprint"]);
        }
	}
}