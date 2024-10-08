using UniversityAPI.Application.DTOs;

namespace UniversityAPI.Core.Helpers
{
	public static class EmailBody
	{
		public static string EmailStringBodyForFirstTime(string name, string email, string password, string emailToken)
		{
			return $@"<html>
                <head></head>
                <body style=""margin: 0; padding: 0; font-family: Arial, Helvetica, sans-serif;"">
                    <div style=""height: auto; background: linear-gradient(to top, #c9c9ff 50%, #6e6ef6 90%) no-repeat; width: 400px; padding: 20px; margin: 0 auto;"">
                        <h1>Reset Your Password</h1>
                        <hr>
                        <p>You're receiving this email to reset your password for your University account.</p>
                        <p><strong>Your Name:</strong> {name}</p>
                        <p><strong>Your Email:</strong> {email}</p>
                        <p><strong>Your Password:</strong> {password}</p>
                        <p>Please click the button below to make a new password:</p>
                        <a href=""https://university-website-pink.vercel.app/reset?email={email}&code={emailToken}"" target=""_blank"" style=""background: #007bff; color: white; border-radius: 4px; display: block; margin: 0 auto; width: 50%; text-align: center; text-decoration: none; padding: 10px;"">
                            Reset Password
                        </a>
                        <p>Kind Regards,<br><br>University IT</p>
                    </div>
                </body>
              </html>";
		}

		public static string EmailStringBody(string backupEmail, string emailToken)
		{
			return $@"<html>
						<head></head>
							<body style=""margin: 0; padding: 0; font-family: Arial, Helvetica, sans-serif;"">
    <div style=""height: auto; background: linear-gradient(to top, #c9c9ff 50%, #6e6ef6 90%) no-repeat; width: 400px; padding: 20px; margin: 0 auto;"">
        <h1>Reset Your Password</h1>
        <hr>
        <p>You're receiving this email because you requested a password reset for your University account.</p>
        <p>Please click the button below to make a new password:</p>
        <a href=""https://university-website-pink.vercel.app/reset?email={backupEmail}&code={emailToken}"" target=""_blank"" style=""background: #007bff; color: white; border-radius: 4px; display: block; margin: 0 auto; width: 50%; text-align: center; text-decoration: none; padding: 10px;"">
            Reset Password
        </a>
        <p>Kind Regards,<br><br>University IT</p>
    </div>
</body>
</html>";
		}
	}
}
