using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ijepai.Web.Models
{
    public class LabSoftwareCustom
    {
        public int ID { get; set; }
        [DataType(DataType.Upload)]
        [Display(Name = "Upload a software")]
        public string Software_Path { get; set; }

        public int LabID { get; set; }
        public Lab Lab { get; set; }
    }
}