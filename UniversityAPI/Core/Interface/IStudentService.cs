using Microsoft.EntityFrameworkCore;
using UniversityAPI.Application.DTOs;
using UniversityAPI.Core.Entities;
using UniversityAPI.Core.Models;

namespace UniversityAPI.Core.Interface
{
	public interface IStudentService
	{
		Task<Student> GetStudentByEmailAsync(string email);
		Task<IEnumerable<Course>> GetAvailableCoursesAsync(string email);
		Task RegisterCoursesAsync(string email, List<string> courseNames);
		Task UpdateStudentAsync(Student student);
		Task<StudentDegreeDto> GetStudentDegreesForSpecificSemester(string email, int semester);
		Task<Petition> SavePetitionAsync(string email, string courseName, string petitionText);
		Task<double> GetGPAAsync(string studentEmail, int semester);
		Task<double> GetCGPAAsync(string studentEmail);
	}
}
