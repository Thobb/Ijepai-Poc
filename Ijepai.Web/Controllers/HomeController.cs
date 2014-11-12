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
        Dictionary<string, string> imageList = new Dictionary<string, string>();
        [Authorize]
        public async Task<ActionResult> Index()
        {
            VMManager vmm = new VMManager(ConfigurationManager.AppSettings["SubcriptionID"], ConfigurationManager.AppSettings["CertificateThumbprint"]);
            imageList = await vmm.GetAzureVMImages();
            List<string> imageListRest = new List<string>();
            //imageListRest.Add(imageList[184]);
            //imageListRest.Add(imageList[185]);
            //imageListRest.Add(imageList[186]);
            //imageListRest.Add(imageList[187]);
            var imgLst = new List<SelectListItem>();
            foreach (KeyValuePair<string, string> entry in imageList)
            {
                imgLst.Add(new SelectListItem { Value = entry.Key, Text = entry.Value });
            }

            TempData["OS"] = imgLst;

            return View();
        }

        [HttpPost]
        public ActionResult Templates()
        {
            return PartialView("_TemplatesPartial");
        }

    }
}