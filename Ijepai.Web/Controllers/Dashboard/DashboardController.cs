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
using Microsoft.AspNet.Identity;
using IjepaiMailer;

namespace Ijepai.Web.Controllers.Dashboard
{
    public class DashboardController : Controller
    {
        static string guid = Guid.NewGuid().ToString();
        string serviceName = "Ijepai"+guid;
        string vmName = "VM1";
        string password = "1234Test!";

        
        [HttpPost]
        // GET: /Dashboard/
        public ActionResult Index()
        {
            return PartialView("_DashboardPartial");
        }

        Dictionary<string, string> imageList = new Dictionary<string,string>();
        public async Task<JsonResult> Getimages()
        {
            VMManager vmm = new VMManager(ConfigurationManager.AppSettings["SubcriptionID"], ConfigurationManager.AppSettings["CertificateThumbprint"]);
            var swr = new StringWriter();
            imageList = await vmm.GetAzureVMImages();
            List<string> imageListRest = new List<string>();
            //imageList.TryGetValue("")
            //imageListRest.Add(;
            //imageListRest.Add(imageList[185]);
            //imageListRest.Add(imageList[186]);
            //imageListRest.Add(imageList[187]);
            var imgLst = new List<SelectListItem>();
            foreach (KeyValuePair<string, string> entry in imageList)
            {
                imgLst.Add(new SelectListItem{ Value =entry.Key, Text =entry.Value});
            }

            TempData["OS"] = imgLst;

            return Json(new { Status = 0, MessageTitle = "Success" });
        }

        // POST: /Dashboard/Create
        [HttpPost]
        public Task<JsonResult> QuickCreate(QuickCreateModel model)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            model.ApplicationUserID = User.Identity.GetUserId();
            var user = db.Users.Where(u => u.Id == model.ApplicationUserID).FirstOrDefault();
            model.RecepientEmail = model.RecepientEmail ?? user.Email_Address;
            db.QuickCreates.Add(model);
            db.SaveChanges();
            var status = GenerateVMConfig(model);
            if (model.SendLink)
            {
                Mailer mail = new Mailer("rahulkarn@gmail.com", "Ijepai");
                string link = "http://vmengine.azurewebsites.net/?" + "QC" + "/" + model.RecepientEmail.Replace("@", "_");
                mail.Compose(link, model.RecepientEmail);
                mail.SendMail();
            }

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