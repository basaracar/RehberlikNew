using System.ComponentModel.DataAnnotations;

namespace RehberlikSistemi.Web.Models.Admin
{
    public class TeacherViewModel
    {
        public string? Id { get; set; }

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

        [Display(Name = "Şifre (sadece yeni eklerken zorunludur)")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
