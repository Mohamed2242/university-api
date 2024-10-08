using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UniversityAPI.Core.Interface;
using UniversityAPI.Core.Models;
using UniversityAPI.Data;

namespace UniversityAPI.Application.Services
{
    public class TokenRepository : ITokenRepository
	{
		private readonly IConfiguration _config;
		private readonly AppDbContext _context;

		public TokenRepository(IConfiguration config, AppDbContext context)
		{
			_config = config;
			_context = context;
		}

		public string CreateJwt(ApplicationUser user)
		{
			var jwtTokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes("veryveryverynewsecuresecretkeyformywebapp._._.");
			var identity = new ClaimsIdentity(
			[
				new Claim(ClaimTypes.Role, user.Role!),
				new Claim(ClaimTypes.Email,user.Email!),
				new Claim(ClaimTypes.Name,$"{user.Name}"),
				new Claim("Faculty", user.Faculty ?? string.Empty),
			]);

			var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = identity,
				Expires = DateTime.Now.AddHours(1),
				SigningCredentials = credentials
			};
			var token = jwtTokenHandler.CreateToken(tokenDescriptor);
			return jwtTokenHandler.WriteToken(token);
		}

		public string CreateRefreshToken()
		{
			var tokenBytes = RandomNumberGenerator.GetBytes(64);
			var refreshToken = Convert.ToBase64String(tokenBytes);

			var tokenInUser = _context.Users
				.Any(a => a.RefreshToken == refreshToken);
			if (tokenInUser)
			{
				return CreateRefreshToken();
			}
			return refreshToken;
		}

		public ClaimsPrincipal GetPrincipleFromExpiredToken(string token)
		{
			var key = Encoding.ASCII.GetBytes("veryveryverynewsecuresecretkeyformywebapp._._.");
			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateAudience = false,
				ValidateIssuer = false,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateLifetime = false
			};
			var tokenHandler = new JwtSecurityTokenHandler();
			var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

			if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
				throw new SecurityTokenException("This is Invalid Token");
			return principal;

		}


		/*public async Task<string> GenerateAccessToken(ApplicationUser user)
		{
			var roles = await _userManager.GetRolesAsync(user);
			var claims = new List<Claim>
		{
			new Claim(ClaimTypes.Name, user.UserName),
			new Claim(ClaimTypes.Email, user.Email),
			new Claim("Faculty", user.Faculty ?? string.Empty),
		};

			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _config["JWT:ValidIssuer"],
				audience: _config["JWT:ValidAudience"],
				expires: DateTime.Now.AddMinutes(30), // Access token expiry
				claims: claims,
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public string GenerateRefreshToken()
		{
			var randomNumber = new byte[32];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(randomNumber);
				return Convert.ToBase64String(randomNumber);
			}
		}
		*/
	}
}
