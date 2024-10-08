using UniversityAPI.Application.DTOs;
using UniversityAPI.Core.Entities;
using UniversityAPI.Core.Models;

namespace UniversityAPI.Core.Interface
{
	public interface IStudentRepository
	{
		Task<Student> GetStudentByEmailAsync(string studentEmail);
		Task UpdateStudentAsync(Student student);
		Task<IEnumerable<Course>> GetAvailableCoursesAsync(string studentEmail);
		Task RegisterCoursesAsync(string studentEmail, List<string> courseNames);
		Task<Petition> SavePetitionAsync(string studentEmail, string courseName, string petitionText);
		Task<StudentDegreeDto> GetStudentDegreesForSpecificSemester(string studentEmail, int semester);
		Task<StudentDegreeDto> GetAllStudentDegreesAsync(string studentEmail);
	}
}
