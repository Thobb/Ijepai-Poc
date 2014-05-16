using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Ijepai.Web.Models
{
    public class Lab
    {
        public Lab()
        {
            this.LabParticipants = new HashSet<LabParticipant>();
            this.PredefinedLabSoftwares = new HashSet<LabSoftwarePredefined>();
            this.CustomLabSoftwares = new HashSet<LabSoftwareCustom>();
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string Time_Zone { get; set; }
        public System.DateTime Start_Time { get; set; }
        public System.DateTime End_Time { get; set; }
        public string Status { get; set; }
        public string HKey { get; set; }

        public string ApplicationUserID { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ICollection<LabParticipant> LabParticipants { get; set; }
        public virtual ICollection<LabSoftwarePredefined> PredefinedLabSoftwares { get; set; }
        public virtual ICollection<LabSoftwareCustom> CustomLabSoftwares { get; set; }
        public int LabConfigurationID { get; set; }
        public virtual LabConfiguration LabConfig { get; set; }
        public virtual ICollection<Billing> Bills { get; set; }
    }
}