namespace UniversityAPI.Application.DTOs
{
	public class TokenApiDTO
	{
		public string AccessToken { get; set; } = string.Empty;
		public string RefreshToken { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;
    }
}
