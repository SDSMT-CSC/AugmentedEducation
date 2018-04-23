//System .dll's
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// The model classes that contain data representation objects for passing information
/// between the web .cshtml code and the backend server code.
/// </summary>
namespace ARFE.Models
{
    /// <summary>
    /// A model for sending a two-factor authentication message through a registered third party
    /// two-factor authentication provider.
    /// </summary>
    public class SendCodeViewModel
    {
        /// <summary> The name of the user-seleted third party two-factor authentication provider. </summary>
        public string SelectedProvider { get; set; }

        /// <summary> The list of available third party two-factor authentication providers. </summary>
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }

        /// <summary> 
        /// The redirection Url to send the user to after having verified their acount information
        /// via the two-factor authentication code.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary> The user's preference to have their account automatically verified, when possible. </summary>
        public bool RememberMe { get; set; }
    }


    /// <summary>
    /// A model for verifying a two-factor authentication code that has been sent to a user.
    /// </summary>
    public class VerifyCodeViewModel
    {
        /// <summary> The name of the user-seleted third party two-factor authentication provider. </summary>
        [Required]
        public string Provider { get; set; }

        /// <summary> The two-factor authentication code that had been sent to the user. </summary>
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        /// <summary> 
        /// The redirection Url to send the user to after having verified their acount information
        /// via the two-factor authentication code.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary> 
        /// The user's preference to have their account automatically verified, when possible,
        /// by the browser that the user is currently using to browse the website.
        /// </summary>
        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        /// <summary> The user's preference to have their account automatically verified, when possible. </summary>
        public bool RememberMe { get; set; }
    }


    /// <summary>
    /// A model for the user to log in to the website.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary> The user's email account that has been registered with their account. </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary> The user's acount password. </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }


        /// <summary> If the user wants their sign-in information to be automatically verified. </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    
    /// <summary>
    /// A model for registering a new user.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary> The user's email that they wish to register with their account. </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// The user's account password.
        /// </summary>
        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 4)]
        public string Password { get; set; }

        /// <summary>
        /// The user's account password, entered a second time to verify that the <see cref="Password"/> 
        /// field was entered corretly.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }


    /// <summary>
    /// A model to reset a user's password.
    /// </summary>
    public class ResetPasswordViewModel
    {
        /// <summary> The user's email address that has been registered with their account. </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary> The user's new password that they wish to set. </summary>
        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 4)]
        public string Password { get; set; }

        /// <summary>
        /// The user's new account password, entered a second time to verify that the <see cref="Password"/> 
        /// field was entered corretly.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// The two-factor authentication code that had been sent to the user that will
        /// allow them to reset their password without knowing their old password.
        /// </summary>
        public string Code { get; set; }
    }


    /// <summary>
    /// A model for the user requesting a password reset via their registered email address.
    /// </summary>
    public class ForgotPasswordViewModel
    {
        /// <summary> The user's registered email address. </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
