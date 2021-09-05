using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class Ngo : IdentityUser
    {
        [Display(Name = "Registration Number")]
        [PersonalData]
        [Column(TypeName = "nvarchar(50)")]
        public string RegistrationNumber { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(50)")]
        public string Name { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(50)")]
        [Display(Name = "Director Name")]
        public string DirectorName { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Address { get; set; }

        public virtual ICollection<Request> MedicineRequests { get; set; }

        public virtual ICollection<Donation> MedicineDonatios { get; set; }

    }
}
