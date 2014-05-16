using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Ijepai.Web.Models
{
    public class LabDataDisk
    {
        public int ID { get; set; }
        [DataType(DataType.Upload)]
        [Display(Name = "Upload Disk Image")]
        public string DataDisk_Path { get; set; }

        public int LabID { get; set; }
        public Lab Lab { get; set; }
    }
}