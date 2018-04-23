//System .dll's
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;

//NuGet packages
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.WindowsAzure.Storage.Blob;

//other classes
using ARFE.Models;

/// <summary>
/// This namespaces is a sub-namespace of the ARFE project namespace specifically
/// for the ASP.NET Controllers.
/// </summary>
namespace ARFE.Controllers
{
    /// <summary>
    /// A class derived from the <see cref="Controller"/> class that has all
    /// of the controller actions to manage user accounts.
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        #region Member Variables

        /// <summary> A member instance of the <see cref="ApplicationSignInManager"/> </summary>
        private ApplicationSignInManager _SignInManager;
        /// <summary> A member instance of the <see cref="ApplicationUserManager"/> </summary>
        private ApplicationUserManager _UserManager;

        #endregion


        #region Constructor

        /// <summary> The default constructor. </summary>
        public AccountController() { }


        /// <summary>
        /// An overloaded constructor that takes instances of the class properties as parameters.
        /// </summary>
        /// <param name="userManager">An instance of the <see cref="ApplicationUserManager"/> class.</param>
        /// <param name="signInManager">An instance of the <see cref="ApplicationSignInManager"/> class.</param>
        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            _UserManager = userManager;
            _SignInManager = signInManager;
        }

        #endregion


        #region Properties

        /// <summary> 
        /// A property reference to the <see cref="ApplicationSignInManager"/> class.  This reference is either to 
        /// the instance provided by the <see cref="ManageController(ApplicationUserManager, ApplicationSignInManager)"/> constructor,
        /// or to the default instance that exists within the context of <see cref="Microsoft.Owin"/>.
        /// </summary>
        public ApplicationSignInManager SignInManager => _SignInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();

        /// <summary> 
        /// A property reference to the <see cref="ApplicationUserManager"/> class.  This reference is either to 
        /// the instance provided by the <see cref="ManageController(ApplicationUserManager, ApplicationSignInManager)"/> constructor,
        /// or to the default instance that exists within the context of <see cref="Microsoft.Owin"/>.
        /// </summary>
        public ApplicationUserManager UserManager => _UserManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

        #endregion


        /// <summary>
        /// The default Login method.
        /// </summary>
        /// <param name="returnUrl">The page url to redirect to after loggin in.</param>
        /// <returns> A web page redirect to the <paramref name="returnUrl"/> address. </returns>
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        /// <summary>
        /// The login method that takes the completed login model object
        /// as login credentials and parameters.
        /// </summary>
        /// <param name="model">The login object with all of the user credentials and parameters.</param>
        /// <param name="returnUrl">The page url to redirect to after loggin in.</param>
        /// <returns> A web page redirect to the appropriate page following the login attempt. </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            ActionResult redirect = View(model);

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, change to shouldLockout: true
                var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

                switch (result)
                {
                    case SignInStatus.Success:
                        redirect = RedirectToLocal(returnUrl);
                        break;
                    case SignInStatus.LockedOut:
                        redirect = View("Lockout");
                        break;
                    case SignInStatus.RequiresVerification:
                        redirect = RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                        break;
                    case SignInStatus.Failure: break;
                    default:
                        ModelState.AddModelError("", "Invalid login attempt.");
                        redirect = View(model);
                        break;
                }
            }

            return redirect;
        }


        /// <summary>
        /// Third party code verification
        /// </summary>
        /// <param name="provider">The provider name.</param>
        /// <param name="returnUrl">The redirect Url.</param>
        /// <param name="rememberMe">Remember the logged in user.</param>
        /// <returns> A web page redirect to the <paramref name="returnUrl"/> address. </returns>
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        /// <summary>
        /// Third party code verification.
        /// </summary>
        /// <param name="model">The user-provided code verification information. </param>
        /// <returns> A web page redirect to the appropriate page after code verification. </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        /// <summary>
        /// Register a new user.
        /// </summary>
        /// <returns>A web page redirect to the Index page.</returns>
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Register a new user.
        /// </summary>
        /// <param name="model"> The user-provided registration information. </param>
        /// <returns> A web page redirect to the appropriate url after user registration. </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    BlobManager blobManager = new BlobManager();
                    CloudBlobContainer container = blobManager.GetOrCreateBlobContainer(model.Email);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "PublicContent");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        /// <summary>
        /// This method currently does nothing without third party email support.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }


        /// <summary>
        /// This mehtod currently does nothing without third party verification.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }


        /// <summary>
        /// This method currently does nothing without third party verification.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        /// <summary>
        /// This method currently does nothing without third party verification.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        /// <summary>
        /// Default password reset view from two-factor auth code.
        /// </summary>
        /// <param name="code">Code provided from a two-factor auth provider.</param>
        /// <returns> A web page redirect to the appropriate page. </returns>
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }


        /// <summary>
        /// Reset user password.
        /// </summary>
        /// <param name="model"> User-provided credentials and password reset. </param>
        /// <returns> A web page redirect to the appropriate confirmation page. </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }


        /// <summary>
        /// Redirect to a page after password reset
        /// </summary>
        /// <returns> A web page redirect. </returns>
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        /// <summary>
        /// This method currently does nothing without third party verification.
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <param name="rememberMe"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }


        /// <summary>
        /// This method currently does nothing without third party verification.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }


        /// <summary>
        /// Log out.
        /// </summary>
        /// <returns> Web page redirect to the Public Content page. </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "PublicContent");
        }


        /// <summary>
        /// Clean up any application resources used by this controller.
        /// </summary>
        /// <param name="disposing">should dispose</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_UserManager != null)
                {
                    _UserManager.Dispose();
                    _UserManager = null;
                }

                if (_SignInManager != null)
                {
                    _SignInManager.Dispose();
                    _SignInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "PublicContent");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}