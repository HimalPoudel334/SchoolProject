using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolProject.Models
{    
    public class Donor : IdentityUser
    {
        [Column(TypeName = "nvarchar(50)")]
        [Display(Name="First Name")]
        public string FirstName { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        [Display(Name="Last Name")]
        public string LastName { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Address { get; set; }

        public virtual ICollection<Donation> Donations { get; set; }
        
        public virtual ICollection<Request> Requests { get; set; }

    }
}
