using System;
using System.Collections.Generic;
using System.Linq;
using RehberlikSistemi.Web.Core.Entities;

namespace RehberlikSistemi.Web.Services
{
    public class PlanGenerationService : IPlanGenerationService
    {
        public List<StudyTask> GeneratePlan(StudentProfile student, List<Subject> allSubjects, DateTime currentDate)
        {
            var subjectPriorties = new Dictionary<int, int>();

            // Initialize all with base priority 1
            foreach (var subject in allSubjects)
                subjectPriorties[subject.Id] = 1;

            // Algorithm step: Add points based on exams
            foreach (var exam in student.Exams)
            {
                if (exam.ExamDate >= currentDate)
                {
                    // Upcoming exams
                    var daysUntil = (exam.ExamDate - currentDate).TotalDays;
                    int addedPriority = exam.ImportanceLevel; // Add their importance level

                    if (daysUntil <= 7) addedPriority += 5; // Urgent
                    else if (daysUntil <= 30) addedPriority += 3; // Soon

                    subjectPriorties[exam.SubjectId] += addedPriority;
                }
                else
                {
                    // Past exams
                    if (exam.Score.HasValue)
                    {
                        if (exam.Score.Value < 50) subjectPriorties[exam.SubjectId] += 4; // Needs much work
                        else if (exam.Score.Value < 70) subjectPriorties[exam.SubjectId] += 2; // Needs some work
                    }
                }
            }

            // Create a queue weighted by priority
            var sortedPriorities = subjectPriorties.OrderByDescending(x => x.Value).ToList();

            // Fill a pool where subjects appear 'Priority' times
            var subjectPool = new List<Subject>();
            foreach (var sp in sortedPriorities)
            {
                var subj = allSubjects.First(s => s.Id == sp.Key);
                for (int i = 0; i < sp.Value; i++)
                {
                    subjectPool.Add(subj);
                }
            }

            // Shuffle or just cycle through the pool sequentially (which groups high priority ones first)
            int poolIndex = 0;

            // Generate tasks for the next 7 days
            var proposedTasks = new List<StudyTask>();

            for (int i = 1; i <= 7; i++)
            {
                var targetDay = currentDate.Date.AddDays(i);
                var dayOfWeek = targetDay.DayOfWeek;

                var availabilities = student.Availabilities.Where(a => a.DayOfWeek == dayOfWeek && a.IsAvailable).ToList();

                foreach (var avail in availabilities)
                {
                    // Create task chunks of 1 hour within the availability block
                    var currentStart = avail.StartTime;
                    while (currentStart.Add(System.TimeSpan.FromHours(1)) <= avail.EndTime)
                    {
                        var taskSubject = subjectPool[poolIndex % subjectPool.Count];
                        poolIndex++;

                        proposedTasks.Add(new StudyTask
                        {
                            StudentId = student.Id,
                            SubjectId = taskSubject.Id,
                            Subject = taskSubject,
                            ScheduledDate = targetDay,
                            StartTime = currentStart,
                            EndTime = currentStart.Add(System.TimeSpan.FromHours(1)),
                            Status = Core.Enums.StudyTaskStatus.Pending
                        });

                        currentStart = currentStart.Add(System.TimeSpan.FromHours(1));
                    }
                }
            }

            return proposedTasks;
        }
    }
}
