using System.Text;
using AutoMapper;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using UniversityAPI.Application.DTOs;
using UniversityAPI.Application.Services;
using UniversityAPI.Core.Helpers;
using UniversityAPI.Core.Interface;
using UniversityAPI.Core.Models;
using UniversityAPI.Data;

namespace UniversityAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class HomeController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ITokenRepository _tokenRepository;
		public readonly AppDbContext _context;
		private readonly IEmailRepository _emailRepository;

		public HomeController(
			UserManager<ApplicationUser> userManger,
			ITokenRepository tokenRepository,
			AppDbContext context,
			IEmailRepository emailRepository
			)
		{
			_userManager = userManger;
			_tokenRepository = tokenRepository;
			_emailRepository = emailRepository;
			_context = context;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
		{
			var user = await _userManager.FindByEmailAsync(loginDto.Email);
			if (user == null)
			{
				return Unauthorized();
			}

			if (user.Faculty != loginDto.FacultyName)
			{
				return Unauthorized("Invalid faculty.");
			}
			if (user.Role != loginDto.Role)
			{
				return BadRequest(new { message = "Unauthorized to login." });
			}

			user.Token = _tokenRepository.CreateJwt(user);
			var newAccessToken = user.Token;
			var newRefreshToken = _tokenRepository.CreateRefreshToken();

			user.RefreshToken = newRefreshToken;
			user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(60);
			await _context.SaveChangesAsync();

			return Ok(new TokenApiDTO()
			{
				AccessToken = newAccessToken,
				RefreshToken = newRefreshToken,
				Message = "Logged in Successfully"
			});
		}

		[HttpPost("refresh")]
		public async Task<IActionResult> Refresh([FromBody] TokenApiDTO tokenApiDto)
		{
			if (tokenApiDto is null)
				return BadRequest("Invalid Client Request");
			string accessToken = tokenApiDto.AccessToken;
			string refreshToken = tokenApiDto.RefreshToken;

			var principal = _tokenRepository.GetPrincipleFromExpiredToken(accessToken);
			var name = principal.Identity?.Name;
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == name);

			if (user is null)
				return BadRequest("Invalid Request");

			var newAccessToken = _tokenRepository.CreateJwt(user);
			var newRefreshToken = _tokenRepository.CreateRefreshToken();
			user.RefreshToken = newRefreshToken;
			await _context.SaveChangesAsync();

			return Ok(new TokenApiDTO()
			{
				AccessToken = newAccessToken,
				RefreshToken = newRefreshToken,
				Message = "Token refreshed Successfully"
			});
		}

		[HttpPost("send-reset-email/{backupEmail}/{email}")]
		public async Task<IActionResult> SendEmail(string backupEmail, string email)
		{
			var user = await _context.Users.FirstOrDefaultAsync(s => s.Email == email);
			if (user is null)
			{
				return NotFound(new
				{
					StatusCode = 404,
					Message = "email Doesn't Exist"
				});
			}			

			// Generate the password reset token
			var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
			var tokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken)); // URL-safe encoding

			user.ResetPasswordExpiry = DateTime.UtcNow.AddMinutes(60);

			var emailModel = new Email(backupEmail, "Reset Password!", EmailBody.EmailStringBody(email, tokenEncoded));

			_emailRepository.SendEmail(emailModel);
			_context.Entry(user).State = EntityState.Modified;
			await _context.SaveChangesAsync();

			return Ok(new
			{
				StatusCode = 200,
				Message = "Email Sent!"
			});
		}

		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDto)
		{
			// Check if NewPassword and ConfirmPassword match
			if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
			{
				return BadRequest(new
				{
					StatusCode = 400,
					Message = "Passwords do not match"
				});
			}

			//var newToken = resetPasswordDto.EmailToken?.Replace(" ", "+");

			// Step 1: Find the user by backupEmail
			//var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email!);
			var user = await _context.Users.FirstOrDefaultAsync(s => s.Email == resetPasswordDto.Email);

			if (user is null)
			{
				return NotFound(new
				{
					StatusCode = 404,
					Message = "email Doesn't Exist"
				});
			}

			var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPasswordDto.EmailToken!));

			// Step 2: Use UserManager to reset the password
			var result = await _userManager.ResetPasswordAsync(user, token, resetPasswordDto.NewPassword!);

			// Step 3: Handle errors in case the token is invalid or expired
			if (!result.Succeeded && !result.Errors.Any(e => e.Code == "InvalidToken"))
			{
				var errors = result.Errors.Select(e => e.Description).ToList();
				return BadRequest(new
				{
					StatusCode = 400,
					Message = "Invalid reset token or expired link",
					Errors = errors
				});
			}

			// Step 4: If successful, return a success message
			return Ok(new
			{
				StatusCode = 200,
				Message = "Password Reset Successfully"
			});
		}
	}
}
