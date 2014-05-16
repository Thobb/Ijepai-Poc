using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ijepai.Web.Models
{
    public class Organization
    {
        public Organization()
        {
            this.ApplicationUsers = new HashSet<ApplicationUser>();
        }

        public int ID { get; set; }
        public string OrganizationName { get; set; }

        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }
    }
}