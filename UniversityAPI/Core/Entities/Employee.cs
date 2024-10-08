using System;
using System.Collections.Generic;

namespace UniversityAPI.Core.Models;

public class Employee : ApplicationUser
{    
	public string? Position { get; set; }

}
