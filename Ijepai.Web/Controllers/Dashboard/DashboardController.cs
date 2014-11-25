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
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.Management.Automation;


namespace Ijepai.Web.Controllers.Dashboard
{
    public class DashboardController : Controller
    {
        static string guid = Guid.NewGuid().ToString();
        string serviceName = "Ijepai" + guid;
        string vmName = string.Empty;
        string password = "1234Test!";

        private void RunPowershellScript(string scriptFile, string scriptParameters)
        {
            RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();
            Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
            runspace.Open();
            System.Management.Automation.RunspaceInvoke scriptInvoker = new System.Management.Automation.RunspaceInvoke(runspace);
            Pipeline pipeline = runspace.CreatePipeline();
            Command scriptCommand = new Command(scriptFile);
            Collection<CommandParameter> commandParameters = new Collection<CommandParameter>();
            foreach (string scriptParameter in scriptParameters.Split(' '))
            {
                CommandParameter commandParm = new CommandParameter(null, scriptParameter);
                commandParameters.Add(commandParm);
                scriptCommand.Parameters.Add(commandParm);
            }
            pipeline.Commands.Add(scriptCommand);
            Collection<PSObject> psObjects;
            psObjects = pipeline.Invoke();
        }
        
        [HttpPost]
        // GET: /Dashboard/
        public ActionResult Index()
        {
            return PartialView("_DashboardPartial");
        }

        public static Dictionary<string, string> imageList {get; set;}
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

        string label { get; set; }
        public async Task<string> GetVMLabel(string imgName)
        {
            string Label;
            VMManager vmm = new VMManager(ConfigurationManager.AppSettings["SubcriptionID"], ConfigurationManager.AppSettings["CertificateThumbprint"]);
            imageList = await vmm.GetAzureVMImages();
            imageList.TryGetValue(imgName, out Label);
            label = Label;
            return label;

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public JsonResult GetQC()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var QCVM = db.QuickCreates.Select(l => new { l.ID, l.Name, l.Machine_Size, l.OSLabel, l.RecepientEmail }).ToList();
            return Json(new { Status = 0, TotalItems = QCVM.Count(), rows = QCVM, org = Session["org"] });
        }

        // POST: /Dashboard/Create
        [HttpPost]
        async public Task<JsonResult> QuickCreate(QuickCreateModel model)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            model.ApplicationUserID = User.Identity.GetUserId();
            var user = db.Users.Where(u => u.Id == model.ApplicationUserID).FirstOrDefault();
            model.RecepientEmail = model.RecepientEmail ?? user.Email_Address;
            vmName = model.Name;
            var status = GenerateVMConfig(model);
            await GetVMLabel(model.OS);
            model.OSLabel = label;
            model.ServiceName = serviceName;
            db.QuickCreates.Add(model);
            db.SaveChanges();
            Mailer mail = new Mailer("rahulkarn@gmail.com", "Ijepai");
            string link = "http://vmengine.azurewebsites.net/?" + serviceName + ".cloudapp.net" + "/" + "administrator" + "/" + password;
            mail.Compose(link, model.RecepientEmail);
            mail.SendMail();
            return Json(new { Status = 0 });
        }


        private static void ApplyNamespace(XElement parent, XNamespace nameSpace)
        {
            parent.Name = nameSpace + parent.Name.LocalName;
            foreach (XElement child in parent.Elements())
            {
                ApplyNamespace(child, nameSpace);
            }
        }
        async public Task <JsonResult> DeleteQCVM(int id)
        {           
            VMManager vmm = new VMManager(ConfigurationManager.AppSettings["SubcriptionID"], ConfigurationManager.AppSettings["CertificateThumbprint"]);
            ApplicationDbContext db = new ApplicationDbContext();
            var cloudService = db.QuickCreates.Where(l => l.ID == id ).FirstOrDefault();
            await vmm.DeleteVM(cloudService.ServiceName);
            db.QuickCreates.Remove(cloudService);
            db.SaveChanges();
            return Json(new { Status = 0 });
        }

        async public Task<JsonResult> CaptureQCVM(int id)
        {
            VMManager vmm = new VMManager(ConfigurationManager.AppSettings["SubcriptionID"], ConfigurationManager.AppSettings["CertificateThumbprint"]);
            ApplicationDbContext db = new ApplicationDbContext();
            var cloudService = db.QuickCreates.Where(l => l.ID == id).FirstOrDefault();
            await vmm.ShutDownVM(cloudService.ServiceName, cloudService.Name);
            VirtualMachineCaptureVMImageParameters param = new VirtualMachineCaptureVMImageParameters();
            param.VMImageLabel = "TestImage";
            param.VMImageName = "TestImage";
            param.OSState = "Specialized";
            System.Threading.CancellationToken token = new System.Threading.CancellationToken(false);
            await vmm.CaptureVM(cloudService.ServiceName, cloudService.Name, param.VMImageName);
           // await vmm.BeginCapturingVMImageAsync(cloudService.ServiceName, cloudService.ServiceName, cloudService.Name, param, token);
            await vmm.RebootVM(cloudService.ServiceName, cloudService.Name);
            return Json(new { Status = 0 });
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