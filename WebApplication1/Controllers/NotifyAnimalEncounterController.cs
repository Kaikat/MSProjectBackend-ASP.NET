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
    public class NotifyAnimalEncounterController : ApiController
    {
        Database Database = new Database();
        private const string ANIMAL_PREVIOUSLY_DISCOVERED = "Animal was already previously discovered by this user.";

        //TODO: Notify animal discovered/caught/released
        //http://tamuyal.azurewebsites.net/api/notifyanimalencounter?session_key=SESSION_KEY&encounter_type=discovered&species=SPECIES
        [HttpPost]
        public BasicResponse NotifyAnimalEncounter([FromUri]string session_key, string encounter_type, string species)
        {
            BasicResponse result = new BasicResponse();

            if (encounter_type == "discovered")
            {
                result.id = "animal discovery";
                if (AnimalPreviouslyDiscovered(session_key, species))
                {
                    result.message = ANIMAL_PREVIOUSLY_DISCOVERED;
                    return result;
                }

                SqlCommand query = new SqlCommand(
                    "INSERT INTO Discovered_Animals VALUES(" +
                        "(SELECT username FROM Sessions WHERE session_key = @sessionKey), " +
                        "@species, GETDATE());");
                query.Parameters.AddWithValue("@sessionKey", session_key);
                query.Parameters.AddWithValue("@species", species);
                Database.Connect();
                Database.Query(query);
                Database.Disconnect();
            }
             //TODO: If caught or released
             else
             {
                result.id = "caught";

             }
            result.error = false;
            return result;
        }

        private bool AnimalPreviouslyDiscovered(string session_key, string species)
        {
            SqlCommand query = new SqlCommand(
                "SELECT COUNT(*) FROM Discovered_Animals " +
                "INNER JOIN Sessions ON Sessions.username = Discovered_Animals.username " +
                "WHERE Sessions.session_key = @sessionKey AND Discovered_Animals.species = @species;");
            query.Parameters.AddWithValue("@sessionKey", session_key);
            query.Parameters.AddWithValue("@species", species);
            Database.Connect();
            SqlDataReader reader = Database.Query(query);
            reader.Read();
            bool result = reader.GetInt32(0) != 0;
            Database.Disconnect();
            return result;
        }
    }
}
