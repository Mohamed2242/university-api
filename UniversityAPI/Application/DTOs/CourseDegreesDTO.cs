namespace UniversityAPI.Application.DTOs
{
	public class CourseDegreesDTO
	{
		public string? CourseId { get; set; }
		public string? Name { get; set; }
		public int? CreditHours { get; set; }
		public bool? ContainsPracticalOrProject { get; set; }

		public double? CourseMidTerm { get; set; }
		public double? CourseFinalExam { get; set; }
		public double? CourseQuizzes { get; set; }
		public double? CoursePractical { get; set; }
		public double? CourseTotalMarks { get; set; }

		public double? StudentMidTerm { get; set; }
		public double? StudentFinalExam { get; set; }
		public double? StudentQuizzes { get; set; }
		public double? StudentPractical { get; set; }
		public double? StudentTotalMarks { get; set; }


	}
}
