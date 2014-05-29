using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ijepai.Web.Controllers.Admin
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetView()
        {
            return View("~/Views/Admin/Index.cshtml");
        }
	}
}