using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ijepai.Web.Models
{
    public class LabCreate
    {
        [Key]
        [Required(ErrorMessage = "A Lab must have a name.")]
        [DataType(DataType.Text, ErrorMessage = "A Lab must have a name.")]
        public string Name { get; set; }

        [Display(Name = "Time Zone")]
        public string Time_Zone { get; set; }

        [Required(ErrorMessage = "Specify starting time of Lab.")]
        [Display(Name = "Start Time")]
        [DataType(DataType.DateTime, ErrorMessage = "Time specified is not in correct format.")]
        public System.DateTime Start_Time { get; set; }

        [Required(ErrorMessage = "Specify end time of Lab.")]
        [Display(Name = "End Time")]
        [DataType(DataType.DateTime, ErrorMessage = "Time specified is not in correct format.")]
        public System.DateTime End_Time { get; set; }

        [Display(Name = "Select Softwares")]
        public string predefinedSoftwares { get; set; }

        [Required(ErrorMessage = "There must be atleast 1 machine in Lab.")]
        [Range(1, 10000, ErrorMessage = "Lab size can only be 1 to 10000")]
        [Display(Name = "Total Machines")]
        public int VM_Count { get; set; }

        public string Networked { get; set; }

        [Required(ErrorMessage = "Ram and processor specs are required.")]
        [Display(Name = "Machine Type")]
        public string VM_Type { get; set; }

        [Display(Name = "Machine Capacity")]
        public double Machine_Size { get; set; }

        [Display(Name = "Operating System")]
        public string OS { get; set; }

        public virtual ICollection<Participant> LabParticipants { get; set; }
    }


    public class Participant
    {
        [Key]
        public int Lab_Id { get; set; }

        [Required]
        [DisplayName("User name")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Username must be a valid Email.")]
        public string Username { get; set; }

        [DisplayName("First name")]
        public string First_Name { get; set; }

        [DisplayName("Last Name")]
        public string Last_Name { get; set; }

        [DisplayName("Role")]
        public string Role { get; set; }
    }

    public class LabResources
    {
        public int Lab_ID { get; set; }

        [Required(ErrorMessage = "A Virtual machine Image is required.")]
        [Display(Name = "Upload VM Image")]
        [DataType(DataType.Upload)]
        public HttpPostedFileBase VM_Image { get; set; }

        [Display(Name = "Upload Software")]
        [DataType(DataType.Upload)]
        public ICollection<HttpPostedFileBase> Softwares { get; set; }

        [Display(Name = "Upload Disk Image")]
        [DataType(DataType.Upload)]
        public ICollection<HttpPostedFileBase> Data_Disks { get; set; }

        [Display(Name = "Upload Files")]
        [DataType(DataType.Upload)]
        public ICollection<HttpPostedFileBase> Files { get; set; }
    }
}