namespace UniversityAPI.Application.DTOs
{
	public class StudentInfoDTO
	{
		public string? StudentId { get; set; }
        public string? Email { get; set; }
        public int? CurrentSemester { get; set; }
		public string? Department { get; set; }
		public int? TotalCreditHours { get; set; }
	}
}
