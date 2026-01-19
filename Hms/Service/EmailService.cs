using AutoMapper;
using Hms.Service.Request.Email;
using Hms.Service.Request.Users;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Hms.Service
{
    #region Interface
    public interface IEmailService
    {
        Task SendRegistrationEmail(UserRequest userRequest);
        Task SendAccessCodeEmail(Domains.User.User user, string accessCode);
        Task SendEmailViaSendGrid(string toEmail, string subject, string htmlContent);
    }
    #endregion

    public class EmailService : BaseService, IEmailService
    {
        #region Properties
        private readonly IEmailSenderService _emailSenderService;
        #endregion

        #region Constructor
        public EmailService(IMapper mapper , IConfiguration configuration,IEmailSenderService emailSenderService) :base(mapper, configuration)
        {
            _emailSenderService = emailSenderService;
        }
        #endregion

        #region SendRegistrationEmail
        public async Task SendRegistrationEmail(UserRequest userRequest)
        {
            try
            {
                string webAppUrl = _configuration.GetValue<string>("HmsWebAppUrl");

                // Create model for email template
                var emailModel = new WelcomeEmailModel
                {
                    FirstName = userRequest.FirstName,
                    LastName = userRequest.LastName,
                    Email = userRequest.Email,
                    LoginUrl = $"{webAppUrl}/Account/Login",
                    LogoUrl = $"{webAppUrl}/images/logo.png"
                };

                // Render view to string
                string emailBody = await _emailSenderService.RenderViewToStringAsync(
                    "EmailTemplates/WelcomeEmail",
                    emailModel
                );

                await SendEmailViaSendGrid(
               userRequest.Email,
               "Welcome to HMS",
               emailBody
           );

                Log.Information("Registration email sent to: {Email}", userRequest.Email);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send registration email to: {Email}", userRequest.Email);
            }
        }
        #endregion

        #region SendAccessCodeEmail
        public async Task SendAccessCodeEmail(Domains.User.User user, string accessCode)
        {
            try
            {
                string webAppUrl = _configuration.GetValue<string>("HmsWebAppUrl");

                var emailModel = new AccessCodeEmailModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    AccessCode = accessCode,
                    ValidMinutes = 60,
                    LoginUrl = $"{webAppUrl}/Account/Login",
                    LogoUrl = $"{webAppUrl}/images/logo.png"
                };

                string emailBody = await _emailSenderService.RenderViewToStringAsync(
                "EmailTemplates/AccessCode",
                emailModel
            );

                await SendEmailViaSendGrid(
                    user.Email,
                    "Your HMS Access Code",
                    emailBody
                );

                Log.Information("Access code email sent to: {Email}", user.Email);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send access code email to: {Email}", user.Email);
            }
        }
        #endregion

        #region SendEmailViaSendGrid
        public async Task SendEmailViaSendGrid(string toEmail, string subject, string htmlContent)
        {
            string apiKey = _configuration.GetValue<string>("SendGrid:ApiKey");
            //string apiKey = _configuration["SendGrid:ApiKey"];

            string fromEmail = _configuration.GetValue<string>("SendGrid:FromEmail") ?? "noreply@hms.com";

            var client = new SendGrid.SendGridClient(apiKey);
            var from = new SendGrid.Helpers.Mail.EmailAddress(fromEmail, "HMS System");
            var to = new SendGrid.Helpers.Mail.EmailAddress(toEmail);
            var msg = SendGrid.Helpers.Mail.MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);

            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                Log.Warning("Email send failed: {StatusCode}", response.StatusCode);
            }
        }
        #endregion
    }
}
