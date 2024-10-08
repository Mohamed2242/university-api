using UniversityAPI.Core.Models;

namespace UniversityAPI.Application.DTOs
{
    public class CourseDTO
	{
		public string? CourseId { get; set; }
		public string? Name { get; set; }
		public int CreditHours { get; set; }
		public string? Faculty { get; set; }
		public int? Semester { get; set; }
		public bool ContainsPracticalOrProject { get; set; }
		public bool HaveAssistants { get; set; }
		public double? MidTerm { get; set; }
		public double? FinalExam { get; set; }
		public double? Quizzes { get; set; }
		public double? Practical { get; set; }
		public double? TotalMarks { get; set; }
		public ICollection<Department> Departments { get; set; } = new List<Department>();

	}
}
