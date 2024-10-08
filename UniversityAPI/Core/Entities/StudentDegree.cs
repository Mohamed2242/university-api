using System;
using System.Collections.Generic;

namespace UniversityAPI.Core.Models;

public class StudentDegree
{
	public int Id { get; set; } // Primary key of the StudentDegree entity

	public double? MidTerm { get; set; }

    public double? FinalExam { get; set; }
    public double? Quizzes { get; set; }
    public double? Practical { get; set; }
    public double? TotalMarks { get; set; }

    public string? StudentId { get; set; }
	public virtual Student? Student { get; set; }

	public int? CourseId { get; set; }
	public virtual Course? Course { get; set; }

}
