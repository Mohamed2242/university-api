using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UniversityAPI.Core.Models;

public class Course
{
    public int Id { get; set; }
    public string? CourseId { get; set; }
    public string? Name { get; set; }
    public int? CreditHours { get; set; }
    public string? Faculty { get; set; }
    public int Semester { get; set; }
    public bool? ContainsPracticalOrProject { get; set; }
    public bool? HaveAssistants { get; set; }
    public double? MidTerm { get; set; }
    public double? FinalExam { get; set; }
    public double? Quizzes { get; set; }
    public double? Practical { get; set; }
    public double? TotalMarks { get; set; }
    [JsonIgnore]
    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    [JsonIgnore]
	public virtual ICollection<Assistant> Assistants { get; set; } = new List<Assistant>();
	public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
	public virtual ICollection<StudentDegree> StudentDegrees { get; set; } = new List<StudentDegree>();
}
