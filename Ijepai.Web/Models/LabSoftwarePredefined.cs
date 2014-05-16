using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Ijepai.Web.Models
{
    public class LabSoftwarePredefined
    {
        [Key]
        public int ID { get; set; }
        public int SoftwareID { get; set; }
        [Display(Name = "Select a software")]
        public Software Software { get; set; }

        public int LabID { get; set; }
        public Lab Lab { get; set; }
    }
}