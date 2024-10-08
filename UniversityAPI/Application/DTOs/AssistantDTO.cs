using UniversityAPI.Core.Models;

namespace UniversityAPI.Application.DTOs
{
    public class AssistantDTO
	{
        public string Name { get; set; }
        public string Email { get; set; }
		public string BackupEmail { get; set; }
		public string Role { get; set; }
		public string Faculty { get; set; }
		public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

	}
}
