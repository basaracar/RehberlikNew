using RehberlikSistemi.Web.Core.Entities;
using System;
using System.Collections.Generic;

namespace RehberlikSistemi.Web.Models.Student
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        
        // Bu haftanın başlangıç (Pazartesi) ve bitiş (Pazar) tarihleri
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        
        public List<StudyTask> CurrentWeekTasks { get; set; } = new List<StudyTask>();
        
        // Öğrenci motive olsun diye haftalık ilerleme yüzdesi filan da eklenebilir
        public int CompletedTasksCount { get; set; }
        public int TotalTasksCount { get; set; }
    }
}
