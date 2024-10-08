using System.Linq;
using Microsoft.EntityFrameworkCore;
using UniversityAPI.Application.DTOs;
using UniversityAPI.Core.Entities;
using UniversityAPI.Core.Interface;
using UniversityAPI.Core.Models;
using UniversityAPI.Data;

namespace UniversityAPI.Application.Services
{
	public class StudentRepository : IStudentRepository
	{
		private readonly AppDbContext _context;
		private readonly ILogger<StudentRepository> _logger;

		public StudentRepository(AppDbContext context, ILogger<StudentRepository> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task<Student> GetStudentByEmailAsync(string email)
		{
			_logger.LogInformation("Request: GetStudentByEmailAsync - Email: {Email}", email);

			try
			{
				var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == email);
				if (student == null)
				{
					_logger.LogWarning("Student not found with email: {Email}", email);
					throw new ArgumentException("Student not found");
				}

				_logger.LogInformation("Response: Successfully retrieved student for email: {Email}", email);
				return student;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in GetStudentByEmailAsync for email: {Email}", email);
				throw;
			}
		}

		public async Task UpdateStudentAsync(Student student)
		{
			_context.Students.Update(student);
			await _context.SaveChangesAsync();
		}
		
		public async Task<IEnumerable<Course>> GetAvailableCoursesAsync(string email)
		{
			_logger.LogInformation("Request: GetAvailableCoursesAsync - Email: {Email}", email);

			try
			{
				// Fetch the student by email
				var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == email);
				if (student == null)
				{
					_logger.LogWarning("Student not found with email: {Email}", email);
					throw new ArgumentException("Invalid email address");
				}

				if (student.CurrentSemester == null)
				{
					_logger.LogWarning("Current semester is null for student with email: {Email}", email);
					throw new InvalidOperationException("Current semester is not assigned for this student.");
				}

				_logger.LogInformation("Fetching courses for department: {Department}, semester: {Semester}", student.Department, student.CurrentSemester);

				// Fetch available courses based on department, semester, and faculty
				var availableCourses = await _context.Courses
					.Where(c => c.Departments.Any(d => d.Name == student.Department)
					&& c.Semester == student.CurrentSemester
					&& c.Faculty == student.Faculty)
					.ToListAsync();


				_logger.LogInformation("Response: Found {Count} courses for email: {Email}", availableCourses.Count, email);

				return availableCourses;
			}
			catch (ArgumentException argEx)
			{
				_logger.LogError(argEx, "Invalid argument in GetAvailableCoursesAsync for email: {Email}", email);
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in GetAvailableCoursesAsync for email: {Email}", email);
				throw;
			}
		}


		public async Task RegisterCoursesAsync(string email, List<string> selectedCoursesIds)
		{
			_logger.LogInformation("Request: RegisterCoursesAsync - Email: {Email}, Course Names: {CourseNames}", email, string.Join(", ", selectedCoursesIds));

			try
			{
				var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == email);
				if (student == null)
				{
					_logger.LogWarning("Student not found with email: {Email}", email);
					throw new ArgumentException("Invalid seat number");
				}

				var courseIds = await _context.Courses
					.Where(c => selectedCoursesIds.Contains(c.CourseId!) && c.Faculty == student.Faculty)
					.Select(c => c.Id)
					.ToListAsync();

				if (!courseIds.Any())
				{
					_logger.LogWarning("No matching courses found for email: {Email}", email);
					throw new ArgumentException("No matching courses found");
				}

				_logger.LogInformation("Registering {Count} courses for student with email: {Email}", courseIds.Count, email);

				foreach (var courseId in courseIds)
				{
					await _context.StudentDegrees.AddAsync(new StudentDegree
					{
						StudentId = student.Id,
						CourseId = courseId,
						MidTerm = 0,
						Quizzes = 0,
						Practical = 0,
						FinalExam = 0,
						TotalMarks = 0
					});
				}

				await _context.SaveChangesAsync();
				_logger.LogInformation("Response: Successfully registered courses for student with email: {Email}", email);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in RegisterCoursesAsync for email: {Email}", email);
				throw;
			}
		}

		public async Task<Petition> SavePetitionAsync(string email, string courseName, string petitionText)
		{
			_logger.LogInformation("Request: SavePetitionAsync - Email: {Email}, Course: {CourseName}, PetitionText: {PetitionText}", email, courseName, petitionText);

			try
			{
				var petition = new Petition
				{
					Email = email,
					CourseName = courseName,
					Text = petitionText,
					Date = DateTime.UtcNow
				};

				await _context.Petitions.AddAsync(petition);
				await _context.SaveChangesAsync();

				_logger.LogInformation("Response: Successfully saved petition for email: {Email}", email);
				return petition;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in SavePetitionAsync for email: {Email}", email);
				throw;
			}
		}

		public async Task<StudentDegreeDto> GetStudentDegreesForSpecificSemester(string studentEmail, int semester)
		{
			_logger.LogInformation("Request: GetStudentDegreesForSpecificSemester - Email: {Email}, Semester: {Semester}", studentEmail, semester);

			try
			{
				var student = await _context.Students
					.Include(s => s.StudentDegrees)
						.ThenInclude(sd => sd.Course)
					.FirstOrDefaultAsync(s => s.Email == studentEmail);

				// Validate student existence
				if (student == null)
				{
					_logger.LogWarning("Student not found with email: {Email}", studentEmail);
					throw new ArgumentException("Student not found");
				}

				// Filter degrees based on the specified semester
				var studentDegrees = student.StudentDegrees
					.Where(sd => sd.Course != null && sd.Course.Semester == semester)
					.ToList();

				// Validate if any degrees were found for the semester
				if (!studentDegrees.Any())
				{
					_logger.LogWarning("No courses found for student with email: {Email} in semester: {Semester}", studentEmail, semester);
					throw new ArgumentException("No courses found for this semester");
				}

				// Prepare DTO
				var studentDegreeDto = new StudentDegreeDto
				{
					StudentId = student.StudentId,
					CurrentSemester = student.CurrentSemester,
					Department = student.Department,
					Courses = studentDegrees.Select(sd => new CourseDegreesDTO
					{
						CourseId = sd.Course?.CourseId,
						Name = sd.Course?.Name,
						CreditHours = sd.Course?.CreditHours,
						ContainsPracticalOrProject = sd.Course?.ContainsPracticalOrProject,

						CourseMidTerm = sd.Course?.MidTerm,
						CourseFinalExam = sd.Course?.FinalExam,
						CourseQuizzes = sd.Course?.Quizzes,
						CoursePractical = sd.Course?.Practical,
						CourseTotalMarks = sd.Course?.TotalMarks,

						StudentMidTerm = sd.MidTerm,
						StudentFinalExam = sd.FinalExam,
						StudentQuizzes = sd.Quizzes,
						StudentPractical = sd.Practical,
						StudentTotalMarks = sd.TotalMarks
					}).ToList()
				};

				// Log the found degrees count
				_logger.LogInformation("Response: Found {Count} degrees for student with email: {Email}, Semester: {Semester}", studentDegrees.Count, studentEmail, semester);
				return studentDegreeDto;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in GetStudentDegreesForSpecificSemester for email: {Email}, Semester: {Semester}", studentEmail, semester);
				throw;
			}
		}



		public async Task<StudentDegreeDto> GetAllStudentDegreesAsync(string studentEmail)
		{
			_logger.LogInformation("Request: GetAllStudentDegreesAsync - Email: {Email}", studentEmail);

			try
			{
				var student = await _context.Students
					.Include(s => s.StudentDegrees)
						.ThenInclude(sd => sd.Course)
					.FirstOrDefaultAsync(s => s.Email == studentEmail);

				// Validate student existence
				if (student == null)
				{
					_logger.LogWarning("Student not found with email: {Email}", studentEmail);
					throw new ArgumentException("Student not found");
				}

				var studentDegrees = student.StudentDegrees
					.Where(sd => sd.Course != null)
					.ToList();

				// Validate if any degrees were found for the semester
				if (!studentDegrees.Any())
				{
					_logger.LogWarning("No courses found for student with email: {Email}", studentEmail);
					throw new ArgumentException("No courses found for this semester");
				}

				// Prepare DTO
				var studentDegreeDto = new StudentDegreeDto
				{
					StudentId = student.StudentId,
					CurrentSemester = student.CurrentSemester,
					Department = student.Department,
					Courses = studentDegrees.Select(sd => new CourseDegreesDTO
					{
						CourseId = sd.Course?.CourseId,
						Name = sd.Course?.Name,
						CreditHours = sd.Course?.CreditHours,
						ContainsPracticalOrProject = sd.Course?.ContainsPracticalOrProject,

						CourseMidTerm = sd.Course?.MidTerm,
						CourseFinalExam = sd.Course?.FinalExam,
						CourseQuizzes = sd.Course?.Quizzes,
						CoursePractical = sd.Course?.Practical,
						CourseTotalMarks = sd.Course?.TotalMarks,

						StudentMidTerm = sd.MidTerm,
						StudentFinalExam = sd.FinalExam,
						StudentQuizzes = sd.Quizzes,
						StudentPractical = sd.Practical,
						StudentTotalMarks = sd.TotalMarks
					}).ToList()
				};

				// Log the found degrees count
				_logger.LogInformation("Response: Found {Count} degrees for student with email: {Email}", studentDegrees.Count, studentEmail);

				return studentDegreeDto;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in GetStudentDegreesForSpecificSemester for email: {Email}", studentEmail);
				throw;
			}
		}

	}

}
