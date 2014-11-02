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
        public Task<JsonResult> QuickCreate(QuickCreateModel model)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var status = GenerateVMConfig(model);
            return status;
        }

        public async Task<JsonResult> GetVMStatus(string ServiceName, string VMName)
        {
            VMManager vmm = GetVMM();
            XDocument vmXML = await vmm.GetAzureVM(ServiceName, VMName);
            return Json(new {Status=0, InstanceStatus = (string)vmXML.Element(vmm.ns + "InstanceStatus"), PowerState = (string)vmXML.Element(vmm.ns + "PowerState")});
        }


        public ActionResult GetView()
        {
            Task<JsonResult> result = Getimages();            
            return PartialView("_DashboardPartial");
        }

        async public Task<JsonResult> GenerateVMConfig(QuickCreateModel model)
        {
            VMManager vmm = GetVMM();

            if (await vmm.IsServiceNameAvailable(serviceName).ConfigureAwait(continueOnCapturedContext:false) == false)
            {
                return Json(new { Status=OperationStatus.Failed});
            }
            

            XDocument vm = vmm.NewAzureVMConfig(vmName,model.Machine_Size , model.OS, GetOSDiskMediaLocation(), true);

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
                    requestID_createDeployment = await vmm.NewAzureVMDeployment(serviceName, vmName, String.Empty, vm, null).ConfigureAwait(continueOnCapturedContext:false);
                }
                else
                {
                    requestID_createDeployment = "";
                }

                return Json(new { Status = result.Status, ServiceName = serviceName , VMName = vmName});
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