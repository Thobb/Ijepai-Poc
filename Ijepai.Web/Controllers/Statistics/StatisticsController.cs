using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ijepai.Web.Controllers.Statistics
{
    public class StatisticsController : Controller
    {
        //
        // GET: /Statistics/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetView()
        {
            return PartialView("_StatisticsPartial");
        }
	}
}