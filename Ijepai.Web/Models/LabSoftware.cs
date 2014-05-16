using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ijepai.Web.Models
{
    public class LabSoftware
    {
        public int ID { get; set; }
        public int Software_ID { get; set; }
        [Display(Name = "Software")]
        public string Software_Path { get; set; }

        public int Lab_ID { get; set; }
        public Lab Lab { get; set; }
    }
}