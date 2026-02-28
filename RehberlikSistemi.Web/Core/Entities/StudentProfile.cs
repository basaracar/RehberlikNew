using System.Collections.Generic;

namespace RehberlikSistemi.Web.Core.Entities
{
    public class StudentProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public string? TeacherId { get; set; }
        public ApplicationUser? Teacher { get; set; }

        public string? GradeLevel { get; set; }
        public string? TargetUniversity { get; set; }

        // Navigation Properties
        public ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
        public ICollection<Exam> Exams { get; set; } = new List<Exam>();
        public ICollection<WeeklyTarget> WeeklyTargets { get; set; } = new List<WeeklyTarget>();
        public ICollection<StudyTask> StudyTasks { get; set; } = new List<StudyTask>();
    }
}
