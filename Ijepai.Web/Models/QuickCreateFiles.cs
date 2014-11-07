using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ijepai.Web.Models
{
    public class QuickCreateFiles
    {
        public int ID { get; set; }
        public string Path { get; set; }

        public int QuickCreateID { get; set; }
        public virtual QuickCreateModel QuickCreate { get; set; }
    }
}