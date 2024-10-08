using UniversityAPI.Application.DTOs;
using UniversityAPI.Controllers;
using UniversityAPI.Core.Interface;
using UniversityAPI.Core.Models;

namespace UniversityAPI.Application.Services
{
	public class DoctorService : IDoctorService
	{
		private readonly IDoctorRepository _doctorRepository;
		private readonly ILogger<DoctorService> _logger;

		public DoctorService(IDoctorRepository doctorRepository, ILogger<DoctorService> logger)
		{
			_doctorRepository = doctorRepository;
			_logger = logger;
		}

		public async Task<Doctor> GetDoctorByEmailAsync(string email)
		{
			return await _doctorRepository.GetDoctorByEmailAsync(email);
		}

		public async Task<List<CourseDTO>> GetCoursesForDoctorsAsync(string email)
		{
			return await _doctorRepository.GetCoursesForDoctorsAsync(email);
		}

		public async Task<IEnumerable<StudentInfoDTO>> GetStudentsByCourseAsync(string courseId)
		{
			try
			{
				_logger.LogInformation("Service: Fetching students registered in course with ID: {CourseId}", courseId);
				var students = await _doctorRepository.GetStudentsByCourseAsync(courseId);
				return students;
			}
			catch (ArgumentException ex)
			{
				_logger.LogWarning(ex, "Service: Course not found or invalid: {CourseId}", courseId);
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Service: Error occurred while fetching students for course: {CourseId}", courseId);
				throw;
			}
		}

		public async Task<StudentDegreeDto> GetStudentsDegreesForCourseAsync(string email, string courseId)
		{
			return await _doctorRepository.GetStudentsDegreesForCourseAsync(email, courseId);
		}

		public async Task<bool> EditStudentDegreesForDoctorAsync(string email, string courseId, StudentDegreeUpdateDTO studentDegreeDto)
		{
			return await _doctorRepository.EditStudentDegreesForDoctorAsync(email, courseId, studentDegreeDto);
		}
	}
}
