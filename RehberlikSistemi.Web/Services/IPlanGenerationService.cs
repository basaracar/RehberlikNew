using System;
using System.Collections.Generic;
using RehberlikSistemi.Web.Core.Entities;

namespace RehberlikSistemi.Web.Services
{
    public interface IPlanGenerationService
    {
        List<StudyTask> GeneratePlan(StudentProfile student, List<Subject> allSubjects, DateTime currentDate);
    }
}
