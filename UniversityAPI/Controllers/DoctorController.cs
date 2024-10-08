using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniversityAPI.Application.DTOs;
using UniversityAPI.Core.Interface;

namespace UniversityAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DoctorController : ControllerBase
	{
		private readonly IDoctorService _doctorService;
		private readonly ILogger<DoctorController> _logger;

		public DoctorController(IDoctorService doctorService, ILogger<DoctorController> logger)
		{
			_doctorService = doctorService;
			_logger = logger;
		}

		[HttpGet("GetDoctorByEmail")]
		public async Task<IActionResult> GetDoctorByEmailAsync(string email)
		{
			try
			{
				var doctor = await _doctorService.GetDoctorByEmailAsync(email);
				return Ok(doctor);
			}
			catch (ArgumentException ex)
			{
				return NotFound(ex.Message);
			}
		}

		[HttpGet("GetCoursesForDoctor/{email}")]
		public async Task<IActionResult> GetCoursesForDoctorsAsync(string email)
		{
			try
			{
				var courses = await _doctorService.GetCoursesForDoctorsAsync(email);
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
				var students = await _doctorService.GetStudentsByCourseAsync(courseId);
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
				var studentsDegrees = await _doctorService.GetStudentsDegreesForCourseAsync(email, courseId);
				return Ok(studentsDegrees);
			}
			catch (ArgumentException ex)
			{
				return NotFound(ex.Message);
			}
		}

		[HttpPut("EditStudentDegreesForDoctor/{email}/{courseId}")]
		public async Task<IActionResult> EditStudentDegreesForDoctorAsync(string email, string courseId, [FromBody] StudentDegreeUpdateDTO studentDegreeDto)
		{
			try
			{
				var result = await _doctorService.EditStudentDegreesForDoctorAsync(email, courseId, studentDegreeDto);
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
