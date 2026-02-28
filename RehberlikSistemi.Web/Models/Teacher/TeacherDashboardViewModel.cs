using System.Collections.Generic;

namespace RehberlikSistemi.Web.Models.Teacher
{
    public class TeacherDashboardViewModel
    {
        public string TeacherFullName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public double AverageCompletionRate { get; set; }
        public int AtRiskStudentsCount { get; set; }
        public int MonthlyGoalCount { get; set; }
        public List<RecentStudentProgressViewModel> RecentStudents { get; set; } = new();
        public List<RecentActivityViewModel> RecentActivities { get; set; } = new();
    }

    public class RecentStudentProgressViewModel
    {
        public int ProfileId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? GradeLevel { get; set; }
        public string? ProfileImageUrl { get; set; }
        public double CompletionRate { get; set; }
        public string Status { get; set; } = string.Empty; // "İyi", "Yavaş", "Riskli"
        public string StatusClass { get; set; } = string.Empty; // "success", "warning", "danger"
    }

    public class RecentActivityViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string ColorClass { get; set; } = string.Empty;
    }
}
