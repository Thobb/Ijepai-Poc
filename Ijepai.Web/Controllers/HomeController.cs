using Ijepai.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SMLibrary;
using System.Configuration;

namespace Ijepai.Web.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public async Task<ActionResult> Index()
        {
            VMManager vmm = new VMManager(ConfigurationManager.AppSettings["SubcriptionID"], ConfigurationManager.AppSettings["CertificateThumbprint"]);
            List<string> imageList = await vmm.GetAzureVMImages();
            TempData["OS"] = new SelectList(imageList);
            return View();
        }

        [HttpPost]
        public ActionResult Templates()
        {
            return PartialView("_TemplatesPartial");
        }

    }
}