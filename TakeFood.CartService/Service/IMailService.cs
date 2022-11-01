using CartService.Model.Content;

namespace StoreService.Service;

public interface IMailService
{
    Task SendMail(MailContent mailContent);

    Task SendEmailAsync(string email, string subject, string htmlMessage);
}
