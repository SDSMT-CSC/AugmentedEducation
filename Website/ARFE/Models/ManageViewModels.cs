//System .dll's
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

//NuGet
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;

/// <summary>
/// The model classes that contain data representation objects for passing information
/// between the web .cshtml code and the backend server code.
/// </summary>
namespace ARFE.Models
{
    /// <summary>
    /// A model for returning user login and verification.
    /// </summary>
    public class IndexViewModel
    {
        /// <summary> A property for whether the returning site visitor has a registered password. </summary>
        public bool HasPassword { get; set; }

        /// <summary> A property for the list of returning site visitor log in records. </summary>
        public IList<UserLoginInfo> Logins { get; set; }

        /// <summary> A property for the returning site visitor's registered phone number. </summary>
        public string PhoneNumber { get; set; }

        /// <summary> A property for if the returning site visitor turned on two-factor authentication. </summary>
        public bool TwoFactor { get; set; }

        /// <summary> A property for if the returning site visitor wants their browser usage remembered. </summary>
        public bool BrowserRemembered { get; set; }
    }


    /// <summary>
    /// A model for viewing and reporting current and past user login information.
    /// </summary>
    public class ManageLoginsViewModel
    {
        /// <summary> A property for the list of currently logged in users. </summary>
        public IList<UserLoginInfo> CurrentLogins { get; set; }

        /// <summary> a property for the list of previously logged in users. </summary>
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }


    /// <summary>
    /// A model for registering a user's password upon account creation.
    /// </summary>
    public class SetPasswordViewModel
    {
        /// <summary> The property field for the user's new password that they wish to set. </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string NewPassword { get; set; }

        /// <summary> 
        /// The property field for the confirmed re-entry of the user's new password that they wish to set. 
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }


    /// <summary>
    /// A model to change a user's registered account password.
    /// </summary>
    public class ChangePasswordViewModel
    {
        /// <summary> The property field for the user's current password that they are changing. </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        /// <summary> The property field for the user's new password that they wish to set. </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string NewPassword { get; set; }

        /// <summary> 
        /// The property field for the confirmed re-entry of the user's new password that they wish to set. 
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// A model for registering a phone number to a user acount.
    /// </summary>
    public class AddPhoneNumberViewModel
    {
        /// <summary> The user's newly registered phone number. </summary>
        [Phone]
        [Required]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

    
    /// <summary>
    /// A model for verifying a user's phone number via two-factor authentication.
    /// This model is currently not being used as two-factor authentication is not supported.
    /// However this serves as an extension point for supporting two-factor authentication at a later time.
    /// </summary>
    public class VerifyPhoneNumberViewModel
    {
        /// <summary> The code provided by two-factor authentication. </summary>
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        /// <summary> The phone number being validated. </summary>
        [Phone]
        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }


    /// <summary>
    /// A model for registering third party two-factor authentication providers.
    /// This model is currently not being used as two-factor authentication is not supported.
    /// However this serves as an extension point for supporting two-factor authentication at a later time.
    /// </summary>
    public class ConfigureTwoFactorViewModel
    {
        /// <summary> The name of the third party two-fator authentication provider. </summary>
        public string SelectedProvider { get; set; }

        /// <summary> 
        /// A List of third party two-fator authentication provider that can be 
        /// used as a combo-box for user selection on a web page UI.
        /// </summary>
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }
}