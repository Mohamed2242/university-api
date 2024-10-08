using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UniversityAPI.Core.Models;

public class Department
{
    public int Id { get; set; }
    public string? DepartmentId { get; set; }
    public string? Name { get; set; }
    public string? Faculty { get; set; }
	[JsonIgnore]
	public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
