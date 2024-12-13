using System.ComponentModel.DataAnnotations;

namespace coursesCenter.ViewModels
{
    public class RegisterTraineViewModel
    {
        [UniqueMail]
        [DataType(DataType.EmailAddress)]
        [Required]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }
        [Required]
        public string Name {  get; set; }
        public string Address {  get; set; }
        [Required]
        public int Level {  get; set; }
        [Display(Name ="Department")]
        public int? DepartmentId { get; set; }
        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        [Display(Name ="Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}
