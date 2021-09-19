using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        public bool Seen { get; set; } = false;

        public IdentityUser User { get; set; }

        public Medicine Medicine { get; set; }

        public Type NotificationType { get; set; }
    }
    public enum Type
    {
        Donation,
        Request
    }

}
