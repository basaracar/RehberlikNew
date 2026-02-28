using System;
using RehberlikSistemi.Web.Core.Enums;

namespace RehberlikSistemi.Web.Core.Entities
{
    public class StudyTask
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public StudentProfile Student { get; set; } = null!;

        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        public DateTime ScheduledDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        
        public StudyTaskStatus Status { get; set; } = StudyTaskStatus.Pending;
        public int? CompletedDurationMinutes { get; set; }
    }
}
