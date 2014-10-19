using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ijepai.Web.Models
{
    public class DashBoardModel
    {
        [Required]
        [Display(Name = "Machine Size")]
        public string Machine_Size { get; set; }

        [Required]
        [Display(Name = "Configuration")]
        public string OS { get; set; }        
    }
}