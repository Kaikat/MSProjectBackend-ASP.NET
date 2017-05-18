using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

using  Utilities;
namespace WebApplication1.Controllers
{
    public class CreateAccountController : ApiController
    {
        public class AccountDetails
        {
            public string username;
            public string name;
            public string password;
            public string email;
        }

        public class EncryptedPasswordPair
        {
            public string HashedPassword { private set; get; }
            public string Salt { private set; get; }

            public EncryptedPasswordPair(string hashedPassword, string salt)
            {
                HashedPassword = hashedPassword;
                Salt = salt;
            }
        }

        private Database Database = new Database();
        private const int Currency = 1000;
        private const string USER_EXISTS_ERROR = "A player with that username already exists. Please select a different username.";
        private const string EMAIL_EXISTS_ERROR = "An account with that email already exists!";

        [HttpPost]
        public BasicResponse AttemptToCreateAccount([FromBody] AccountDetails details)
        {
            BasicResponse result = new BasicResponse();
            result.id = "account_creation";
            
            result.error = !isUnique("username", details.username);
            if (result.error)
            {
                result.message = USER_EXISTS_ERROR;
                return result;
            }

            result.error = !isUnique("email", details.email);
            if (result.error)
            {
                result.message = EMAIL_EXISTS_ERROR;
                return result;
            }
            
            //Create the account
            EncryptedPasswordPair passwordSet = HashPassword(details.password);

            string avatar = "lena";
            SqlCommand queryCreateAccount = new SqlCommand("INSERT INTO Users values(@username, @name, @hash, @salt, @email, @currency, @avatar);");
            queryCreateAccount.Parameters.AddWithValue("@username", details.username);
            queryCreateAccount.Parameters.AddWithValue("@name", details.name);
            queryCreateAccount.Parameters.AddWithValue("@hash", passwordSet.HashedPassword);
            queryCreateAccount.Parameters.AddWithValue("@salt", passwordSet.Salt);
            queryCreateAccount.Parameters.AddWithValue("@email", details.email);
            queryCreateAccount.Parameters.AddWithValue("@currency", Currency);
            queryCreateAccount.Parameters.AddWithValue("@avatar", avatar);

            Database.Connect();
            Database.Query(queryCreateAccount);
            Database.Disconnect();

            result.error = false;
            result.message = details.username;
            return result;
        }

        //Helper functions
        private bool isUnique(string item, string value)
        {
            bool unique = true;
            string parameter = "@" + item;
            StringBuilder query = new StringBuilder("SELECT COUNT(*) FROM Users WHERE ");
            query.Append(item);
            query.Append(" = ");
            query.Append(parameter);
            query.Append(";");

            SqlCommand command = new SqlCommand(query.ToString());
            command.Parameters.AddWithValue(parameter, value);
            Database.Connect();
            SqlDataReader reader = Database.Query(command);
            reader.Read();
            if(reader.GetInt32(0) != 0)
            {
                unique = false;
            }
            Database.Disconnect();
            return unique;
        }

        private EncryptedPasswordPair HashPassword(string password)
        {
            byte[] salt;
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(salt = new byte[16]);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            
            return new EncryptedPasswordPair(Convert.ToBase64String(hashBytes), Convert.ToBase64String(salt));
        }
    }
}



/* byte[] salt;
             RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
             rng.GetBytes(salt = new byte[16]);
             var pbkdf2 = new Rfc2898DeriveBytes(details.password, salt, 10000);
             byte[] hash = pbkdf2.GetBytes(20);
             byte[] hashBytes = new byte[36];
             Array.Copy(salt, 0, hashBytes, 0, 16);
             Array.Copy(hash, 0, hashBytes, 16, 20);
             string hashedPassword = Convert.ToBase64String(hashBytes);*/
