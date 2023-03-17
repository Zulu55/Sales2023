using MailKit.Net.Smtp;
using MimeKit;
using Sales.Shared.Responses;

namespace Sales.API.Helpers
{
    public class MailHelper : IMailHelper
    {
        private readonly IConfiguration _configuration;

        public MailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Response SendMail(string toName, string toEmail, string subject, string body)
        {
            try
            {
                var from = _configuration["Mail:From"];
                var name = _configuration["Mail:Name"];
                var smtp = _configuration["Mail:Smtp"];
                var port = _configuration["Mail:Port"];
                var password = _configuration["Mail:Password"];

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(name, from));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Connect(smtp, int.Parse(port!), false);
                    client.Authenticate(from, password);
                    client.Send(message);
                    client.Disconnect(true);
                }

                return new Response { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    Result = ex
                };
            }
        }
    }
}
