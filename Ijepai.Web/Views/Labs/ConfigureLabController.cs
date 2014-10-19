using SMLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Ijepai.Web.Views.Labs
{
    public class ConfigureLabController : Controller
    {

      
        // GET: ConfigureLab
        public ActionResult Index()
        {
            //ViewData["OS"] = GetOS();        

            return View();
        }

        // GET: ConfigureLab/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ConfigureLab/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ConfigureLab/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: ConfigureLab/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ConfigureLab/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: ConfigureLab/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ConfigureLab/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
