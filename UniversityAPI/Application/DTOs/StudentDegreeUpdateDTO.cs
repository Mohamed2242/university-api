namespace UniversityAPI.Application.DTOs
{
	public class StudentDegreeUpdateDTO
	{
		public string Email { get; set; }
		public double? MidTerm { get; set; }
		public double? FinalExam { get; set; }
		public double? Quizzes { get; set; }
		public double? Practical { get; set; }
	}
}
