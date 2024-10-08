using System;
using System.Collections.Generic;

namespace UniversityAPI.Core.Models;

public class Student : ApplicationUser
{    
    public string? StudentId { get; set; }
    
    public int? CurrentSemester { get; set; }

    public string? Department { get; set; }

    public int? TotalCreditHours { get; set; }

	public bool HasRegisteredCourses { get; set; }

	public virtual ICollection<StudentDegree> StudentDegrees { get; set; } = new List<StudentDegree>();

}
