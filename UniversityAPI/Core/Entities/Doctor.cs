using System;
using System.Collections.Generic;

namespace UniversityAPI.Core.Models;

public class Doctor : ApplicationUser
{    
    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
