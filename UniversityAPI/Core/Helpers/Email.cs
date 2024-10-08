namespace UniversityAPI.Core.Helpers
{
    public class Email(string to, string subject, string content)
    {
        public string To { get; set; } = to;
        public string Subject { get; set; } = subject;
        public string Content { get; set; } = content;
    }
}
