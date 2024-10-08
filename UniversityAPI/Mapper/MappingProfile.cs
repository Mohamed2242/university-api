using AutoMapper;
using UniversityAPI.Application.DTOs;
using UniversityAPI.Core.Models;

namespace UniversityAPI.Mapper
{
    public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<StudentDTO, Student>();
			CreateMap<Student, StudentDTO>();

			CreateMap<DoctorDTO, Doctor>();
			CreateMap<Doctor, DoctorDTO>();

			CreateMap<AssistantDTO, Assistant>();
			CreateMap<Assistant, AssistantDTO>();

			CreateMap<EmployeeDTO, Employee>();
			CreateMap<Employee, EmployeeDTO>();

			CreateMap<CourseDTO, Course>();
			CreateMap<Course, CourseDTO>();

			CreateMap<DepartmentDTO, Department>();
			CreateMap<Department, DepartmentDTO>();

		}
	}
}
