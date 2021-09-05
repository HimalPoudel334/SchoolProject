using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        public IdentityUser FeedbackBy { get; set; }

        public string Comment { get; set; }

        public DateTime On { get; set; }
    }
}
