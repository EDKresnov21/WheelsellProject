using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using Wheelsell.BusinessLogic.Settings;

namespace Wheelsell.BusinessLogic.Services;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string toEmail, string username, string token);
    Task SendPasswordResetAsync(string toEmail, string username, string token);
    Task SendNewMessageNotificationAsync(string toEmail, string username, string advertTitle, string senderUsername);
    Task SendAdvertSoldNotificationAsync(string toEmail, string username, string advertTitle);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public Task SendEmailConfirmationAsync(string toEmail, string username, string token)
    {
        var link = $"{_settings.ClientBaseUrl}/confirm-email?token={token}";
        var body = $"<p>Hi {username},</p>" +
                   "<p>Welcome to WheelSell! Please confirm your email address by clicking the link below.</p>" +
                   $"<p><a href=\"{link}\">Confirm my email</a></p>" +
                   "<p>If you did not create this account, you can ignore this email.</p>";

        return SendAsync(toEmail, "Confirm your WheelSell account", body);
    }

    public Task SendPasswordResetAsync(string toEmail, string username, string token)
    {
        var link = $"{_settings.ClientBaseUrl}/reset-password?token={token}";
        var body = $"<p>Hi {username},</p>" +
                   "<p>We received a request to reset your WheelSell password. Click the link below to choose a new one.</p>" +
                   $"<p><a href=\"{link}\">Reset my password</a></p>" +
                   "<p>If you did not request this, you can safely ignore this email.</p>";

        return SendAsync(toEmail, "Reset your WheelSell password", body);
    }

    public Task SendNewMessageNotificationAsync(string toEmail, string username, string advertTitle, string senderUsername)
    {
        var body = $"<p>Hi {username},</p>" +
                   $"<p>{senderUsername} sent you a new message about your advert \"{advertTitle}\" on WheelSell.</p>" +
                   $"<p><a href=\"{_settings.ClientBaseUrl}/chat\">Open WheelSell chat</a></p>";

        return SendAsync(toEmail, "New message on WheelSell", body);
    }

    public Task SendAdvertSoldNotificationAsync(string toEmail, string username, string advertTitle)
    {
        var body = $"<p>Hi {username},</p>" +
                   $"<p>Your advert \"{advertTitle}\" has been marked as sold on WheelSell.</p>";

        return SendAsync(toEmail, "Your advert has been marked as sold", body);
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.Username, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
