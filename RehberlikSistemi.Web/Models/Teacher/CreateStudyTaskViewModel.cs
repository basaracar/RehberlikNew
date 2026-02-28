using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RehberlikSistemi.Web.Core.Entities;

namespace RehberlikSistemi.Web.Models.Teacher
{
    public class CreateStudyTaskViewModel
    {
        public int ProfileId { get; set; }
        
        // Ekranda öğrencinin boş zamanlarını listeleyebilmek için
        public List<Availability> Availabilities { get; set; } = new List<Availability>();
        
        // Ekranda takvimi çizebilmek için haftanın günleri ve o haftadaki mevcut görevler
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public List<StudyTask> ExistingTasks { get; set; } = new List<StudyTask>();
        
        public string RequestSuccessMessage { get; set; } = string.Empty;

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
        public TimeSpan StartTime { get; set; } = new TimeSpan(14, 0, 0);

        [Required(ErrorMessage = "Bitiş saati zorunludur.")]
        [DataType(DataType.Time)]
        [Display(Name = "Bitiş Saati")]
        public TimeSpan EndTime { get; set; } = new TimeSpan(15, 0, 0);
    }
}
