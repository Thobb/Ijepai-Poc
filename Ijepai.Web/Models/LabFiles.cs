using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Ijepai.Web.Models
{
    public class LabFiles
    {
        public int ID { get; set; }
        [DataType(DataType.Upload)]
        [Display(Name = "Upload a File")]
        public string File_Path { get; set; }

        public int LabID { get; set; }
        public virtual Lab Lab { get; set; }
    }
}