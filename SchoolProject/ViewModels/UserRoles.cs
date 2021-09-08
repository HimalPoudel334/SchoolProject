using System.ComponentModel.DataAnnotations;

namespace SchoolProject.ViewModels
{
    public class UserRoles
    {
        [Required]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; }
    }
}
