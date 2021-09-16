using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolProject.Models
{
    public class Request 
    {
        public int Id { get; set; }

        public Medicine Medicine { get; set; }

        public virtual Donor Requestor { get; set; }

        [Display(Name = "Requested To")]
        public virtual Ngo RequestingNgo { get; set; }

        public int Quantity { get; set; }

        [Display(Name = "Request Date")]
        public DateTime RequestDate { get; set; }

        [Display(Name = "Status")]
        public bool Completed { get; set; } = false;
    }
}