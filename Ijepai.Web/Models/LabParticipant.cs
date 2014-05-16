using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ijepai.Web.Models
{
    public class LabParticipant
    {
        public int ID { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email_Address { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Role { get; set; }
        public string VM_ID { get; set; }

        public int LabID { get; set; }
        public Lab Lab { get; set; }
    }
}