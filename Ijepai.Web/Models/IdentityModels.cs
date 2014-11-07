using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace Ijepai.Web.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            this.Labs = new HashSet<Lab>();
        }

        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Email_Address { get; set; }
        public string Credit_Card_Number { get; set; }

        public virtual ICollection<Lab> Labs { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }
        public DbSet<Lab> Labs { get; set; }
        public DbSet<LabParticipant> LabParticipants { get; set; }
        public DbSet<LabConfiguration> LabConfiguration { get; set; }
        public DbSet<LabSoftwarePredefined> PredefinedLabSoftwares { get; set; }
        public DbSet<LabSoftwareCustom> CustomLabSoftwares { get; set; }
        public DbSet<LabDataDisk> LabDataDisks { get; set; }
        public DbSet<LabFiles> LabFiles { get; set; }
        public DbSet<LabVM> LabVM { get; set; }
        public DbSet<Billing> Bills { get; set; }
        public DbSet<Software> Softwares { get; set; }
        public DbSet<QuickCreateModel> QuickCreates { get; set; }
        public DbSet<QuickCreateSoftwaresPredefined> QuickCreatePredefinedSoftwares { get; set; }
        public DbSet<QuickCreateSoftwaresCustom> QuickCreateCustomSoftwares { get; set; }
        public DbSet<QuickCreateFiles> QuickCreateFiles { get; set; }

        public System.Data.Entity.DbSet<Ijepai.Web.Models.LabCreate> LabCreates { get; set; }
    }
}