using RehberlikSistemi.Web.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RehberlikSistemi.Web.Models.Student
{
    public class StudentAvailabilityViewModel
    {
        public List<Availability> Availabilities { get; set; } = new List<Availability>();
        
        [Display(Name = "Gün")]
        public DayOfWeek NewDayOfWeek { get; set; }
        
        [Display(Name = "Başlangıç Saati")]
        [Required]
        public TimeSpan NewStartTime { get; set; }
        
        [Display(Name = "Bitiş Saati")]
        [Required]
        public TimeSpan NewEndTime { get; set; }
    }
}
