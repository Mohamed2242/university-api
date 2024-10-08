using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniversityAPI.Application.DTOs;
using UniversityAPI.Core.Interface;
using UniversityAPI.Core.Models;
using UniversityAPI.Data;

namespace UniversityAPI.Application.Repositories
{
	public class DoctorRepository : IDoctorRepository
	{
		private readonly AppDbContext _context;
		private readonly ILogger<DoctorRepository> _logger;
		private readonly IMapper _mapper;
		public DoctorRepository(AppDbContext context, ILogger<DoctorRepository> logger, IMapper mapper)
		{
			_context = context;
			_logger = logger;
			_mapper = mapper;
		}

		public async Task<Doctor> GetDoctorByEmailAsync(string email)
		{
			try
			{
				_logger.LogInformation("Fetching doctor with email: {Email}", email);
				var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == email);
				if (doctor == null)
				{
					_logger.LogWarning("Doctor with email {Email} not found.", email);
					throw new ArgumentException("Doctor not found");
				}
				return doctor;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while fetching doctor with email {Email}", email);
				throw;
			}
		}

		public async Task<List<CourseDTO>> GetCoursesForDoctorsAsync(string email)
		{
			try
			{
				_logger.LogInformation("Fetching courses for doctor with email: {Email}", email);
				var doctor = await GetDoctorByEmailAsync(email);
				var courses = doctor.Courses.ToList();
				var courseDtos = _mapper.Map<List<CourseDTO>>(courses);
				_logger.LogInformation("Courses fetched successfully for doctor email: {Email}", email);

				return courseDtos;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while fetching courses for doctor with email {Email}", email);
				throw;
			}
		}

		public async Task<IEnumerable<StudentInfoDTO>> GetStudentsByCourseAsync(string courseId)
		{
			try
			{
				_logger.LogInformation("Repository: Fetching students for course with ID: {CourseId}", courseId);

				var course = await _context.Courses
					.Include(c => c.StudentDegrees)
					.ThenInclude(sd => sd.Student) // Include the student entity
					.FirstOrDefaultAsync(c => c.CourseId == courseId) ?? throw new ArgumentException("Course not found");

				var students = course.StudentDegrees
					.Where(sd => sd.CourseId == course.Id) // Ensure we're filtering by course
					.Select(sd => new StudentInfoDTO
					{
						StudentId = sd.Student!.StudentId,
						Email = sd.Student.Email,
						CurrentSemester = sd.Student.CurrentSemester,
						Department = sd.Student.Department,
						TotalCreditHours = sd.Student.TotalCreditHours,
					})
					.ToList();

				_logger.LogInformation("Repository: Found {StudentCount} students for course {CourseId}", students.Count, courseId);

				return students;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Repository: An error occurred while fetching students for course {CourseId}", courseId);
				throw;
			}
		}


		public async Task<StudentDegreeDto> GetStudentsDegreesForCourseAsync(string studentEmail, string courseId)
		{

			_logger.LogInformation("Fetching student degrees for doctor {studentEmail} and course {CourseId}", studentEmail, courseId);

			try
			{
				var student = await _context.Students
					.Include(s => s.StudentDegrees)
						.ThenInclude(sd => sd.Course)
					.FirstOrDefaultAsync(s => s.Email == studentEmail);

				// Validate student existence
				if (student == null)
				{
					_logger.LogWarning("Student not found with email: {StudentEmail}", studentEmail);
					throw new ArgumentException("Student not found");
				}

				var studentDegreesForSpecificCourse = student.StudentDegrees
					.Where(sd => sd.Course != null && sd.Course.CourseId == courseId)
					.ToList();

				// Validate if any degrees were found for the semester
				if (!studentDegreesForSpecificCourse.Any())
				{
					_logger.LogWarning("No courses found for student with email: {StudentEmail}", studentEmail);
					throw new ArgumentException("No courses found for this semester");
				}

				// Prepare DTO
				var studentDegreeDto = new StudentDegreeDto
				{
					StudentId = student.StudentId,
					CurrentSemester = student.CurrentSemester,
					Department = student.Department,
					Courses = studentDegreesForSpecificCourse.Select(sd => new CourseDegreesDTO
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
				_logger.LogInformation("Response: Found {Count} degrees for student with email: {Email}", studentDegreesForSpecificCourse.Count, studentEmail);

				return studentDegreeDto;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in GetStudentDegreesForSpecificSemester for email: {Email}", studentEmail);
				throw;
			}


		}

		public async Task<bool> EditStudentDegreesForDoctorAsync(string email, string courseId, StudentDegreeUpdateDTO studentDegreeDto)
		{
			try
			{
				_logger.LogInformation("Editing student degrees for doctor {Email} and course {CourseName}", email, courseId);
				var doctor = await _context.Doctors
					.Include(d => d.Courses)
					.FirstOrDefaultAsync(d => d.Email == email) ?? throw new ArgumentException("Doctor not found");

				var course = doctor.Courses.FirstOrDefault(c => c.CourseId == courseId) ?? throw new ArgumentException("Doctor does not teach this course");

				bool containsPracticalOrProject = (bool)(course.ContainsPracticalOrProject!);

				var studentDegree = await _context.StudentDegrees
					.Include(sd => sd.Student)
					.Include(sd => sd.Course)
					.FirstOrDefaultAsync(sd => sd.CourseId == course.Id && sd.Student!.Email == studentDegreeDto.Email)
					?? throw new ArgumentException("Student not found or not enrolled in this course.");

				// Update the fields
				if (studentDegreeDto.MidTerm.HasValue)
					studentDegree.MidTerm = studentDegreeDto.MidTerm;
				if (studentDegreeDto.FinalExam.HasValue)
					studentDegree.FinalExam = studentDegreeDto.FinalExam;
				if (studentDegreeDto.Quizzes.HasValue)
					studentDegree.Quizzes = studentDegreeDto.Quizzes;
				if (studentDegreeDto.Practical.HasValue && containsPracticalOrProject)
					studentDegree.Practical = studentDegreeDto.Practical;

				// Calculate and update total marks
				studentDegree.TotalMarks = CalculateTotalMarks(studentDegree, containsPracticalOrProject);

				_context.StudentDegrees.Update(studentDegree);
				await _context.SaveChangesAsync();

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while editing student degrees for doctor {Email} and course {CourseName}", email, courseId);
				throw;
			}
		}


		private static double CalculateTotalMarks(StudentDegree studentDegree, bool containsPracticalOrProject)
		{
			double totalMarks = 0;
			if (studentDegree.MidTerm.HasValue) totalMarks += (double)studentDegree.MidTerm;
			if (studentDegree.FinalExam.HasValue) totalMarks += (double)studentDegree.FinalExam;
			if (studentDegree.Quizzes.HasValue) totalMarks += (double)studentDegree.Quizzes;
			if (studentDegree.Practical.HasValue && containsPracticalOrProject) totalMarks += (double)studentDegree.Practical;
			return totalMarks;
		}
	}

}
