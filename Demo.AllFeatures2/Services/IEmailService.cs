namespace Demo.AllFeatures2.Services;

public interface IEmailService
{
    Task SendEmailAsync(string email);
    Task FailEmailAsync(string email);
}

