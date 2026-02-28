using System;

namespace RehberlikSistemi.Web.Core.Entities
{
    public class Exam
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public StudentProfile Student { get; set; } = null!;

        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        public DateTime ExamDate { get; set; }
        
        // Importance level 1 (Low) to 5 (High)
        public int ImportanceLevel { get; set; } = 3;

        // Belli olduğunda sınavdan alınan puan girilir (0-100 vb.)
        public int? Score { get; set; }
    }
}
