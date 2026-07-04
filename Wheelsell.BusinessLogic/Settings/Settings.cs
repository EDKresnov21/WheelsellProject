namespace Wheelsell.BusinessLogic.Settings;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}

public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = "WheelSell";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ClientBaseUrl { get; set; } = "http://localhost:5173";
}

public class FileStorageSettings
{
    public string RootPath { get; set; } = "wwwroot/uploads";
    public string BaseUrl { get; set; } = "/uploads";
    public int MaxImagesPerAdvert { get; set; } = 15;
    public int MaxVideosPerAdvert { get; set; } = 5;
    public long MaxVideoSizeBytes { get; set; } = 300L * 1024 * 1024;
}
