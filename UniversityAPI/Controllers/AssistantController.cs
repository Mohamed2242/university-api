using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniversityAPI.Application.DTOs;
using UniversityAPI.Application.Services;
using UniversityAPI.Core.Interface;

namespace UniversityAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AssistantController : ControllerBase
	{
		private readonly IAssistantService _assistantService;
		private readonly ILogger<DoctorController> _logger;

		public AssistantController(IAssistantService assistantService, ILogger<DoctorController> logger)
		{
			_assistantService = assistantService;
			_logger = logger;
		}

		[HttpGet("GetAssistant")]
		public async Task<IActionResult> GetAssistantByEmailAsync([FromQuery] string email)
		{
			try
			{
				var assistant = await _assistantService.GetAssistantByEmailAsync(email);
				return Ok(assistant);
			}
			catch (ArgumentException ex)
			{
				return NotFound(ex.Message);
			}
		}

		[HttpGet("GetCoursesForAssistant/{email}")]
		public async Task<IActionResult> GetCoursesForAssistantAsync(string email)
		{
			try
			{
				var courses = await _assistantService.GetCoursesForAssistantAsync(email);
				return Ok(courses);
			}
			catch (ArgumentException ex)
			{
				return NotFound(ex.Message);
			}
		}

		[HttpGet("GetStudentsByCourse/{courseId}")]
		public async Task<IActionResult> GetStudentsByCourseAsync(string courseId)
		{
			try
			{
				_logger.LogInformation("Fetching students registered in course with ID: {CourseId}", courseId);
				var students = await _assistantService.GetStudentsByCourseAsync(courseId);
				return Ok(students);
			}
			catch (ArgumentException ex)
			{
				_logger.LogWarning(ex, "Course not found or invalid: {CourseId}", courseId);
				return NotFound(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while fetching students for course: {CourseId}", courseId);
				return StatusCode(500, "Internal server error");
			}
		}

		[HttpGet("GetStudentsDegreesForCourse/{email}/{courseId}")]
		public async Task<IActionResult> GetStudentsDegreesForCourseAsync(string email, string courseId)
		{
			try
			{
				var studentDegrees = await _assistantService.GetStudentsDegreesForCourseAsync(email, courseId);
				return Ok(studentDegrees);
			}
			catch (ArgumentException ex)
			{
				return NotFound(ex.Message);
			}
		}

		[HttpPut("EditStudentDegreesForAssistant/{email}/{courseId}")]
		public async Task<IActionResult> EditStudentDegreesForAssistantAsync(string email, string courseId, [FromBody] StudentDegreeUpdateDTO studentDegreeDto)
		{
			try
			{
				var result = await _assistantService.EditStudentDegreesForAssistantAsync(email, courseId, studentDegreeDto);
				if (result)
					return Ok("Student degrees updated successfully.");
				else
					return BadRequest("Failed to update student degrees.");
			}
			catch (ArgumentException ex)
			{
				return NotFound(ex.Message);
			}
		}
	}
}
