using Ijepai.Web.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace Ijepai.Web.Controllers.Labs
{
    public class LabsController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public JsonResult Index()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var labData = db.Labs.ToList().Select(l => new { l.ID, l.Name, l.Time_Zone, Start_Time = l.Start_Time.ToString("dd-MMM-yyyy HH:MM"), End_Time = l.End_Time.ToString("dd-MMM-yyyy HH:MM"), l.Status, l.LabConfig.VM_Count });
            return Json(new { Status = 0, TotalItems = labData.Count(), rows = labData });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public JsonResult GetLabParticipants(int ID)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var participants = db.Labs.Where(l => l.ID == ID).ToList().Select(l => l.LabParticipants.Select(p => new { p.ID, Email_Address = p.Email_Address ?? "", First_Name = p.First_Name ?? "", Last_Name = p.Last_Name ?? "", Role = p.Role ?? "" }));
            return Json(new { Status = 0, TotalItems = participants.Count(), rows = participants });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public JsonResult CreateLab(LabCreate newLabData, int Lab_ID)
        {
            var a = ModelState.IsValid;
            JsonResult result = new JsonResult();
            ApplicationDbContext db = new ApplicationDbContext();
            if (newLabData.Name != null)
            {
                Lab newLab = new Lab();
                newLab.ApplicationUserID = User.Identity.GetUserId();
                newLab.Name = newLabData.Name;
                newLab.Status = "Scheduled";
                newLab.Start_Time = newLabData.Start_Time;
                newLab.End_Time = newLabData.End_Time;
                db.Labs.Add(newLab);
                db.SaveChanges();
                Lab_ID = newLab.ID;
            }
            try
            {
                if (Lab_ID != 0)
                {
                    if (newLabData.Networked != null)
                    {
                        ConfigureLab(newLabData, Lab_ID);
                    }
                    if (newLabData.LabParticipants.Count() != 0)
                    {
                        EditParticipants(newLabData.LabParticipants, Lab_ID);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.ToString();
            }

            result = Json(new { Status = 0, ModelState = a });
            return result;
        }

        protected int ConfigureLab(LabCreate newLabData, int Lab_ID)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            LabConfiguration newLabConfig = new LabConfiguration();
            newLabConfig.LabID = Lab_ID;
            newLabConfig.Networked = newLabData.Networked;
            newLabConfig.OS = newLabData.OS;
            newLabConfig.VM_Count = newLabData.VM_Count;
            newLabConfig.VM_Type = newLabData.VM_Type;
            newLabConfig.Hard_Disk = newLabData.Machine_Size;
            db.LabConfiguration.Add(newLabConfig);
            db.SaveChanges();
            return 0;
        }

        protected int EditParticipants(ICollection<Participant> Participants, int Lab_ID)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            db.LabParticipants.Where(p => p.LabID == Lab_ID).ToList().ForEach(p => db.LabParticipants.Remove(p));
            if (Participants != null)
            {
                foreach (Participant participant in Participants)
                {
                    if (participant.Username != null)
                    {
                        LabParticipant labParticipant = new LabParticipant();
                        labParticipant.Email_Address = participant.Username.Trim();
                        if (participant.First_Name != null) labParticipant.First_Name = participant.First_Name.Trim();
                        if (participant.Last_Name != null) labParticipant.Last_Name = participant.Last_Name.Trim();
                        labParticipant.Role = participant.Role.Trim();
                        labParticipant.LabID = Lab_ID;
                        db.LabParticipants.Add(labParticipant);
                        db.SaveChanges();
                    }
                }
            }
            return 0;
        }

        public JsonResult UploadLabResources(HttpPostedFileBase dataFile)
        {
            if (!System.IO.Directory.Exists(Server.MapPath("/Lab_Data")))
            {
                System.IO.Directory.CreateDirectory(Server.MapPath("/Lab_Data"));
            }
            if ((dataFile != null) && (dataFile.ContentLength > 0))
            {
                var fileName = System.IO.Path.GetFileName(dataFile.FileName);
                dataFile.SaveAs(System.IO.Path.Combine(Server.MapPath("/Lab_Data"), fileName));
            }
            return Json(new { Status = 0 });
        }

        public ActionResult GetView()
        {
            return PartialView("_LabsPartial");
        }
    }
}