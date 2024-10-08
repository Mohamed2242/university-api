using UniversityAPI.Core.Helpers;

namespace UniversityAPI.Core.Interface
{
    public interface IEmailRepository
	{
		void SendEmail(Email email);
	}
}
