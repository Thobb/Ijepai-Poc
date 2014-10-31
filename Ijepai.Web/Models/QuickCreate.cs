using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ijepai.Web.Models
{
    public class QuickCreate
    {
        public QuickCreate()
        {
            this.PredefinedSoftwares = new HashSet<QuickCreateSoftwaresPredefined>();
            this.CustomSoftwares = new HashSet<QuickCreateSoftwaresCustom>();
        }

        public int ID;
        public string RecepientEmail;
        public string VMPath;
        public string VM_Type;
        public string VM_Size;

        public string ApplicationUserID { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ICollection<QuickCreateSoftwaresCustom> CustomSoftwares { get; set; }
        public virtual ICollection<QuickCreateSoftwaresPredefined> PredefinedSoftwares { get; set; }
    }
}