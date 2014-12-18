using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ijepai.Web.Models
{
    public class LabConfiguration
    {
        public Nullable<int> VM_Count { get; set; }
        public string VM_Type { get; set; }
        public double Hard_Disk { get; set; }
        public string Machine_Size { get; set; }
        public string OS { get; set; }
        public string Networked { get; set; }

        [Key]
        [ForeignKey("Lab")]
        public int LabID { get; set; }
        public virtual Lab Lab { get; set; }
    }
}