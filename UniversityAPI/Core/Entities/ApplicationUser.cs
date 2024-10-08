using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace UniversityAPI.Core.Models;

public partial class ApplicationUser : IdentityUser
{
    public string? Name { get; set; }

    public string? BackupEmail { get; set; }

    public string? Faculty { get; set; }
    
	// Add Token and Refresh Token property
	public string? Token { get; set; }

    public string? Role { get; set; }

    public string? RefreshToken { get; set; }

	public DateTime RefreshTokenExpiryTime { get; set; }

	public DateTime ResetPasswordExpiry { get; set; }
}
