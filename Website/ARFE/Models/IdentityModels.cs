//System .dll's
using System.Security.Claims;
using System.Threading.Tasks;

//NuGet
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;


/// <summary>
/// The model classes that contain data representation objects for passing information
/// between the web .cshtml code and the backend server code.
/// </summary>
namespace ARFE.Models
{
    /// <summary>
    /// This class holds the profile information for a user within the appliation.
    /// Profile data for the user can be added by adding more properties to the ApplicationUser 
    /// class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Create and register new users within the application and store their credentials.
        /// </summary>
        /// <param name="manager">The class object used for user state interaction and representation.</param>
        /// <returns>A new claim identity for the newly created user.</returns>
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }


    /// <summary>
    /// This class inherits from and can override the Identity default Db that
    /// is automatically generated from the <see cref="ApplicationUser"/> class.
    /// If additionaly database fields are needed for the application database tables,
    /// they can be added here.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Default constructor that creates an instance from the DefaultConnection of the Identity
        /// registered Db.
        /// </summary>
        public ApplicationDbContext() : base("DefaultConnection", throwIfV1Schema: false) { }

        /// <summary>
        /// A statically available method to generate a new default instance of this class.
        /// </summary>
        /// <returns>A new default instance of this class.</returns>
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}