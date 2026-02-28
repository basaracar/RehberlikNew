using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RehberlikSistemi.Web.Core.Entities;

namespace RehberlikSistemi.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<StudentProfile> StudentProfiles { get; set; }
        public DbSet<Availability> Availabilities { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<WeeklyTarget> WeeklyTargets { get; set; }
        public DbSet<StudyTask> StudyTasks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure StudentProfile relationships
            builder.Entity<StudentProfile>()
                .HasOne(sp => sp.User)
                .WithOne()
                .HasForeignKey<StudentProfile>(sp => sp.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentProfile>()
                .HasOne(sp => sp.Teacher)
                .WithMany()
                .HasForeignKey(sp => sp.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);
                
            // When deleting a Subject or Student, related task/exam mappings should be cascaded
            builder.Entity<Exam>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Exams)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Entity<Exam>()
                .HasOne(e => e.Subject)
                .WithMany(s => s.Exams)
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Entity<WeeklyTarget>()
                .HasOne(wt => wt.Subject)
                .WithMany(s => s.WeeklyTargets)
                .HasForeignKey(wt => wt.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Entity<StudyTask>()
                .HasOne(st => st.Subject)
                .WithMany(s => s.StudyTasks)
                .HasForeignKey(st => st.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
