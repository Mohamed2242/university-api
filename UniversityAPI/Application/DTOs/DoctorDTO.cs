using UniversityAPI.Core.Models;

namespace UniversityAPI.Application.DTOs
{
    public class DoctorDTO
	{
        public string Name { get; set; }
        public string Email { get; set; }
		public string BackupEmail { get; set; }
		public string Role { get; set; }
		public string Faculty { get; set; }
		public ICollection<Course> Courses { get; set; } = new List<Course>();

	}
}
