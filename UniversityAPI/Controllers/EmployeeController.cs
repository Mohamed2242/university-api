using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using UniversityAPI.Application.DTOs;
using UniversityAPI.Core.Helpers;
using UniversityAPI.Core.Interface;
using UniversityAPI.Core.Models;
using UniversityAPI.Data;

namespace UniversityAPI.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	public class EmployeeController : ControllerBase
	{

		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IMapper _mapper;
		public readonly AppDbContext _context;
		private readonly ILogger<EmployeeController> _logger;
		private readonly IEmailRepository _emailRepository;


		public EmployeeController(
			UserManager<ApplicationUser> userManager,
			IMapper mapper,
			AppDbContext context,
			ILogger<EmployeeController> logger,
			IEmailRepository emailRepository
			)
		{
			_userManager = userManager;
			_mapper = mapper;
			_context = context;
			_logger = logger;
			_emailRepository = emailRepository;
		}


		public static string GenerateRandomString(int length)
		{
			if (length < 4) throw new ArgumentException("Length must be at least 4 characters.");

			// Character sets
			const string upperCaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			const string lowerCaseChars = "abcdefghijklmnopqrstuvwxyz";
			const string numberChars = "0123456789";
			const string specialChars = "!@#$%^&*()-_=+[]{};:,.<>?";

			// Create a random string with at least one character from each category
			Random random = new Random();
			char[] password = new char[length];
			password[0] = upperCaseChars[random.Next(upperCaseChars.Length)];
			password[1] = lowerCaseChars[random.Next(lowerCaseChars.Length)];
			password[2] = numberChars[random.Next(numberChars.Length)];
			password[3] = specialChars[random.Next(specialChars.Length)];

			// Fill the rest of the password with random characters from all sets
			string allChars = upperCaseChars + lowerCaseChars + numberChars + specialChars;
			for (int i = 4; i < length; i++)
			{
				password[i] = allChars[random.Next(allChars.Length)];
			}

			// Shuffle the characters to avoid a predictable pattern
			return new string(Shuffle(password));
		}

		private static char[] Shuffle(char[] array)
		{
			Random rng = new Random();
			int n = array.Length;
			while (n > 1)
			{
				int k = rng.Next(n--);
				(array[n], array[k]) = (array[k], array[n]); // Swap
			}
			return array;
		}
		//----------------------------------------STUDENT----------------------------------------------------


		[Authorize(Roles = "Admin")]
		[HttpPost("add/student")]
		public async Task<IActionResult> AddStudent([FromBody] StudentDTO studentDto)
		{
			_logger.LogInformation("Adding new student: {Email}", studentDto.Email);

			var user = new ApplicationUser
			{
				UserName = studentDto.Email,
				Name = studentDto.Name,
				Email = studentDto.Email,
				BackupEmail = studentDto.BackupEmail,
				Faculty = studentDto.Faculty,
				Role = studentDto.Role
			};
			var password = GenerateRandomString(10);

			var result = await _userManager.CreateAsync(user, password);
			if (!result.Succeeded)
			{
				_logger.LogError("Failed to create user: {Email}. Errors: {Errors}", studentDto.Email, result.Errors);
				return BadRequest(result.Errors);
			}

			await _userManager.AddToRoleAsync(user, studentDto.Role);

			// Generate the password reset token
			var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
			var tokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken)); // URL-safe encoding

			user.ResetPasswordExpiry = DateTime.UtcNow.AddMinutes(60);
			var emailModel = new Email(studentDto.BackupEmail, "Reset Password!", EmailBody.EmailStringBodyForFirstTime(studentDto.Name, studentDto.Email, password, tokenEncoded));

			_emailRepository.SendEmail(emailModel);

			_logger.LogInformation("Email sent successfully to: {BackupEmail}", studentDto.BackupEmail);

			var student = _mapper.Map<Student>(studentDto);

			await _context.Students.AddAsync(student);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Student added successfully: {Email}", studentDto.Email);
			return Ok(new { Message = "Student added successfully" });
		}

		[Authorize(Roles = "Admin")]
		[HttpGet("get/student/{email}")]
		public async Task<IActionResult> GetStudent(string email)
		{
			_logger.LogInformation("Fetching student with email: {Email}", email);

			var student = await _context.Students.FirstOrDefaultAsync(c => c.Email == email);
			if (student == null)
			{
				_logger.LogWarning("Student not found: {Email}", email);
				return NotFound(new { Message = "Student not found" });
			}

			var studentDto = _mapper.Map<StudentDTO>(student);
			_logger.LogInformation("Student fetched successfully: {Email}", email);
			return Ok(studentDto);
		}

		[Authorize(Roles = "Admin")]
		[HttpGet("get/allStudents/{faculty}")]
		public async Task<IActionResult> GetAllStudents(string faculty)
		{
			_logger.LogInformation("Fetching students for faculty : {facultyName}", faculty);

			var students = await _context.Students
										  .Where(s => s.Faculty == faculty)
										  .ToListAsync();

			var studentDtos = _mapper.Map<List<StudentDTO>>(students);

			if (students == null || !students.Any())
			{
				_logger.LogWarning("No students found for faculty : {FacultyName}", faculty);
				return Ok(studentDtos);
			}

			_logger.LogInformation("Students fetched successfully for faculty ID: {FacultyName}", faculty);
			return Ok(studentDtos);
		}

		[Authorize(Roles = "Admin")]
		[HttpPut("update/student")]
		public async Task<IActionResult> UpdateStudent([FromBody] StudentDTO studentDto)
		{
			_logger.LogInformation("Attempting to update student with email: {Email}", studentDto.Email);

			var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == studentDto.Email);
			if (student == null)
			{
				_logger.LogWarning("Student not found: {Email}", studentDto.Email);
				return NotFound(new { Message = "Student not found" });
			}

			_mapper.Map(studentDto, student);
			_context.Students.Update(student);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Student updated successfully: {Email}", studentDto.Email);
			return Ok(new { Message = "Student updated successfully" });
		}

		[Authorize(Roles = "Admin")]
		[HttpDelete("delete/student/{email}")]
		public async Task<IActionResult> DeleteStudent(string email)
		{
			_logger.LogInformation("Attempting to delete student with email: {Email}", email);

			var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == email);
			if (student == null)
			{
				_logger.LogWarning("Student not found: {Email}", email);
				return NotFound(new { Message = "Student not found" });
			}

			_context.Students.Remove(student);
			await _context.SaveChangesAsync();

			var userStudent = await _userManager.FindByEmailAsync(email);
			if (userStudent == null)
			{
				_logger.LogWarning("User Student not found: {Email}", email);
				return NotFound(new { Message = "User Student not found" });
			}

			await _userManager.RemoveFromRoleAsync(userStudent, userStudent.Role!);
			var result = await _userManager.DeleteAsync(userStudent);
			if (!result.Succeeded)
			{
				_logger.LogError("Error deleting user student: {Email}", email);
				return BadRequest(result.Errors);
			}

			_logger.LogInformation("Student and user student deleted successfully: {Email}", email);
			return Ok(new { Message = "Student deleted successfully" });
		}

		//--------------------------------------------DOCTOR--------------------------------------------------------------

		[Authorize(Roles = "Admin")]
		[HttpPost("add/doctor")]
		public async Task<IActionResult> AddDoctor([FromBody] DoctorDTO doctorDto)
		{
			_logger.LogInformation("Adding new doctor: {Email}", doctorDto.Email);

			var user = new ApplicationUser
			{
				UserName = doctorDto.Email,
				Name = doctorDto.Name,
				Email = doctorDto.Email,
				BackupEmail = doctorDto.BackupEmail,
				Faculty = doctorDto.Faculty,
				Role = doctorDto.Role
			};

			var password = GenerateRandomString(10);

			var result = await _userManager.CreateAsync(user, password);
			if (!result.Succeeded)
			{
				_logger.LogError("Failed to create user: {Email}. Errors: {Errors}", doctorDto.Email, result.Errors);
				return BadRequest(result.Errors);
			}

			await _userManager.AddToRoleAsync(user, doctorDto.Role);

			// Generate the password reset token
			var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
			var tokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken)); // URL-safe encoding

			user.ResetPasswordExpiry = DateTime.UtcNow.AddMinutes(60);
			var emailModel = new Email(doctorDto.BackupEmail, "Reset Password!", EmailBody.EmailStringBodyForFirstTime(doctorDto.Name, doctorDto.Email, password, tokenEncoded));

			_emailRepository.SendEmail(emailModel);

			_logger.LogInformation("Email sent successfully to: {BackupEmail}", doctorDto.BackupEmail);

			// Handle course assignments
			var doctorCourseIds = doctorDto.Courses.Select(cd => cd.CourseId).ToList();
			var courses = await _context.Courses
				.Where(c => doctorCourseIds.Contains(c.CourseId))
				.ToListAsync();

			if (courses == null || !courses.Any())
			{
				return BadRequest(new { Message = "Invalid courses selected" });
			}

			var doctor = _mapper.Map<Doctor>(doctorDto);

			// Explicitly assign the retrieved courses to the doctor entity
			doctor.Courses = courses;

			await _context.Doctors.AddAsync(doctor);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Doctor added successfully: {Email}", doctorDto.Email);
			return Ok(new { Message = "Doctor added successfully" });
		}

		[Authorize(Roles = "Admin")]
		[HttpGet("get/doctor/{email}")]
		public async Task<IActionResult> GetDoctor(string email)
		{
			_logger.LogInformation("Fetching doctor with email: {Email}", email);

			var doctor = await _context.Doctors.FirstOrDefaultAsync(c => c.Email == email);
			if (doctor == null)
			{
				_logger.LogWarning("Doctor not found: {Email}", email);
				return NotFound(new { Message = "Doctor not found" });
			}

			var doctorDto = _mapper.Map<DoctorDTO>(doctor);
			_logger.LogInformation("Doctor fetched successfully: {Email}", email);
			return Ok(doctorDto);
		}

		[Authorize(Roles = "Admin")]
		[HttpGet("get/allDoctors/{faculty}")]
		public async Task<IActionResult> GetAllDoctors(string faculty)
		{
			_logger.LogInformation("Fetching doctors for faculty ID: {FacultyName}", faculty);

			var doctors = await _context.Doctors
										 .Where(d => d.Faculty == faculty)
										 .ToListAsync();

			var doctorDtos = _mapper.Map<List<DoctorDTO>>(doctors);

			if (doctors == null || !doctors.Any())
			{
				_logger.LogWarning("No doctors found for faculty ID: {FacultyName}", faculty);
				return Ok(doctorDtos);
			}

			_logger.LogInformation("Doctors fetched successfully for faculty ID: {FacultyName}", faculty);
			return Ok(doctorDtos);
		}


		[Authorize(Roles = "Admin")]
		[HttpPut("update/doctor")]
		public async Task<IActionResult> UpdateDoctor([FromBody] DoctorDTO doctorDto)
		{
			_logger.LogInformation("Attempting to update doctor with email: {Email}", doctorDto.Email);

			var doctor = await _context.Doctors
				.Include(d => d.Courses)
				.FirstOrDefaultAsync(d => d.Email == doctorDto.Email);
			if (doctor == null)
			{
				_logger.LogWarning("Doctor not found: {Email}", doctorDto.Email);
				return NotFound(new { Message = "Doctor not found" });
			}

			// Retrieve the courses based on the course IDs in the doctorDto
			var doctorCourseIds = doctorDto.Courses.Select(cd => cd.CourseId).ToList();
			var courses = await _context.Courses
				.Where(c => doctorCourseIds.Contains(c.CourseId))
				.ToListAsync();

			if (courses == null || !courses.Any())
			{
				_logger.LogWarning("Invalid courses selected for doctor: {Email}", doctorDto.Email);
				return BadRequest(new { Message = "Invalid courses selected" });
			}

			_mapper.Map(doctorDto, doctor);

			// Explicitly assign the retrieved courses to the doctor entity
			doctor.Courses = courses;

			_context.Doctors.Update(doctor);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Doctor updated successfully: {Email}", doctorDto.Email);
			return Ok(new { Message = "Doctor updated successfully" });
		}


		[Authorize(Roles = "Admin")]
		[HttpDelete("delete/doctor/{email}")]
		public async Task<IActionResult> DeleteDoctor(string email)
		{
			_logger.LogInformation("Attempting to delete doctor with email: {Email}", email);

			var doctor = await _context.Doctors.FirstOrDefaultAsync(s => s.Email == email);
			if (doctor == null)
			{
				_logger.LogWarning("Doctor not found: {Email}", email);
				return NotFound(new { Message = "Doctor not found" });
			}

			_context.Doctors.Remove(doctor);
			await _context.SaveChangesAsync();

			var userDoctor = await _userManager.FindByEmailAsync(email);
			if (userDoctor == null)
			{
				_logger.LogWarning("User Doctor not found: {Email}", email);
				return NotFound(new { Message = "User Doctor not found" });
			}

			await _userManager.RemoveFromRoleAsync(userDoctor, userDoctor.Role!);
			var result = await _userManager.DeleteAsync(userDoctor);
			if (!result.Succeeded)
			{
				_logger.LogError("Error deleting user doctor: {Email}", email);
				return BadRequest(result.Errors);
			}

			_logger.LogInformation("Doctor and user doctor deleted successfully: {Email}", email);
			return Ok(new { Message = "Doctor deleted successfully" });
		}

		//---------------------------------------------ASSISTANT-----------------------------------------------------------

		[Authorize(Roles = "Admin")]
		[HttpPost("add/assistant")]
		public async Task<IActionResult> AddAssistant([FromBody] AssistantDTO assistantDto)
		{
			_logger.LogInformation("Adding new assistant: {Email}", assistantDto.Email);

			var user = new ApplicationUser
			{
				UserName = assistantDto.Email,
				Name = assistantDto.Name,
				Email = assistantDto.Email,
				BackupEmail = assistantDto.BackupEmail,
				Faculty = assistantDto.Faculty,
				Role = assistantDto.Role
			};

			var password = GenerateRandomString(10);

			var result = await _userManager.CreateAsync(user, password);
			if (!result.Succeeded)
			{
				_logger.LogError("Failed to create user: {Email}. Errors: {Errors}", assistantDto.Email, result.Errors);
				return BadRequest(result.Errors);
			}

			await _userManager.AddToRoleAsync(user, assistantDto.Role);

			// Generate the password reset token
			var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
			var tokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken)); // URL-safe encoding

			user.ResetPasswordExpiry = DateTime.UtcNow.AddMinutes(60);
			var emailModel = new Email(assistantDto.BackupEmail, "Reset Password!", EmailBody.EmailStringBodyForFirstTime(assistantDto.Name, assistantDto.Email, password, tokenEncoded));

			_emailRepository.SendEmail(emailModel);

			_logger.LogInformation("Email sent successfully to: {BackupEmail}", assistantDto.BackupEmail);

			// Retrieve associated courses based on course IDs provided
			var courseIds = assistantDto.Courses.Select(c => c.CourseId).ToList(); // Assuming Course has an Id property
			var courses = await _context.Courses
				.Where(c => courseIds.Contains(c.CourseId))
				.ToListAsync();

			if (courses == null || !courses.Any())
			{
				_logger.LogWarning("Invalid courses selected for assistant: {Email}", assistantDto.Email);
				return BadRequest(new { Message = "Invalid courses selected" });
			}

			// Map assistantDto to assistant entity
			var assistant = _mapper.Map<Assistant>(assistantDto);
			assistant.Courses = courses; // Assign retrieved courses

			await _context.Assistants.AddAsync(assistant);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Assistant added successfully: {Email}", assistantDto.Email);
			return Ok(new { Message = "Assistant added successfully" });
		}


		[Authorize(Roles = "Admin")]
		[HttpGet("get/assistant/{email}")]
		public async Task<IActionResult> GetAssistant(string email)
		{
			_logger.LogInformation("Fetching assistant with email: {Email}", email);

			var assistant = await _context.Assistants.FirstOrDefaultAsync(c => c.Email == email);
			if (assistant == null)
			{
				_logger.LogWarning("Assistant not found: {Email}", email);
				return NotFound(new { Message = "Assistant not found" });
			}

			var assistantDto = _mapper.Map<AssistantDTO>(assistant);
			_logger.LogInformation("Assistant fetched successfully: {Email}", email);
			return Ok(assistantDto);
		}

		[Authorize(Roles = "Admin")]
		[HttpGet("get/allAssistants/{faculty}")]
		public async Task<IActionResult> GetAllAssistants(string faculty)
		{
			_logger.LogInformation("Fetching assistants for faculty ID: {FacultyName}", faculty);

			var assistants = await _context.Assistants
											.Where(a => a.Faculty == faculty)
											.ToListAsync();

			var assistantDtos = _mapper.Map<List<AssistantDTO>>(assistants);

			if (assistants == null || !assistants.Any())
			{
				_logger.LogWarning("No assistants found for faculty ID: {FacultyName}", faculty);
				return Ok(assistantDtos);
			}

			_logger.LogInformation("Assistants fetched successfully for faculty ID: {FacultyName}", faculty);
			return Ok(assistantDtos);
		}

		[Authorize(Roles = "Admin")]
		[HttpPut("update/assistant")]
		public async Task<IActionResult> UpdateAssistant([FromBody] AssistantDTO assistantDto)
		{
			_logger.LogInformation("Attempting to update assistant with email: {Email}", assistantDto.Email);

			var assistant = await _context.Assistants
				.Include(a => a.Courses) // Include courses for updating
				.FirstOrDefaultAsync(a => a.Email == assistantDto.Email);

			if (assistant == null)
			{
				_logger.LogWarning("Assistant not found: {Email}", assistantDto.Email);
				return NotFound(new { Message = "Assistant not found" });
			}

			// Retrieve associated courses based on course IDs provided
			var courseIds = assistantDto.Courses.Select(c => c.CourseId).ToList(); // Assuming Course has an Id property
			var courses = await _context.Courses
				.Where(c => courseIds.Contains(c.CourseId))
				.ToListAsync();

			if (courses == null || !courses.Any())
			{
				_logger.LogWarning("Invalid courses selected for assistant: {Email}", assistantDto.Email);
				return BadRequest(new { Message = "Invalid courses selected" });
			}

			// Map assistantDto to assistant entity, but don't override courses yet
			_mapper.Map(assistantDto, assistant);
			assistant.Courses = courses; // Assign retrieved courses

			// Update the assistant entity
			_context.Assistants.Update(assistant);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Assistant updated successfully: {Email}", assistantDto.Email);
			return Ok(new { Message = "Assistant updated successfully" });
		}


		[Authorize(Roles = "Admin")]
		[HttpDelete("delete/assistant/{email}")]
		public async Task<IActionResult> DeleteAssistant(string email)
		{
			_logger.LogInformation("Attempting to delete assistant with email: {Email}", email);

			var assistant = await _context.Assistants.FirstOrDefaultAsync(s => s.Email == email);
			if (assistant == null)
			{
				_logger.LogWarning("Assistant not found: {Email}", email);
				return NotFound(new { Message = "Assistant not found" });
			}

			_context.Assistants.Remove(assistant);
			await _context.SaveChangesAsync();

			var userAssistant = await _userManager.FindByEmailAsync(email);
			if (userAssistant == null)
			{
				_logger.LogWarning("User Assistant not found: {Email}", email);
				return NotFound(new { Message = "User Assistant not found" });
			}

			await _userManager.RemoveFromRoleAsync(userAssistant, userAssistant.Role!);
			var result = await _userManager.DeleteAsync(userAssistant);
			if (!result.Succeeded)
			{
				_logger.LogError("Error deleting user assistant: {Email}", email);
				return BadRequest(result.Errors);
			}

			_logger.LogInformation("Assistant and user assistant deleted successfully: {Email}", email);
			return Ok(new { Message = "Assistant deleted successfully" });
		}

		
		//--------------------------------------------EMPLOYEE--------------------------------------------------------------

		//[Authorize(Roles = "Admin")]
		[HttpPost("add/employee")]
		public async Task<IActionResult> AddEmployee([FromBody] EmployeeDTO employeeDto)
		{
			_logger.LogInformation("Adding new employee: {Email}", employeeDto.Email);

			var user = new ApplicationUser
			{
				UserName = employeeDto.Email,
				Name = employeeDto.Name,
				Email = employeeDto.Email,
				BackupEmail = employeeDto.BackupEmail,
				Faculty = employeeDto.Faculty,
				Role = employeeDto.Role
			};

			var password = GenerateRandomString(10);

			var result = await _userManager.CreateAsync(user, password);
			if (!result.Succeeded /*&& !result.Errors.Any(e => e.Code == "DuplicateUserName")*/)
			{
				_logger.LogError("Failed to create user: {Email}. Errors: {Errors}", employeeDto.Email, result.Errors);
				return BadRequest(result.Errors);
			}

			await _userManager.AddToRoleAsync(user, employeeDto.Role);

			// Generate the password reset token
			var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
			var tokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken)); // URL-safe encoding

			user.ResetPasswordExpiry = DateTime.UtcNow.AddMinutes(60);
			var emailModel = new Email(employeeDto.BackupEmail, "Reset Password!", EmailBody.EmailStringBodyForFirstTime(employeeDto.Name, employeeDto.Email, password, tokenEncoded));

			_emailRepository.SendEmail(emailModel);

			_logger.LogInformation("Email sent successfully to: {BackupEmail}", employeeDto.BackupEmail);


			var employee = _mapper.Map<Employee>(employeeDto);

			await _context.Employees.AddAsync(employee);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Employee added successfully: {Email}", employeeDto.Email);
			return Ok(new { Message = "Employee added successfully" });
		}


		[HttpPost("add/superAdmin")]
		public async Task<IActionResult> AddAdmin([FromBody] EmployeeDTO employeeDto)
		{
			_logger.LogInformation("Adding new employee: {Email}", employeeDto.Email);

			var user = new ApplicationUser
			{
				UserName = employeeDto.Email,
				Name = employeeDto.Name,
				Email = employeeDto.Email,
				Faculty = employeeDto.Faculty,
				Role = employeeDto.Role
			};

			var password = "Admin@1";

			var result = await _userManager.CreateAsync(user, password);
			if (!result.Succeeded /*&& !result.Errors.Any(e => e.Code == "DuplicateUserName")*/)
			{
				_logger.LogError("Failed to create user: {Email}. Errors: {Errors}", employeeDto.Email, result.Errors);
				return BadRequest(result.Errors);
			}

			await _userManager.AddToRoleAsync(user, employeeDto.Role);

			var employee = _mapper.Map<Employee>(employeeDto);

			await _context.Employees.AddAsync(employee);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Super Admin added successfully: {Email}", employeeDto.Email);
			return Ok(new { Message = "Super Admin added successfully" });
		}


		[Authorize(Roles = "Admin")]
		[HttpGet("get/employee/{email}")]
		public async Task<IActionResult> GetEmployee(string email)
		{
			_logger.LogInformation("Fetching employee with email: {Email}", email);

			var employee = await _context.Employees.FirstOrDefaultAsync(c => c.Email == email);
			if (employee == null)
			{
				_logger.LogWarning("Employee not found: {Email}", email);
				return NotFound(new { Message = "Employee not found" });
			}

			var employeeDto = _mapper.Map<EmployeeDTO>(employee);
			_logger.LogInformation("Employee fetched successfully: {Email}", email);
			return Ok(employeeDto);
		}


		[Authorize(Roles = "Admin")]
		[HttpGet("get/allAdmins/{faculty}")]
		public async Task<IActionResult> GetAllAdmins(string faculty)
		{
			_logger.LogInformation("Fetching admins for faculty ID: {FacultyName}", faculty);

			var employees = await _context.Employees
										   .Where(e => e.Faculty == faculty && e.Role == "Admin")
										   .ToListAsync();

			var employeeDtos = _mapper.Map<List<EmployeeDTO>>(employees);

			if (employees == null || !employees.Any())
			{
				_logger.LogWarning("No admins found for faculty ID: {FacultyName}", faculty);
				return Ok(employeeDtos);
			}

			_logger.LogInformation("Employees fetched successfully for faculty ID: {FacultyName}", faculty);
			return Ok(employeeDtos);
		}

		[Authorize(Roles = "Admin")]
		[HttpPut("update/employee")]
		public async Task<IActionResult> UpdateEmployee([FromBody] EmployeeDTO employeeDto)
		{
			_logger.LogInformation("Attempting to update employee with email: {Email}", employeeDto.Email);

			var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == employeeDto.Email);
			if (employee == null)
			{
				_logger.LogWarning("Employee not found: {Email}", employeeDto.Email);
				return NotFound(new { Message = "Employee not found" });
			}

			_mapper.Map(employeeDto, employee);
			_context.Employees.Update(employee);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Employee updated successfully: {Email}", employeeDto.Email);
			return Ok(new { Message = "Employee updated successfully" });
		}


		[Authorize(Roles = "Admin")]
		[HttpDelete("delete/employee/{email}")]
		public async Task<IActionResult> DeleteEmployee(string email)
		{
			_logger.LogInformation("Attempting to delete employee with email: {Email}", email);

			var employee = await _context.Employees.FirstOrDefaultAsync(s => s.Email == email);
			if (employee == null)
			{
				_logger.LogWarning("Employee not found: {Email}", email);
				return NotFound(new { Message = "Employee not found" });
			}

			_context.Employees.Remove(employee);
			await _context.SaveChangesAsync();

			var userEmployee = await _userManager.FindByEmailAsync(email);
			if (userEmployee == null)
			{
				_logger.LogWarning("User Employee not found: {Email}", email);
				return NotFound(new { Message = "User Employee not found" });
			}

			await _userManager.RemoveFromRoleAsync(userEmployee, userEmployee.Role!);
			var result = await _userManager.DeleteAsync(userEmployee);
			if (!result.Succeeded)
			{
				_logger.LogError("Error deleting user employee: {Email}", email);
				return BadRequest(result.Errors);
			}

			_logger.LogInformation("Employee and user employee deleted successfully: {Email}", email);
			return Ok(new { Message = "Employee deleted successfully" });
		}


		//---------------------------------------------COURSE---------------------------------------------------------------

		[Authorize(Roles = "Admin")]
		[HttpPost("add/course")]
		public async Task<IActionResult> AddCourse([FromBody] CourseDTO courseDto)
		{
			_logger.LogInformation("Adding new course: {Name}", courseDto.Name);

			// Retrieve the departments based on the department IDs in the courseDto
			var departmentIds = courseDto.Departments.Select(d => d.DepartmentId).ToList();
			var departments = await _context.Departments
				.Where(d => departmentIds.Contains(d.DepartmentId))
				.ToListAsync();

			if (departments == null || !departments.Any())
			{
				_logger.LogWarning("Invalid departments selected for course: {Name}", courseDto.Name);
				return BadRequest(new { Message = "Invalid departments selected" });
			}

			// Map courseDto to course entity
			var course = _mapper.Map<Course>(courseDto);
			course.Departments = departments; // Explicitly assign the retrieved departments

			await _context.Courses.AddAsync(course);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Course added successfully: {Name}", courseDto.Name);
			return Ok(new { Message = "Course added successfully" });
		}


		[Authorize(Roles = "Admin")]
		[HttpGet("get/course/{faculty}/{courseId}")]
		public async Task<IActionResult> GetCourse(string faculty, string courseId)
		{
			_logger.LogInformation("Fetching course: {CourseId}", courseId);

			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId && c.Faculty == faculty);
			if (course == null)
			{
				_logger.LogWarning("Course not found: {CourseId}", courseId);
				return NotFound(new { Message = "Course not found" });
			}

			var courseDto = _mapper.Map<CourseDTO>(course);
			_logger.LogInformation("Course fetched successfully: {CourseId}", courseId);
			return Ok(courseDto);
		}


		[Authorize(Roles = "Admin")]
		[HttpGet("get/allCourses/{faculty}")]
		public async Task<IActionResult> GetAllCourses(string faculty)
		{
			_logger.LogInformation("Fetching courses for faculty ID: {FacultyName}", faculty);

			var courses = await _context.Courses
										 .Where(c => c.Faculty == faculty)
										 .ToListAsync();

			var courseDtos = _mapper.Map<List<CourseDTO>>(courses);

			if (courses == null || !courses.Any())
			{
				_logger.LogWarning("No courses found for faculty ID: {FacultyName}", faculty);
				return Ok(courses);
			}

			_logger.LogInformation("Courses fetched successfully for faculty ID: {FacultyName}", faculty);
			return Ok(courseDtos);
		}

		[Authorize(Roles = "Admin")]
		[HttpGet("get/coursesBySemester/{faculty}/{semester}")]
		public async Task<IActionResult> GetCoursesBySemester(string faculty, int semester)
		{
			var courses = await _context.Courses
				.Where(c => c.Faculty == faculty && c.Semester == semester)
				.ToListAsync();

			var courseDtos = _mapper.Map<List<CourseDTO>>(courses);

			if (courses == null || !courses.Any())
			{
				_logger.LogWarning("No courses found for faculty: {FacultyName} and semester: {Semester}", faculty, semester);
				return Ok(courses);
			}

			_logger.LogInformation("Courses fetched successfully for faculty: {FacultyName} and semester: {Semester}", faculty, semester);
			return Ok(courseDtos);
		}

		[Authorize(Roles = "Admin")]
		[HttpGet("get/coursesByName/{faculty}/{name}")]
		public async Task<IActionResult> GetCoursesByName(string faculty, string name)
		{
			var courses = await _context.Courses
				.Where(c => c.Faculty == faculty && c.Name!.ToLower().Contains(name))
				.ToListAsync();

			var courseDtos = _mapper.Map<List<CourseDTO>>(courses);

			if (courses == null || !courses.Any())
			{
				_logger.LogWarning("No courses found for faculty: {FacultyName} and name: {Name}", faculty, name);
				return Ok(courses);
			}

			_logger.LogInformation("Courses fetched successfully for faculty: {FacultyName} and name: {Name}", faculty, name);
			return Ok(courseDtos);
		}

		[Authorize(Roles = "Admin")]
		[HttpGet("get/coursesByNameAndSemester/{faculty}/{semester}/{name}")]
		public async Task<IActionResult> GetCoursesByNameAndSemester(string faculty, string name, int semester)
		{
			var courses = await _context.Courses
				.Where(c => c.Faculty == faculty && c.Name!.ToLower().Contains(name) && c.Semester == semester)
				.ToListAsync();

			var courseDtos = _mapper.Map<List<CourseDTO>>(courses);

			if (courses == null || !courses.Any())
			{
				_logger.LogWarning("No courses found for faculty: {FacultyName}, semester: {Semester} and name: {Name}", faculty, semester, name);
				return Ok(courses);
			}

			_logger.LogInformation("Courses fetched successfully for faculty: {FacultyName}, semester: {Semester} and name: {Name}", faculty, semester, name);
			return Ok(courseDtos);
		}


		[Authorize(Roles = "Admin")]
		[HttpGet("get/allCoursesForAssistants/{faculty}")]
		public async Task<IActionResult> GetAllCoursesForAssistants(string faculty)
		{
			_logger.LogInformation("Fetching courses for faculty ID: {FacultyName}", faculty);

			var courses = await _context.Courses
										 .Where(c => c.Faculty == faculty && c.HaveAssistants == true)
										 .ToListAsync();

			if (courses == null || !courses.Any())
			{
				_logger.LogWarning("No courses found for faculty ID: {FacultyName}", faculty);
				return NotFound(new { Message = "No courses found for this faculty" });
			}

			var courseDtos = _mapper.Map<List<CourseDTO>>(courses);
			_logger.LogInformation("Courses fetched successfully for faculty ID: {FacultyName}", faculty);
			return Ok(courseDtos);
		}

		[Authorize(Roles = "Admin")]
		[HttpPut("update/course")]
		public async Task<IActionResult> UpdateCourse([FromBody] CourseDTO courseDto)
		{
			_logger.LogInformation("Attempting to update course with name: {CourseId}", courseDto.CourseId);

			var course = await _context.Courses
		.Include(c => c.Departments)  // Include related departments
		.FirstOrDefaultAsync(c => c.CourseId == courseDto.CourseId && c.Faculty == courseDto.Faculty);

			if (course == null)
			{
				_logger.LogWarning("Course not found: {CourseId}", courseDto.CourseId);
				return NotFound(new { Message = "Course not found" });
			}

			// Retrieve the departments based on the department IDs in the courseDto
			var departmentIds = courseDto.Departments.Select(d => d.DepartmentId).ToList();
			var departments = await _context.Departments
				.Where(d => departmentIds.Contains(d.DepartmentId))
				.ToListAsync();

			if (departments == null || !departments.Any())
			{
				_logger.LogWarning("Invalid departments selected for course: {CourseId}", courseDto.CourseId);
				return BadRequest(new { Message = "Invalid departments selected" });
			}

			// Map courseDto to course entity, but don't override departments yet
			_mapper.Map(courseDto, course);
			course.Departments = departments; // Explicitly assign the retrieved departments

			_context.Courses.Update(course);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Course updated successfully: {CourseId}", courseDto.CourseId);
			return Ok(new { Message = "Course updated successfully" });
		}


		[Authorize(Roles = "Admin")]
		[HttpDelete("delete/course/{courseId}")]
		public async Task<IActionResult> DeleteCourse(string courseId)
		{
			_logger.LogInformation("Attempting to delete course with name: {CourseId}", courseId);

			var course = await _context.Courses.FirstOrDefaultAsync(s => s.CourseId == courseId);
			if (course == null)
			{
				_logger.LogWarning("Course not found: {CourseId}", courseId);
				return NotFound(new { Message = "Course not found" });
			}

			_context.Courses.Remove(course);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Course deleted successfully: {CourseId}", courseId);
			return Ok(new { Message = "Course deleted successfully" });
		}


		//--------------------------------------------DEPARTMENT------------------------------------------------------------

		[Authorize(Roles = "Admin")]
		[HttpPost("add/department")]
		public async Task<IActionResult> AddDepartment([FromBody] DepartmentDTO departmentDto)
		{
			_logger.LogInformation("Adding new department: {Name}", departmentDto.Name);

			var department = _mapper.Map<Department>(departmentDto);

			await _context.Departments.AddAsync(department);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Department added successfully: {Name}", departmentDto.Name);
			return Ok(new { DepartmentId = departmentDto.DepartmentId, Name = departmentDto.Name, Faculty = departmentDto.Faculty, Message = "Department added successfully" });
		}

		[Authorize(Roles = "Admin")]
		[HttpPost("get/department")]
		public async Task<IActionResult> GetDepartment(DepartmentDTO departmentDto)
		{
			_logger.LogInformation("Fetching department: {Name}", departmentDto.Name);

			var department = await _context.Departments
						.FirstOrDefaultAsync(d => d.DepartmentId == departmentDto.DepartmentId && d.Faculty == departmentDto.Faculty);
			if (department == null)
			{
				_logger.LogWarning("Department not found: {Name}", departmentDto.Name);
				return NotFound(new { Message = "Department not found" });
			}

			var departmentDTO = _mapper.Map<DepartmentDTO>(department);
			_logger.LogInformation("Department fetched successfully: {Name}", departmentDto.Name);
			return Ok(departmentDTO);
		}

		[Authorize(Roles = "Admin")]
		[HttpGet("get/allDepartments/{faculty}")]
		public async Task<IActionResult> GetAllDepartments(string faculty)
		{
			_logger.LogInformation("Fetching departments for faculty ID: {FacultyName}", faculty);

			var departments = await _context.Departments
											 .Where(d => d.Faculty == faculty)
											 .ToListAsync();

			var departmentDtos = _mapper.Map<List<DepartmentDTO>>(departments);

			if (departments == null || !departments.Any())
			{
				_logger.LogWarning("No departments found for faculty ID: {FacultyName}", faculty);
				return Ok(departmentDtos);
			}

			_logger.LogInformation("Departments fetched successfully for faculty ID: {FacultyName}", faculty);
			return Ok(departmentDtos);
		}

		[Authorize(Roles = "Admin")]
		[HttpPut("update/department/")]
		public async Task<IActionResult> UpdateDepartment([FromBody] DepartmentDTO departmentDto)
		{
			_logger.LogInformation("Attempting to update department with name: {Name}", departmentDto.Name);

			var department = await _context.Departments
						.FirstOrDefaultAsync(d => d.DepartmentId == departmentDto.DepartmentId && d.Faculty == departmentDto.Faculty);
			if (department == null)
			{
				_logger.LogWarning("Department not found: {Name}", departmentDto.Name);
				return NotFound(new { Message = "Department not found" });
			}

			_mapper.Map(departmentDto, department);
			_context.Departments.Update(department);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Department updated successfully: {Name}", departmentDto.Name);
			return Ok(new { Message = "Department updated successfully" });
		}


		[Authorize(Roles = "Admin")]
		[HttpDelete("delete/department")]
		public async Task<IActionResult> DeleteDepartment([FromBody] DepartmentDTO departmentDto)
		{
			_logger.LogInformation("Attempting to delete department with Id: {Id}", departmentDto.DepartmentId);

			var department = await _context.Departments
						.FirstOrDefaultAsync(d => d.DepartmentId == departmentDto.DepartmentId && d.Faculty == departmentDto.Faculty);
			if (department == null)
			{
				_logger.LogWarning("Department not found: {Id}", departmentDto.DepartmentId);
				return NotFound(new { Message = "Department not found" });
			}

			_context.Departments.Remove(department);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Department deleted successfully: {Id}", departmentDto.DepartmentId);
			return Ok(new { Message = "Department deleted successfully" });
		}

	}
}
