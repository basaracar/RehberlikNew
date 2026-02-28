using System.ComponentModel.DataAnnotations;

namespace RehberlikSistemi.Web.Models.Teacher
{
    public class TeacherEditStudentViewModel
    {
        public int ProfileId { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-Posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-Posta")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Sınıf")]
        public string? GradeLevel { get; set; }

        [Display(Name = "Hedef Bölüm")]
        public string? TargetUniversity { get; set; }
    }
}
