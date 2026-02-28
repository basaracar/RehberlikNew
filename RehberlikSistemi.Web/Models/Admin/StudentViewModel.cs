using System.ComponentModel.DataAnnotations;

namespace RehberlikSistemi.Web.Models.Admin
{
    public class StudentViewModel
    {
        public string? Id { get; set; }
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

        [Display(Name = "Şifre")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Sınıf Seviyesi")]
        public string? GradeLevel { get; set; }

        [Display(Name = "Hedef Üniversite/Bölüm")]
        public string? TargetUniversity { get; set; }

        [Display(Name = "Atanacak Öğretmen")]
        public string? TeacherId { get; set; }
        public string? TeacherName { get; set; }
    }
}
