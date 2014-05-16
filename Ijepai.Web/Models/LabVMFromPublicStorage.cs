using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ijepai.Web.Models
{
    public class LabVMFromPublicStorage
    {
        public int ID { get; set; }
        [DataType(DataType.Url)]
        [Display(Name = "Path of VM")]
        public string VM_Path { get; set; }
    }
}