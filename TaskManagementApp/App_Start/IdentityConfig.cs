using System;
using System.Configuration;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using TaskManagementApp.DAL;
using TaskManagementApp.Models;
using System.Net.Mail;
using Vonage.Request;
using Vonage;
using Vonage.Messaging;

namespace TaskManagementApp
{
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            await configSendMessageAsync(message);
        }

        private async Task configSendMessageAsync(IdentityMessage message)
        {
            string HostAddress = ConfigurationManager.AppSettings["Host"].ToString();
            string FromEmail = ConfigurationManager.AppSettings["From"].ToString();
            string Password = ConfigurationManager.AppSettings["Password"].ToString();
            string Port = ConfigurationManager.AppSettings["Port"].ToString();

            try
            { 
                // Setting Up Email Content
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(FromEmail, "TaskPilot Support @MY");
                mailMessage.Body = message.Body;
                mailMessage.Subject = message.Subject;
                mailMessage.IsBodyHtml = true;
                mailMessage.To.Add(new MailAddress(message.Destination));

                // Setting Up SMTP Client
                SmtpClient smtpClient = new SmtpClient
                {
                    Host = HostAddress,
                    Port = Convert.ToInt32(Port),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(mailMessage.From.Address, password: Password)
                };

                smtpClient.Send(mailMessage);

            }catch (SmtpFailedRecipientException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            await Task.FromResult(0);
        }

    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            var key = ConfigurationManager.AppSettings["SMSAccountIdentification"].ToString();
            var password = ConfigurationManager.AppSettings["SMSAccountPassword"].ToString();
            var sender = ConfigurationManager.AppSettings["SMSAccountFrom"].ToString();
            var credentials = Credentials.FromApiKeyAndSecret(key, password);
            var client = new SmsClient(credentials);
            var request = new SendSmsRequest { From = sender, To = message.Destination, Body = message.Body, Text = message.Body};
            var response = client.SendAnSmsAsync(request);

            return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<TaskContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
