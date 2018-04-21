//System .dll's
using System;
using System.Security.Claims;
using System.Threading.Tasks;

//NuGet packages
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;

//Application Classes
using ARFE.Models;

namespace ARFE
{
    /// <summary>
    /// A class derived from the <see cref="Microsoft.AspNet.Identity.IIdentityMessageService"/> class
    /// to implement third party account verification via email.  This implementation is currently
    /// empty as no third party email verification is currently supported.
    /// </summary>
    public class EmailService : IIdentityMessageService
    {
        /// <summary>
        /// This method is required to be overwritten by the inheritance pattern of the
        /// <see cref="Microsoft.AspNet.Identity.IIdentityMessageService"/> class. As no third party
        /// email verification is currently supported, the method just returns 0 to the caller.
        /// </summary>
        /// <param name="message">The message to send via the registered email provider.</param>
        /// <returns>0</returns>
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }
    }


    /// <summary>
    /// A class derived from the <see cref="Microsoft.AspNet.Identity.IIdentityMessageService"/> class
    /// to implement third party account verification via SMS text messaging.  This implementation is currently
    /// empty as no third party SMS text messaging verification is currently supported.
    /// </summary>
    public class SmsService : IIdentityMessageService
    {
        /// <summary>
        /// This method is required to be overwritten by the inheritance pattern of the
        /// <see cref="Microsoft.AspNet.Identity.IIdentityMessageService"/> class. As no third party
        /// SMS text messaging verification is currently supported, the method just returns 0 to the caller.
        /// </summary>
        /// <param name="message">The message to send via the registered SMS provider.</param>
        /// <returns>0</returns>
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }


    /// <summary>
    /// Creates a useful interface to the <see cref="Microsoft.AspNet.Identity.UserManager{TUser}"/> class for easily
    /// registering new user accounts with account parameter verification handling.
    /// </summary>
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        /// <summary>
        /// Constructor, serves as a pass-through entity to the parent class constructor.
        /// </summary>
        /// <param name="store">
        /// An instance of <see cref="Microsoft.AspNet.Identity.IUserStore{TUser}"/>, which serves as the 
        /// collection of application registered users.
        /// </param>
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }


        /// <summary>
        /// A method to register a new user with validated credentials.
        /// </summary>
        /// <param name="options"> The user configuration option. </param>
        /// <param name="context"> Serves as the configuration and user verification context. </param>
        /// <returns>
        /// A reference to the ApplicationUserManager class.
        /// </returns>
        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 4, //minimum length
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
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


    /// <summary>
    /// Creates a useful interface to the <see cref="Microsoft.AspNet.Identity.UserManager{TUser}"/> class for easily
    /// verifying user accounts with account userName/password verification handling.
    /// </summary>
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        /// <summary>
        /// Constructor, serves as a pass-through entity to the parent class constructor.
        /// </summary>
        /// <param name="userManager"> A reference to the <see cref="ApplicationUserManager"/> class. </param>
        /// <param name="authenticationManager">
        /// An instance of the <see cref="Microsoft.Owin.Security.IAuthenticationManager"/> class.
        /// </param>
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }


        /// <summary>
        /// Creates a user identity object that represents a validated user and their credentials.
        /// </summary>
        /// <param name="user"> The registed user to create the identity object for. </param>
        /// <returns>
        /// The authenticated user's identity object.
        /// </returns>
        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }


        /// <summary>
        /// A method to register a new user sign in validator.
        /// </summary>
        /// <param name="options"> The user configuration option. </param>
        /// <param name="context"> Serves as the configuration and user verification context. </param>
        /// <returns>
        /// A reference to the ApplicationSignInManager class.
        /// </returns>
        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
