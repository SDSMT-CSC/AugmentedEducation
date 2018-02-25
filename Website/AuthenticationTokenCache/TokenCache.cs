using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace AuthenticationTokenCache
{
    public class TokenCache
    {
        #region Members

        private static TokenCache s_Instance = null;
        private Dictionary<string, DateTime> _TokenToExpirationTime;
        private Dictionary<string, Tuple<string, string>> _TokenToUser;

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


        #region Properties


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

        public bool ValidateToken(string token)
        {
            bool validated = false;
            DateTime expirationTime;
            string userName, password;
            Tuple<string, string> userValues;

            if(_TokenToUser.TryGetValue(token, out userValues))
            {
                userName = userValues.Item1;
                password = userValues.Item2;
                expirationTime = _TokenToExpirationTime[token];
                validated = (expirationTime < DateTime.UtcNow);
            }

            ClearExpiredTokens();

            return validated;
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

            foreach(string token in expiredTokens)
            {
                _TokenToUser.Remove(token);
                _TokenToExpirationTime.Remove(token);
            }
        }

        #endregion
    }
}
