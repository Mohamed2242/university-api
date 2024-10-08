using System.Security.Claims;
using UniversityAPI.Core.Models;

namespace UniversityAPI.Core.Interface
{
	public interface ITokenRepository
	{
		string CreateJwt(ApplicationUser user);
		string CreateRefreshToken();
		ClaimsPrincipal GetPrincipleFromExpiredToken(string token);
	}
}
