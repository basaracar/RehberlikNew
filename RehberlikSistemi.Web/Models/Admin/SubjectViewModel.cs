using System.ComponentModel.DataAnnotations;
using RehberlikSistemi.Web.Core.Enums;

namespace RehberlikSistemi.Web.Models.Admin
{
    public class SubjectViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Ders adı zorunludur.")]
        [Display(Name = "Ders Adı")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ders türü seçilmelidir.")]
        [Display(Name = "Ders Türü")]
        public SubjectType SubjectType { get; set; }
    }
}
