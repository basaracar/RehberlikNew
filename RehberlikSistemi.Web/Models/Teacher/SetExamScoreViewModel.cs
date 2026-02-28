using System.ComponentModel.DataAnnotations;

namespace RehberlikSistemi.Web.Models.Teacher
{
    public class SetExamScoreViewModel
    {
        public int ExamId { get; set; }
        public int ProfileId { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        [Display(Name = "Sınav Puanı")]
        [Range(0, 100, ErrorMessage = "Puan 0 ile 100 arasında olmalıdır.")]
        public int? Score { get; set; }
    }
}
