using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ijepai.Web.Models
{
    public class QuickCreateFiles
    {
        public int ID;
        public string Path { get; set; }

        public int LabID { get; set; }
        public virtual Lab Lab { get; set; }
    }
}