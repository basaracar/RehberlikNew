using System.Collections.Generic;
using RehberlikSistemi.Web.Core.Enums;

namespace RehberlikSistemi.Web.Core.Entities
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public SubjectType SubjectType { get; set; }
        
        public ICollection<Exam> Exams { get; set; } = new List<Exam>();
        public ICollection<WeeklyTarget> WeeklyTargets { get; set; } = new List<WeeklyTarget>();
        public ICollection<StudyTask> StudyTasks { get; set; } = new List<StudyTask>();
    }
}
