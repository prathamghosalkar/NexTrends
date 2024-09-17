using System.Net.Mail;
using System.Net;

namespace NexTrends
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAsync(string recipientEmail, string subject, string message)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpHost = emailSettings["SMTPHost"];
            var smtpPort = int.Parse(emailSettings["SMTPPort"]);
            var senderEmail = emailSettings["SenderEmail"];
            var senderPassword = emailSettings["SenderPassword"];

            try
            {
                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = smtpPort == 587, // Enable SSL/TLS based on port
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(recipientEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully.");
            }
            catch (SmtpException ex)
            {
                _logger.LogError($"SMTP error: {ex.Message}");
                throw;  // Re-throw the exception to handle it further up the stack if needed
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email: {ex.Message}");
                throw;  // Re-throw the exception to handle it further up the stack if needed
            }
        }

        // New method to send email with a PDF attachment
        public async Task SendAsyncWithBody(string recipientEmail, string subject, string bodyText, byte[] pdfAttachment, string attachmentFileName)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpHost = emailSettings["SMTPHost"];
            var smtpPort = int.Parse(emailSettings["SMTPPort"]);
            var senderEmail = emailSettings["SenderEmail"];
            var senderPassword = emailSettings["SenderPassword"];

            try
            {
                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = smtpPort == 587, // Enable SSL/TLS based on port
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = subject,
                    Body = bodyText,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(recipientEmail);

                // Attach the PDF to the email
                if (pdfAttachment != null)
                {
                    var attachment = new Attachment(new MemoryStream(pdfAttachment), attachmentFileName, "application/pdf");
                    mailMessage.Attachments.Add(attachment);
                }

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email with PDF attachment sent successfully.");
            }
            catch (SmtpException ex)
            {
                _logger.LogError($"SMTP error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email with attachment: {ex.Message}");
                throw;
            }
        }
    }



}
