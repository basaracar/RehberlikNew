using System.ComponentModel.DataAnnotations;

namespace RehberlikSistemi.Web.Models.Teacher
{
    public class TeacherStudentViewModel
    {
        public int ProfileId { get; set; }
        
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;
        
        [Display(Name = "Sınıf")]
        public string? GradeLevel { get; set; }
        
        [Display(Name = "Hedef Bölüm")]
        public string? TargetUniversity { get; set; }
    }
}
