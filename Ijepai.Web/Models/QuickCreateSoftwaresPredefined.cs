﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ijepai.Web.Models
{
    public class QuickCreateSoftwaresPredefined
    {
        public int ID { get; set; }

        public int QuickCreateID { get; set; }
        public virtual QuickCreate QuickCreate { get; set; }
    }
}