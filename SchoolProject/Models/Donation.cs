using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class Donation
    {
        public int Id { get; set; }

        public virtual Donor Donor { get; set; }

        public Medicine Medicine { get; set; }

        [Display(Name = "Receiver Ngo")]
        public virtual Ngo ReceiverNgo { get; set; }

        [Display(Name ="Donation Time")]
        public DateTime DonationTime { get; set; }

        public int Quantity { get; set; }

        [Display(Name = "Status")]
        public bool Completed { get; set; } = false;
    }
}
