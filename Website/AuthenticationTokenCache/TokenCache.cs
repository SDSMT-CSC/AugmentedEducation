using System;
using System.Linq;
using System.Collections.Generic;

namespace AuthenticationTokenCache
{
    public class TokenCache
    {
        #region Members

        private static TokenCache s_Instance = null;
        private Dictionary<string, DateTime> _TokenToExpirationTime = null;
        private Dictionary<string, Tuple<string,string>> _TokenToUser = null;

        #endregion


        #region Constructor

        public static TokenCache Init()
        {
            if (s_Instance == null)
            {
                s_Instance = new TokenCache();
            }

            return s_Instance;
        }

        private TokenCache()
        {
            //If service/site goes down, destroy all active tokens
            _TokenToExpirationTime = new Dictionary<string, DateTime>();
            _TokenToUser = new Dictionary<string, Tuple<string, string>>();
        }

        #endregion


        #region Public Methods

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

            _TokenToUser.Add(token, new Tuple<string, string>(userName, password));
            _TokenToExpirationTime.Add(token, DateTime.UtcNow.AddHours(2));
            ClearExpiredTokens();

            return token;
        }


        public Tuple<string, string> ValidateToken(string token)
        {
            string userName = string.Empty;
            Tuple<string, string> userInfo;
            DateTime expirationTime = DateTime.UtcNow.AddDays(-1);

            if (_TokenToUser.TryGetValue(token, out userInfo))
            {
                expirationTime = _TokenToExpirationTime[token];
            }

            ClearExpiredTokens();

            //hasn't expired yet : return token
            //else : null
            return (DateTime.UtcNow < expirationTime ? userInfo : null);
        }


        #endregion


        #region Private Methods

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
