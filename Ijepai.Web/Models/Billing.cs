using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ijepai.Web.Models
{
    public class Billing
    {
        public int ID { get; set; }
        public string Item { get; set; }
        public decimal Price { get; set; }

        public int LabID { get; set; }
        public virtual Lab Lab { get; set; }
    }
}