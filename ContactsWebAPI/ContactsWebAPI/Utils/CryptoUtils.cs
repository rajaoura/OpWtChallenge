using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ContactsWebAPI.Utils
{
    public class CryptoUtils
    {
        public static  string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        public static string GetTokenClaimsInfo(string authToken,Func<IList<Claim>,string> getInfoFromClaims)
        {
            var handler = new JwtSecurityTokenHandler(); ;
            //remove bearer prefix 
            var tokenStr = authToken.Split(' ')[1];
            var token = handler.ReadJwtToken(tokenStr);  //mettre un bon Func pour faire genre C#
            return  getInfoFromClaims(token.Claims.ToList<Claim>());            
        }
    }
}
