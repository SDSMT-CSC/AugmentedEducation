using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using AuthenticationTokenCache;

/// <summary>
/// The namespace containing all tests that pertain to the 
/// AuthenticationTokenCache.TokenCache class and it's methods.
/// </summary>
namespace AuthTokenCache.Tests
{
    /// <summary>
    /// The class containing all tests that pertain to the 
    /// AuthenticationTokenCache.TokenCache class and it's methods.
    /// </summary>
    [TestClass]
    public class TokenCacheTests
    {
        /// <summary>
        /// Test that the constructor of the TokenCache is in fact private.
        /// Calling Activator.CreateInstance will try to call the object constructor
        /// but will throw a MissingMethodException since one hasn't been made publicly 
        /// accessible due to the fact that it's a singleton cache. 
        /// </summary>
        [TestMethod]
        public void Test_PrivateConstructor()
        {
            TokenCache cache = null;
            Type cacheType = typeof(TokenCache);

            Assert.IsNull(cache);

            //assert couldn't create, still null
            Assert.ThrowsException<MissingMethodException>(() => {
                cache = (Activator.CreateInstance(cacheType) as TokenCache);
            });
            Assert.IsNull(cache);
        }

        /// <summary>
        /// Test that the publicly available static Init() method creates an instance
        /// of the TokenCache object.
        /// </summary>
        [TestMethod]
        public void Test_Init()
        {
            TokenCache cache = TokenCache.Init();

            Assert.IsNotNull(cache);
            Assert.IsInstanceOfType(cache, typeof(TokenCache));
        }

        /// <summary>
        /// Test that inserting user info to the cache creates a non-empty string 
        /// token.
        /// </summary>
        [TestMethod]
        public void Test_Insert()
        {
            TokenCache cache = TokenCache.Init();
            string token = cache.GenerateToken("user", "password");
            Assert.IsFalse(string.IsNullOrEmpty(token));
        }

        /// <summary>
        /// Test that the token produced for a users information is able to 
        /// successfully retrieve the same user information.
        /// </summary>
        [TestMethod]
        public void Test_ValidateSingle()
        {
            string userName = "user";
            string password = "password";
            TokenCache cache = TokenCache.Init();

            string token = cache.GenerateToken(userName, password);
            Assert.IsFalse(string.IsNullOrEmpty(token));

            //get user name password out by the token provided
            Tuple<string, string> user_pass = cache.ValidateToken(token);

            //assert valid user info
            Assert.IsNotNull(user_pass);
            Assert.IsTrue(user_pass.Item1 == userName);
            Assert.IsTrue(user_pass.Item2 == password);
        }

        /// <summary>
        /// Test that creating two separate variables of type TokenCache with the static
        /// Init() method sets each variable to the same static instance of the 
        /// TokenCache object.
        /// </summary>
        [TestMethod]
        public void Test_Singleton()
        {
            string user = "user";
            string pass = "password";
            TokenCache tokenCache_a = TokenCache.Init();
            TokenCache tokenCache_b = TokenCache.Init();
            Assert.IsTrue(ReferenceEquals(tokenCache_a, tokenCache_b));

            //get token for user info from instance a
            string token = tokenCache_a.GenerateToken(user, pass);
            //get user info back out by token validation in instance b
            Tuple<string, string> userInfo = tokenCache_b.ValidateToken(token);

            //make assertions that user info retrieved from b by token
            //received from a is the original user info put into a to create token
            Assert.IsNotNull(userInfo);
            Assert.IsTrue(userInfo.Item1 == user);
            Assert.IsTrue(userInfo.Item2 == pass);
        }

        /// <summary>
        /// Test that users information can be supplied but providing an invalid token 
        /// will not return any user's information.
        /// </summary>
        [TestMethod]
        public void Test_BadTokenReturnsNull()
        {
            string userName = "user";
            string password = "password";
            TokenCache cache = TokenCache.Init();

            string token = cache.GenerateToken(userName, password);
            Assert.IsFalse(string.IsNullOrEmpty(token));

            //attempting to get info with an invalid token gives null response
            Tuple<string, string> user_pass = cache.ValidateToken("Some other token");
            Assert.IsNull(user_pass);
        }

        /// <summary>
        /// Test that the cache successfully handles more than one user's
        /// information.
        /// </summary>
        [TestMethod]
        public void Test_ValidateMultiple()
        {
            TokenCache cache = TokenCache.Init();
            Dictionary<Tuple<string, string>, string> userInfo_Token = new Dictionary<Tuple<string, string>, string>();

            //100 unique UserName/Password/Tokens
            for (int userCount = 0; userCount < 100; userCount++)
            {
                string user = $"user-{userCount}";
                string pass = $"pass-{userCount}";
                string token = cache.GenerateToken(user, pass);
                Assert.IsFalse(string.IsNullOrEmpty(token));

                Tuple<string, string> userInfo = new Tuple<string, string>(user, pass);

                userInfo_Token.Add(userInfo, token);
            }

            //every token unique
            Assert.IsTrue(userInfo_Token.Values.ToList().Distinct().Count() == userInfo_Token.Count());

            foreach (string token in userInfo_Token.Values.Reverse().ToList())
            {
                //get username/pass info
                Tuple<string, string> fetchUserInfo = cache.ValidateToken(token);
                //return value matches creation pattern
                Assert.IsTrue(fetchUserInfo.Item1.StartsWith("user-"));
                Assert.IsTrue(fetchUserInfo.Item2.StartsWith("pass-"));
                //return tuple should be a key in the dictionary
                Assert.IsTrue(userInfo_Token.ContainsKey(fetchUserInfo));
                //the value of that key should be the token that the cache was queried with
                Assert.IsTrue(userInfo_Token[fetchUserInfo] == token);
            }
        }

        /// <summary>
        /// Test that a single user can get multiple tokens that will all remain valid.
        /// </summary>
        [TestMethod]
        public void Test_MultipleTokensToSameUser()
        {
            int maxTokens = 100;
            string user = "user";
            string pass = "pass";
            TokenCache cache = TokenCache.Init();
            List<string> tokens = new List<string>();

            for (int tokenCount = 0; tokenCount < maxTokens; tokenCount++)
            {
                //get several tokens all for the same user
                tokens.Add(cache.GenerateToken(user, pass));
            }

            //ensure all tokens were added
            Assert.IsTrue(tokens.Count == maxTokens);
            //ensure all tokens unique
            Assert.IsTrue(tokens.Distinct().Count() == tokens.Count);

            //loop over all tokens
            foreach (string token in tokens)
            {
                //set info null and get info for next token
                Tuple<string, string> userInfo = null;
                userInfo = cache.ValidateToken(token);

                //assert info matches user
                Assert.IsNotNull(userInfo);
                Assert.IsTrue(userInfo.Item1 == user);
                Assert.IsTrue(userInfo.Item2 == pass);
            }
        }
    }
}