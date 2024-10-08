using Microsoft.EntityFrameworkCore;
using UniversityAPI.Application.DTOs;
using UniversityAPI.Core.Entities;
using UniversityAPI.Core.Helpers;
using UniversityAPI.Core.Interface;
using UniversityAPI.Core.Models;

namespace UniversityAPI.Application.Services
{
	public class StudentService : IStudentService
	{
		private readonly IStudentRepository _studentRepository;

		public StudentService(IStudentRepository studentRepository)
		{
			_studentRepository = studentRepository;
		}

		public async Task<Student> GetStudentByEmailAsync(string email)
		{
			return await _studentRepository.GetStudentByEmailAsync(email);
		}
		public async Task UpdateStudentAsync(Student student)
		{
			await _studentRepository.UpdateStudentAsync(student);
		}
		
		public async Task<IEnumerable<Course>> GetAvailableCoursesAsync(string email)
		{
			return await _studentRepository.GetAvailableCoursesAsync(email);
		}

		public async Task RegisterCoursesAsync(string email, List<string> courseNames)
		{
			await _studentRepository.RegisterCoursesAsync(email, courseNames);
		}

		public async Task<StudentDegreeDto> GetStudentDegreesForSpecificSemester(string studentEmail, int semester)
		{
			var getStudentDegreesForSpecificSemester = await _studentRepository.GetStudentDegreesForSpecificSemester(studentEmail, semester);

			return getStudentDegreesForSpecificSemester;
		}

		public async Task<Petition> SavePetitionAsync(string email, string courseName, string petitionText)
		{
			return await _studentRepository.SavePetitionAsync(email, courseName, petitionText);
		}

		public async Task<double> GetGPAAsync(string studentEmail, int semester)
		{
			var studentDegrees = await _studentRepository.GetStudentDegreesForSpecificSemester(studentEmail, semester);

			double totalWeightedMarks = 0;
			double totalCredits = 0;

			foreach (var degree in studentDegrees.Courses)
			{
				// Ensure that CourseTotalMarks and CourseCreditHours are not null
				if (degree.CourseTotalMarks.HasValue && degree.CreditHours.HasValue)
				{
					var totalMarks = degree.StudentTotalMarks ?? 0;
					var courseTotalMarks = degree.CourseTotalMarks.Value;
					var creditHours = degree.CreditHours.Value;

					// Calculate GPA for this course
					double courseGPA = (totalMarks / courseTotalMarks) * creditHours;
					totalWeightedMarks += courseGPA;
					totalCredits += creditHours;
				}
			}

			return totalCredits > 0 ? totalWeightedMarks / totalCredits : 0;
		}


		public async Task<double> GetCGPAAsync(string studentEmail)
		{
			var allStudentDegrees = await _studentRepository.GetAllStudentDegreesAsync(studentEmail);

			double totalWeightedMarks = 0;
			double totalCredits = 0;

			foreach (var degree in allStudentDegrees.Courses)
			{
				// Ensure that CourseTotalMarks and CourseCreditHours are not null
				if (degree.CourseTotalMarks.HasValue && degree.CreditHours.HasValue)
				{
					var totalMarks = degree.StudentTotalMarks ?? 0;
					var courseTotalMarks = degree.CourseTotalMarks.Value;
					var creditHours = degree.CreditHours.Value;

					// Calculate GPA for this course
					double courseGPA = (totalMarks / courseTotalMarks) * creditHours;
					totalWeightedMarks += courseGPA;
					totalCredits += creditHours;
				}
			}

			return totalCredits > 0 ? totalWeightedMarks / totalCredits : 0;
		}


	}
}
