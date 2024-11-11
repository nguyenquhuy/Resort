using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

public class EmailService
{
    private readonly string _smtpHost = "smtp.gmail.com"; // Ví dụ SMTP của Gmail
    private readonly int _smtpPort = 587; // Cổng SMTP của Gmail
    private readonly string _smtpUser = "huyq0202hn@gmail.com"; // Địa chỉ email gửi
    private readonly string _smtpPassword = "zujggzgbzepwpnwi"; // Mật khẩu email

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Trường Sinh Resort", _smtpUser));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        message.Body = bodyBuilder.ToMessageBody();

        using (var smtpClient = new SmtpClient())
        {
            await smtpClient.ConnectAsync(_smtpHost, _smtpPort, false);
            await smtpClient.AuthenticateAsync(_smtpUser, _smtpPassword);
            await smtpClient.SendAsync(message);
            await smtpClient.DisconnectAsync(true);
        }
    }
}
