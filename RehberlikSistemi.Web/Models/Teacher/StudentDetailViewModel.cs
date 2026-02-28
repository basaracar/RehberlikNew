using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using RehberlikSistemi.Web.Core.Entities;

namespace RehberlikSistemi.Web.Models.Teacher
{
    public class StudentDetailViewModel
    {
        public int ProfileId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? GradeLevel { get; set; }
        public string? TargetUniversity { get; set; }

        public List<Exam> Exams { get; set; } = new();
        public List<SubjectTargetViewModel> AutomatedTargets { get; set; } = new();
        
        [Obsolete("Artık otomatik hedefler kullanılıyor.")]
        public List<WeeklyTarget> WeeklyTargets { get; set; } = new();
        
        // Merged from CreateStudyTaskViewModel
        [ValidateNever]
        public List<Availability> Availabilities { get; set; } = new();

        [Required(ErrorMessage = "Ders seçimi zorunludur.")]
        [Display(Name = "Ders")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Planlanan Tarih")]
        public DateTime ScheduledDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
        [DataType(DataType.Time)]
        [Display(Name = "Başlangıç Saati")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Bitiş saati zorunludur.")]
        [DataType(DataType.Time)]
        [Display(Name = "Bitiş Saati")]
        public TimeSpan EndTime { get; set; }

        [ValidateNever]
        public DateTime WeekStartDate { get; set; }
        [ValidateNever]
        public DateTime WeekEndDate { get; set; }
        [ValidateNever]
        public string RequestSuccessMessage { get; set; } = string.Empty;
    }

    public class SubjectTargetViewModel
    {
        public string SubjectName { get; set; } = string.Empty;
        public double PlannedHours { get; set; }
        public double CompletedHours { get; set; }
        public int ProgressPercentage => PlannedHours > 0 ? (int)((CompletedHours / PlannedHours) * 100) : 0;
    }
}
