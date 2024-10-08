﻿using UniversityAPI.Application.DTOs;
using UniversityAPI.Core.Models;

namespace UniversityAPI.Core.Interface
{
	public interface IAssistantRepository
	{
		Task<Assistant> GetAssistantByEmailAsync(string email);
		Task<List<CourseDTO>> GetCoursesForAssistantAsync(string email);
		Task<IEnumerable<StudentInfoDTO>> GetStudentsByCourseAsync(string courseId);
		Task<StudentDegreeDto> GetStudentsDegreesForCourseAsync(string email, string courseId);
		Task<bool> EditStudentDegreesForAssistantAsync(string email, string courseId, StudentDegreeUpdateDTO studentDegreeDto);
	}
}
