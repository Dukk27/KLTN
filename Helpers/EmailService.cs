using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace KLTN.Helpers
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
            string emailType,
            string toEmail,
            string subject,
            string body
        )
        {
            var emailSettings = _configuration.GetSection("EmailSettings").GetSection(emailType);
            if (emailSettings == null)
            {
                throw new Exception($"Không tìm thấy cấu hình email cho loại: {emailType}");
            }

            string smtpServer = emailSettings["SmtpServer"];
            int port = int.Parse(emailSettings["SmtpPort"]);
            string senderEmail = emailSettings["SenderEmail"];
            string senderPassword = emailSettings["SenderPassword"];

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Hệ Thống", senderEmail));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(senderEmail, senderPassword);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi email ({emailType}): {ex.Message}");
            }
        }
    }
}
