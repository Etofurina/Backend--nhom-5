using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace Web_API.Services
{
	public class EmailService
	{
		private readonly IConfiguration _config;

		public EmailService(IConfiguration config)
		{
			_config = config;
		}

		public void SendEmail(string to, string subject, string body)
		{
			var email = new MimeMessage();
			// Cấu hình email người gửi (Lấy từ appsettings hoặc điền cứng để test)
			email.From.Add(MailboxAddress.Parse(_config["EmailSettings:EmailFrom"]));
			email.To.Add(MailboxAddress.Parse(to));
			email.Subject = subject;
			email.Body = new TextPart(TextFormat.Html) { Text = body };

			using var smtp = new SmtpClient();
			// Kết nối đến Gmail Server
			smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

			// Đăng nhập (Dùng App Password)
			smtp.Authenticate(_config["EmailSettings:EmailFrom"], _config["EmailSettings:AppPassword"]);

			smtp.Send(email);
			smtp.Disconnect(true);
		}
	}
}