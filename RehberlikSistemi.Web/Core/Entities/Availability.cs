using System;

namespace RehberlikSistemi.Web.Core.Entities
{
    public class Availability
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public StudentProfile Student { get; set; } = null!;

        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
