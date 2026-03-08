using System;
using System.ComponentModel.DataAnnotations;
using RehberlikSistemi.Web.Extensions;

namespace RehberlikSistemi.Web.Models.Teacher
{
    public class CreateWeeklyTargetViewModel
    {
        public int ProfileId { get; set; }
        
        [Required(ErrorMessage = "Ders seçimi zorunludur.")]
        [Display(Name = "Ders")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Hedef saat zorunludur.")]
        [Range(1, 100, ErrorMessage = "Hedef saat 1-100 arasında olmalıdır.")]
        [Display(Name = "Haftalık Hedef Saat")]
        public int TargetHours { get; set; } = 1;

        [Required(ErrorMessage = "Hafta başlangıcı zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Hafta Başlangıç Tarihi")]
        public DateTime WeekStartDate { get; set; } = DateTime.Today.GetStartOfWeek();
    }
}
