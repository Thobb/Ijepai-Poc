using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Ijepai.Web.Models
{
    public class QuickCreateModel
    {
        public QuickCreateModel()
        {
            this.PredefinedSoftwares = new HashSet<QuickCreateSoftwaresPredefined>();
            this.CustomSoftwares = new HashSet<QuickCreateSoftwaresCustom>();
        }

        public int ID { get; set; }
        public int Name { get; set; }
        [Required]
        [Display(Name = "Send Link")]
        public bool SendLink { get; set; }
        [Required]
        [Display(Name = "Email Address")]
        public string RecepientEmail { get; set;}
        public string VMPath { get; set;}
        [Required]
        [Display(Name = "Machine Size")]
        public string Machine_Size { get; set; }

        [Required]
        [Display(Name = "Configuration")]
        public string OS { get; set; }   

        public string ApplicationUserID { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ICollection<QuickCreateSoftwaresCustom> CustomSoftwares { get; set; }
        public virtual ICollection<QuickCreateSoftwaresPredefined> PredefinedSoftwares { get; set; }
    }
}