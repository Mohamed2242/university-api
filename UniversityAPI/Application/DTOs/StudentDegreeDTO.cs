namespace UniversityAPI.Application.DTOs
{
	public class StudentDegreeDto
	{
		public string? StudentId { get; set; }
		public int? CurrentSemester { get; set; }
		public string? Department { get; set; }
		public bool HasRegisteredCourses { get; set; }

		public ICollection<CourseDegreesDTO> Courses { get; set; } = new List<CourseDegreesDTO>();
	}

}
