using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data.SqlClient;
using System.Security.Cryptography;

using Utilities;
namespace WebApplication1.Controllers
{
    public class LoginController : ApiController
    {
        public class LoginInfo
        {
            public string username;
            public string password;
        }

        Database Database = new Database();
        private const string INVALID_USERNAME_PASSWORD = "Invalid username or password";

        [HttpPost]
        public BasicResponse DecryptPassword([FromBody] LoginInfo login)
        {
            BasicResponse result = new BasicResponse();
            result.id = "session_id";

            SqlCommand query = new SqlCommand("SELECT * FROM Users WHERE username = @username;");
            query.Parameters.AddWithValue("@username", login.username);
            Database.Connect();
            SqlDataReader reader = Database.Query(query);

            //Invalid Username
            if (!reader.HasRows)
            {
                result.message = INVALID_USERNAME_PASSWORD;
                Database.Disconnect();
                return result;
            }

            while (reader.Read())
            {
                byte[] hashBytes = Convert.FromBase64String(reader["pass_hash"].ToString());
                byte[] salt = new byte[16];
                Array.Copy(hashBytes, 0, salt, 0, 16);
                var pbkdf2 = new Rfc2898DeriveBytes(login.password, salt, 10000);
                byte[] hash = pbkdf2.GetBytes(20);
                for (int i = 0; i < 20; i++)
                {
                    //Wrong Password
                    if (hashBytes[i + 16] != hash[i])
                    {
                        result.message = INVALID_USERNAME_PASSWORD;
                        Database.Disconnect();
                        return result;
                    }
                }

                byte[] sessionToken;
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                rng.GetBytes(sessionToken = new byte[16]);

                result.message = Convert.ToBase64String(sessionToken);
                result.error = false;
            }

            Database.Disconnect();
            return result;
        }
    }
}
