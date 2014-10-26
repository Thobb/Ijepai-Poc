using Ijepai.Web.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using SMLibrary;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using IjepaiMailer;

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
            var labData = db.Labs.ToList().Select(l => new {
                l.ID,
                l.Name,
                l.Time_Zone,
                Start_Time = l.Start_Time.ToString("dd'/'MMM'/'yyyy HH:mm"),
                End_Time = l.End_Time.ToString("dd'/'MMM'/'yyyy HH:mm"),
                l.Status,
                VM_Count = (l.LabConfig == null)? 0 :l.LabConfig.VM_Count,
                Participant_Count = (l.LabParticipants != null)? l.LabParticipants.Count() : 0,
                Networked = (l.LabConfig == null)? "" : l.LabConfig.Networked,
                OS = (l.LabConfig == null)? "" : l.LabConfig.OS,
                VM_Type = (l.LabConfig == null)? "" : l.LabConfig.VM_Type,
                Hard_Disk = (l.LabConfig == null)? 0 : l.LabConfig.Hard_Disk
            });
            
            return Json(new { Status = 0, TotalItems = labData.Count(), rows = labData });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public JsonResult LabList()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var labData = db.Labs.Select(l => new { l.ID, l.Name }).ToList();
            return Json(new { Status = 0, TotalItems = labData.Count(), rows = labData });
        }

        [Authorize]
        public ActionResult Subdomain(string subdomain)
        {
            return Content(subdomain);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public JsonResult GetLabParticipants(int ID)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var participants = db.Labs.Where(l => l.ID == ID).ToList().Select(l => l.LabParticipants.Select(p => new {
                p.ID,
                Email_Address = p.Email_Address ?? "",
                First_Name = p.First_Name ?? "",
                Last_Name = p.Last_Name ?? "",
                Role = p.Role ?? ""
            }));
            return Json(new { Status = 0, TotalItems = participants.Count(), rows = participants, org = Session["org"] });
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
                if (Lab_ID != 0)
                {
                    newLab = db.Labs.Where(l => l.ID == Lab_ID).FirstOrDefault();
                }
                newLab.ApplicationUserID = User.Identity.GetUserId();
                newLab.Name = newLabData.Name;
                newLab.Status = "Scheduled";
                newLab.Time_Zone = newLabData.Time_Zone;
                newLab.Start_Time = newLabData.Start_Time;
                newLab.End_Time = newLabData.End_Time;
                if (Lab_ID == 0)
                {
                    db.Labs.Add(newLab);
                }
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
                    EditParticipants(newLabData.LabParticipants, Lab_ID);
                }
            }
            catch (Exception ex)
            {
                string message = ex.ToString();
            }
            result = Json(new { Status = 0, ModelState = a, Lab = "lab-" + Lab_ID });
            return result;
        }

        protected int ConfigureLab(LabCreate newLabData, int Lab_ID)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            LabConfiguration newLabConfig = new LabConfiguration();
            LabConfiguration config = db.LabConfiguration.Where(c => c.LabID == Lab_ID).FirstOrDefault();
            if (config != null)
            {
                newLabConfig = config;
            }
            else
            {
                newLabConfig.LabID = Lab_ID;
            }
            newLabConfig.Networked = newLabData.Networked;
            newLabConfig.OS = newLabData.OS;
            newLabConfig.VM_Count = newLabData.VM_Count;
            newLabConfig.Hard_Disk = newLabData.Machine_Size;
            if (config == null)
            {
                db.LabConfiguration.Add(newLabConfig);
            }
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
                    }
                }
            }
            db.SaveChanges();
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

        public JsonResult RescheduleLab(int Lab_ID, DateTime Start_Time, DateTime End_Time)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            Lab lab = db.Labs.Where(l => l.ID == Lab_ID).FirstOrDefault();
            lab.Start_Time = Start_Time;
            lab.End_Time = End_Time;
            db.SaveChanges();
            return Json(new { Status = 0 });
        }

        public JsonResult EditLab(LabCreate newLabData, int Lab_ID)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var lab = db.Labs.Where(l => l.ID == Lab_ID).FirstOrDefault();
            lab.Name = newLabData.Name;
            lab.Start_Time = newLabData.Start_Time;
            lab.End_Time = newLabData.End_Time;
            try
            {
                EditParticipants(newLabData.LabParticipants, Lab_ID);
                ConfigureLab(newLabData, Lab_ID);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                string msg = ex.ToString();
            }
            return Json(new { Status = 0 });
        }

        public JsonResult DeleteLab(int Lab_ID)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var lab = db.Labs.Where(l => l.ID == Lab_ID).FirstOrDefault();
            db.Labs.Remove(lab);
            db.LabParticipants.Where(p => p.LabID == Lab_ID).ToList().ForEach(p => db.LabParticipants.Remove(p));
            db.LabConfiguration.Where(c => c.LabID == Lab_ID).ToList().ForEach(c => db.LabConfiguration.Remove(c));

            db.SaveChanges();
            return Json(new { Status = 0 });
        }

        public JsonResult EditParticipantParticulars(Participant participantData, int Participant_ID)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var participant = db.LabParticipants.Where(p => p.LabID == participantData.Lab_Id && p.ID == Participant_ID).FirstOrDefault();
            participant.Last_Name = participantData.Last_Name;
            participant.First_Name = participantData.First_Name;
            participant.Role = participantData.Role;
            participant.Email_Address = participantData.Username;
            db.SaveChanges();
            return Json(new { Status = 0, Lab = "lab-" + participantData.Lab_Id });
        }

        public JsonResult MoveParticipant(int Participant_ID, int Lab_ID, int newLab_ID, string todo)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var participant = db.LabParticipants.Where(p => p.ID == Participant_ID && p.LabID == Lab_ID).FirstOrDefault();
            LabParticipant participantCp = participant;
            participant.LabID = newLab_ID;
            if (todo == "Move")
            {
                db.LabParticipants.Remove(participant);
                db.SaveChanges();
            }
            db.LabParticipants.Add(participantCp);
            db.SaveChanges();
            return Json(new { Status = 0, prevLab = "lab-" + Lab_ID, newLab = "lab-" + newLab_ID });
        }

        public JsonResult GetMachineLink(int Participant_ID, int Lab_ID)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            LabParticipant participant = db.LabParticipants.Where(p => p.ID == Participant_ID && p.LabID == Lab_ID).FirstOrDefault();
            Lab lab = db.Labs.Where(l => l.ID == Lab_ID).FirstOrDefault();
            string UserName = User.Identity.Name;
            string MachineLink = "http://ijepai.azurewebsites.net/" + UserName + "/" + lab.Name + "/" + participant.Email_Address.Replace("@", "_");
            return Json(new { Status = 0, Message = MachineLink });
        }

        public JsonResult SendMachineLink(int Participant_ID, int Lab_ID)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            LabParticipant participant = db.LabParticipants.Where(p => p.ID == Participant_ID && p.LabID == Lab_ID).FirstOrDefault();
            Lab lab = db.Labs.Where(l => l.ID == Lab_ID).FirstOrDefault();
            string UserName = User.Identity.Name;
            string link = "http://ijepai.azurewebsites.net/" + UserName + "/" + lab.Name + "/" + participant.Email_Address.Replace("@", "_");
            Mailer mail = new Mailer("rahulkarn@gmail.com", "Ijepai");
            mail.Compose(link, participant.Email_Address);
            mail.SendMail();
            return Json(new { Status = 0});
        }

        public JsonResult StartMachine(int Participant_ID, int Lab_ID)
        {
            return Json(new { Status = 0 });
        }

        public JsonResult StopMachine(int Participant_ID, int Lab_ID)
        {
            return Json(new { Status = 0 });
        }

        public JsonResult DeleteParticipant(int Participant_ID, int Lab_ID)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var participant = db.LabParticipants.Where(p => p.ID == Participant_ID && p.LabID == Lab_ID).FirstOrDefault();
            db.LabParticipants.Remove(participant);
            db.SaveChanges();
            return Json(new { Status = 0, Lab = "lab-" + Lab_ID });
        }

        public ActionResult GetView()
        {
            return PartialView("_LabsPartial");
        }
    }
}