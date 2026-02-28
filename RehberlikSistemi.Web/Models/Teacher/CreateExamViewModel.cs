using System;
using System.ComponentModel.DataAnnotations;

namespace RehberlikSistemi.Web.Models.Teacher
{
    public class CreateExamViewModel
    {
        public int ProfileId { get; set; }
        
        [Required(ErrorMessage = "Ders seçimi zorunludur.")]
        [Display(Name = "Ders")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Sınav tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Sınav Tarihi")]
        public DateTime ExamDate { get; set; } = DateTime.Today.AddDays(7);

        [Range(1, 5)]
        [Display(Name = "Önem Seviyesi (1-5)")]
        public int ImportanceLevel { get; set; } = 3;
    }
}
