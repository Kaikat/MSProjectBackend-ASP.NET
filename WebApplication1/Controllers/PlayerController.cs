using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data.SqlClient;
using Utilities;
namespace WebApplication1.Controllers
{
    public class PlayerController : ApiController
    {
        public class PlayerData
        {
            public string name;
            public string avatar;
            public int currency;

            public PlayerData(string playerName, string playerAvatar, int playerCurrency)
            {
                name = playerName;
                avatar = playerAvatar;
                currency = playerCurrency;
            }
        }

        Database Database = new Database();

        [HttpGet]
        //http://tamuyal.azurewebsites.net/api/player?session_key=SessionKey (replace SessionKey with valid session key)
        public PlayerData PlayerBasicData([FromUri]string session_key)
        {
            SqlCommand query = new SqlCommand(
                "SELECT Users.name, Users.avatar, Users.currency FROM Users " +
                "INNER JOIN Sessions ON Sessions.username = Users.username " +
                "WHERE Sessions.session_key = @sessionKey;");
            query.Parameters.AddWithValue("@sessionKey", session_key);
            Database.Connect();
            SqlDataReader reader = Database.Query(query);
            reader.Read();
            PlayerData playerData = new PlayerData(reader["name"].ToString(),
                reader["avatar"].ToString(), Int32.Parse(reader["currency"].ToString()));
            Database.Disconnect();

            return playerData;
        }

        [HttpGet]
        public BasicResponse UpdateAvatar([FromUri]string session_key, string avatar)
        {
            BasicResponse response = new BasicResponse("updateAvatar");
            SqlCommand query = new SqlCommand(
                "UPDATE Users SET avatar = @avatar " +
                "FROM Users " +
                "INNER JOIN Sessions ON Users.username = Sessions.username " +
                "WHERE Sessions.session_key = @sessionKey"
            );
            query.Parameters.AddWithValue("@avatar", avatar);
            query.Parameters.AddWithValue("@sessionKey", session_key);
            Database.Connect();
            Database.Query(query);
            Database.Disconnect();
            response.message = "Set player avatar to " + avatar;
            response.error = false;
            return response;
        }
    }
}
