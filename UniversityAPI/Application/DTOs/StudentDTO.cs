using UniversityAPI.Core.Models;

namespace UniversityAPI.Application.DTOs
{
    public class StudentDTO
	{
        public string StudentId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string BackupEmail { get; set; }
        public string Role { get; set; }
        public string Faculty { get; set; }
        public int CurrentSemester { get; set; }
		public string Department { get; set; }
		public int TotalCreditHours { get; set; }
        public bool HasRegisteredCourses { get; set; }
    }
}
