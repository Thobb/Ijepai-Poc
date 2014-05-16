using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ijepai.Web.Controllers.Dashboard
{
    public class DashboardController : Controller
    {
        //
        // GET: /Dashboard/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetView()
        {
            return PartialView("_DashboardPartial");
        }
	}
}