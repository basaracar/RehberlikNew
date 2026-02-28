using System;

namespace RehberlikSistemi.Web.Core.Entities
{
    public class WeeklyTarget
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public StudentProfile Student { get; set; } = null!;

        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        public int TargetHours { get; set; }
        public DateTime WeekStartDate { get; set; }
    }
}
