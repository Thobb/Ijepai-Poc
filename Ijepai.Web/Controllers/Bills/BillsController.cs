using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ijepai.Web.Controllers.Bills
{
    public class BillsController : Controller
    {
        //
        // GET: /Bills/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetView()
        {
            return PartialView("_BillsPartial");
        }
	}
}