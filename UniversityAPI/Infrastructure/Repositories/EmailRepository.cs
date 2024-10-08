using MailKit.Net.Smtp;
using MimeKit;
using UniversityAPI.Core.Helpers;
using UniversityAPI.Core.Interface;

namespace UniversityAPI.Infrastructure.Services
{
	public class EmailRepository : IEmailRepository
	{
		private readonly IConfiguration _config;
		public EmailRepository(IConfiguration configuration)
		{
			_config = configuration;
		}

		public void SendEmail(Email email)
		{
			var emailMessage = new MimeMessage();
			var from = _config["EmailSettings:From"];
			emailMessage.From.Add(new MailboxAddress("Changing Password", from));
			emailMessage.To.Add(new MailboxAddress(email.To, email.To));
			emailMessage.Subject = email.Subject;

			// Directly assign email.Content to TextPart without string.Format
			emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
			{
				Text = email.Content // No need for string.Format
			};

			using (var client = new SmtpClient())
			{
				try
				{
					client.Connect(_config["EmailSettings:SmtpServer"], 465, true);
					client.Authenticate(_config["EmailSettings:From"], _config["EmailSettings:Password"]);
					client.Send(emailMessage);
				}
				catch (Exception ex)
				{
					throw;
				}
				finally
				{
					client.Disconnect(true);
					client.Dispose();
				}
			}
		}
	}
}
