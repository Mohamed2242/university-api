using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityAPI.Application.DTOs;
using UniversityAPI.Application.Services;
using UniversityAPI.Core.Interface;
using UniversityAPI.Core.Models;
using UniversityAPI.Data;

namespace UniversityAPI.Controllers
{
	[Authorize(Roles = "Student")]
	[Route("api/[controller]")]
	[ApiController]
	public class StudentController : ControllerBase
	{
		private readonly IStudentService _studentService;
		private readonly IMapper _mapper;
		private readonly ILogger<StudentController> _logger;

		public StudentController(
			IStudentService studentService, IMapper mapper, ILogger<StudentController> logger)
		{
			_studentService = studentService;
			_mapper = mapper;
			_logger = logger;
		}


		// GET: api/student/{email}
		[HttpGet("{email}")]
		public async Task<IActionResult> GetStudentByEmail(string email)
		{
			try
			{
				var student = await _studentService.GetStudentByEmailAsync(email);
				var studentDto = _mapper.Map<StudentDTO>(student);
				_logger.LogInformation("Student fetched successfully: {Email}", email);
				return Ok(studentDto);
			}
			catch (ArgumentException ex)
			{
				return NotFound(ex.Message);
			}
		}

		
		// GET: api/student/available-courses/{email}
		[HttpGet("available-courses/{email}")]
		public async Task<IActionResult> GetAvailableCourses(string email)
		{
			try
			{
				var courses = await _studentService.GetAvailableCoursesAsync(email);
				var courseDtos = _mapper.Map<List<CourseDTO>>(courses);
				_logger.LogInformation("Courses fetched successfully for User: {Email}", email);
				return Ok(courseDtos);
			}
			catch (ArgumentException ex)
			{
				return NotFound(ex.Message);
			}
		}

		// POST: api/student/{email}/register-courses
		[HttpPost("register-courses/{email}")]
		public async Task<IActionResult> RegisterCourses(string email, [FromBody] List<string> selectedCoursesIds)
		{
			try
			{
				await _studentService.RegisterCoursesAsync(email, selectedCoursesIds);
				return Ok("Courses registered successfully.");
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpPut("hasRegisteredCourses/{email}")]
		public async Task<IActionResult> UpdateStudentRegistration(string email)
		{
			if (string.IsNullOrEmpty(email))
			{
				return BadRequest("Invalid email.");
			}

			try
			{
				var student = await _studentService.GetStudentByEmailAsync(email);
				if (student == null)
				{
					return NotFound($"Student with email {email} not found.");
				}

				student.HasRegisteredCourses = true;

				await _studentService.UpdateStudentAsync(student);

				return Ok(new { message = "Student registration status updated successfully." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}
		// GET: api/student/get/degrees/{email}/{semester}
		[HttpGet("get/degrees/{email}/{semester}")]
		public async Task<IActionResult> GetStudentDegrees(string email, int semester)
		{
			var studentDegrees = await _studentService.GetStudentDegreesForSpecificSemester(email, semester);
	
			return Ok(studentDegrees);
			
		}

		// POST: api/student/save-petition
		[HttpPost("save-petition")]
		public async Task<IActionResult> SavePetition([FromBody] PetitionRequestDTO petitionRequestDto)
		{
			try
			{
				var petition = await _studentService.SavePetitionAsync(petitionRequestDto.Email, petitionRequestDto.CourseName, petitionRequestDto.PetitionText);
				return Ok(petition);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpGet("GetGPA/{studentEmail}/{semester}")]
		public async Task<IActionResult> GetGPAAsync( string studentEmail, int semester)
		{
			try
			{
				var gpa = await _studentService.GetGPAAsync(studentEmail, semester);
				return Ok(new { GPA = gpa });
			}
			catch (ArgumentException ex)
			{
				return NotFound(ex.Message);
			}
		}

		[HttpGet("GetCGPA/{studentEmail}")]
		public async Task<IActionResult> GetCGPAAsync(string studentEmail)
		{
			try
			{
				var cgpa = await _studentService.GetCGPAAsync(studentEmail);
				return Ok(new { CGPA = cgpa });
			}
			catch (ArgumentException ex)
			{
				return NotFound(ex.Message);
			}
		}
	}
}
