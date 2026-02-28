using RehberlikSistemi.Web.Core.Entities;
using System.Collections.Generic;

namespace RehberlikSistemi.Web.Models.Teacher
{
    public class PlanGeneratorViewModel
    {
        public int ProfileId { get; set; }
        public string StudentFullName { get; set; } = string.Empty;
        public List<StudyTask> ProposedTasks { get; set; } = new List<StudyTask>();
        
        /// <summary>
        /// JSON serialized ProposedTasks for POST body
        /// </summary>
        public string SerializedTasks { get; set; } = string.Empty;
        
        public string Message { get; set; } = string.Empty;
        public bool IsSuccessful { get; set; } = true;
    }
}
