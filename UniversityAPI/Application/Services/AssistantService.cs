using UniversityAPI.Application.DTOs;
using UniversityAPI.Application.Repositories;
using UniversityAPI.Controllers;
using UniversityAPI.Core.Interface;
using UniversityAPI.Core.Models;

namespace UniversityAPI.Application.Services
{
	public class AssistantService : IAssistantService
	{
		private readonly IAssistantRepository _assistantRepository;
		private readonly ILogger<AssistantService> _logger;

		public AssistantService(IAssistantRepository assistantRepository, ILogger<AssistantService> logger)
		{
			_assistantRepository = assistantRepository;
			_logger = logger;
		}

		public async Task<Assistant> GetAssistantByEmailAsync(string email)
		{
			return await _assistantRepository.GetAssistantByEmailAsync(email);
		}

		public async Task<List<CourseDTO>> GetCoursesForAssistantAsync(string email)
		{
			return await _assistantRepository.GetCoursesForAssistantAsync(email);
		}

		public async Task<IEnumerable<StudentInfoDTO>> GetStudentsByCourseAsync(string courseId)
		{
			try
			{
				_logger.LogInformation("Service: Fetching students registered in course with ID: {CourseId}", courseId);
				var students = await _assistantRepository.GetStudentsByCourseAsync(courseId);
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
			return await _assistantRepository.GetStudentsDegreesForCourseAsync(email, courseId);
		}

		public async Task<bool> EditStudentDegreesForAssistantAsync(string email, string courseId, StudentDegreeUpdateDTO studentDegreeDto)
		{
			return await _assistantRepository.EditStudentDegreesForAssistantAsync(email, courseId, studentDegreeDto);
		}
	}

}
