namespace NexTrends
{
    public interface IEmailService
    {
        Task SendAsync(string recipientEmail, string subject, string message);
        Task SendAsyncWithBody(string recipientEmail, string subject, string bodyText, byte[] pdfAttachment, string attachmentFileName);
    }
}
