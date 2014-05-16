using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Ijepai.Web.Models
{
    public class LabVM
    {
        [Key, ForeignKey("Lab")]
        public int Lab_ID { get; set; }
        public Lab Lab { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Upload VM Image")]
        public string VM_Path { get; set; }
    }
}