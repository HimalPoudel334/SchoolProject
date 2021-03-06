using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class Complaint
    {
        public int Id { get; set; }

        public Donor By { get; set; }

        public DateTime On { get; set; } = DateTime.Today;
        
        [Required]
        [Display(Name = "Complaint")]
        public string UserComplaint { get; set; }
        
        [Required]
        public string Suggestion { get; set; }
    }
}
