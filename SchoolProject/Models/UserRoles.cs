using System.ComponentModel.DataAnnotations;

namespace SchoolProject.Models
{
    public class UserRoles
    {
        [Required]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; }
    }
}
