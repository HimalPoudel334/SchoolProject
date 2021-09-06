using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class Medicine
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [Display(Name = "Generic Name")]
        public string GenericName { get; set; }

        [Column(TypeName = "varchar(6)")]
        public string Mg { get; set; }

        public string Description { get; set; }

        [Display(Name = "Expiry Date")]
        public DateTime ExpiryDate { get; set; }

        //overide Equals method
        public override bool Equals(object obj)
        {
            Medicine medicineObj = obj as Medicine;
            if (medicineObj == null)
                return false;
            return Name.Equals(medicineObj.Name) && GenericName.Equals(medicineObj.GenericName) && Mg.Equals(medicineObj.Mg) && Description.Equals(medicineObj.Description) && ExpiryDate.Equals(medicineObj.ExpiryDate);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
