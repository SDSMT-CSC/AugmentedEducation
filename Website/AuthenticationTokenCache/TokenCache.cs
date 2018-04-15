using System;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// The namespace containing all code relating to the in memory cache used for
/// mobile authentication.
/// </summary>
namespace AuthenticationTokenCache
{
    /// <summary>
    /// A class to manage tokens that are associated to user login information.
    /// This is used for mobile user authentication by verifying tokens, that the user
    /// explicitly requests, to that user's log in credentials.  This system of token
    /// passing makes for simpler and more secure communication between the mobile 
    /// application and the web service.
    /// </summary>
    public class TokenCache
    {
        #region Members

        /// <summary>The singleton instance object.</summary>
        private static TokenCache s_Instance = null;
        /// <summary>The backing tracker for token expiration times.</summary>
        private Dictionary<string, DateTime> _TokenToExpirationTime = null;
        /// <summary>The backing tracker for tokens to user information.</summary>
        private Dictionary<string, Tuple<string,string>> _TokenToUser = null;

        #endregion


        #region Constructor

        /// <summary>
        /// A static initialization method that calls a private constructor internally
        /// to ensure a static single instance of this object is maintained throughout the
        /// run-time life of the application.
        /// </summary>
        /// <returns>The once-constructed instance of the TokenCache.</returns>
        public static TokenCache Init()
        {
            if (s_Instance == null)
            {
                s_Instance = new TokenCache();
            }

            return s_Instance;
        }

        /// <summary>
        /// A private constructor to ensure the static Init method is the only 
        /// way to retrieve on object of this type.
        /// </summary>
        private TokenCache()
        {
            //If service/site goes down, destroy all active tokens
            _TokenToExpirationTime = new Dictionary<string, DateTime>();
            _TokenToUser = new Dictionary<string, Tuple<string, string>>();
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Given a user's user name and password credentials, generate a 
        /// new authentication token associated with those credentials.
        /// The token will be valid for two hours.
        /// The users credentials are not validated at this point, they are only
        /// stored.  If incorrect credentials are supplied then the application
        /// will catch that internally via ASP.NET Identity.
        /// </summary>
        /// <param name="userName">The user provided user name.</param>
        /// <param name="password">The user provided password.</param>
        /// <returns>A new base64 encoded string of unique bytes.</returns>
        public string GenerateToken(string userName, string password)
        {
            byte[] key, timeStamp;
            string token = string.Empty;

            //ensure don't generate a token that's in use
            do
            {   //create token
                key = Guid.NewGuid().ToByteArray();
                timeStamp = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
                token = Convert.ToBase64String(timeStamp.Concat(key).ToArray());
            } while (_TokenToUser.ContainsKey(token));

            //cache token -> user info, cache token expiration time
            _TokenToUser.Add(token, new Tuple<string, string>(userName, password));
            _TokenToExpirationTime.Add(token, DateTime.UtcNow.AddHours(2));

            //lazily remove expired tokens
            ClearExpiredTokens();

            return token;
        }

        /// <summary>
        /// Verify that a token exists within the current state of the in-memory
        /// storage.  If it does and the token has yet to expire, return a tuple 
        /// of the associated user name and password that the token was created 
        /// with.  If it does not exist or the token has expired, the response is
        /// null.
        /// </summary>
        /// <param name="token">
        /// The randomized string provided by the <see cref="GenerateToken(string, string)"/> method.
        /// </param>
        /// <returns>
        /// <ul>
        ///     <li>A tuple of the associated user name and password - the token is valid.</li>
        ///     <li>Null - the token is invalid or expired.</li>
        /// </ul>
        /// </returns>
        public Tuple<string, string> ValidateToken(string token)
        {
            string userName = string.Empty;
            Tuple<string, string> userInfo;
            DateTime expirationTime = DateTime.UtcNow.AddDays(-1);

            if (_TokenToUser.TryGetValue(token, out userInfo))
            {
                expirationTime = _TokenToExpirationTime[token];
            }

            //lazily remove expired tokens
            ClearExpiredTokens();

            //hasn't expired yet : return token
            //else : null
            return (DateTime.UtcNow < expirationTime ? userInfo : null);
        }


        #endregion


        #region Private Methods

        /// <summary>
        /// An internal method to keep the cache size small during times
        /// of more frequent use.  Calls to this method are made "lazily" in 
        /// both the <see cref="GenerateToken(string, string)"/>, and the 
        /// <see cref="ValidateToken(string)"/> methods.  
        /// This method finds all cached tokens that exist past their listed expiration
        /// date and remove them and their associated values from both internal dictionaries.
        /// </summary>
        private void ClearExpiredTokens()
        {
            //all keys from dict where value is expired
            List<string> expiredTokens = _TokenToExpirationTime
                .Where(dt => dt.Value <= DateTime.UtcNow)
                .Select(t => t.Key)
                .ToList();

            foreach (string token in expiredTokens)
            {
                _TokenToUser.Remove(token);
                _TokenToExpirationTime.Remove(token);
            }
        }

        #endregion
    }
}
