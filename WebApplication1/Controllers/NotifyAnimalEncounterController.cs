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
        public class AnimalEncounterData
        {
            public string session_key;
            public string encounter_type;
            public string species;
            public int encounter_id;
            public string nickname;
            public float? age;
            public float? height;
            public float? weight;
            public float? health1;
            public float? health2;
            public float? health3;
        }

        private const string ANIMAL_PREVIOUSLY_DISCOVERED = "Animal was already previously discovered by this user.";
        private const string INVALID_ENCOUNTER_TYPE = "Invalid encounter type.";
        private const string DISCOVERED = "discovered";
        private const string CAUGHT = "caught";
        private const string RELEASED = "released";

        Database Database = new Database();

        [HttpPost]
        //http://tamuyal.azurewebsites.net/api/notifyanimalencounter
        public BasicResponse NotifyAnimalEncounter([FromBody] AnimalEncounterData encounterData)
        {
            switch (encounterData.encounter_type)
            {
                case DISCOVERED:
                    return NotifyAnimalDiscovered(encounterData.session_key, encounterData.species);
                case CAUGHT:
                    return NotifyAnimalCaught(encounterData);
                case RELEASED:
                    return NotifyAnimalReleased(encounterData);
                default:
                    return new BasicResponse("animal_encounter", INVALID_ENCOUNTER_TYPE);
            }
        }

        private BasicResponse NotifyAnimalDiscovered(string session_key, string species)
        {
            BasicResponse result = new BasicResponse("animal discovery");
            result.id = "animal discovery";
            if (AnimalPreviouslyDiscovered(session_key, species))
            {
                result.message = ANIMAL_PREVIOUSLY_DISCOVERED;
                return result;
            }

            string date = DateTime.Now.ToString();
            SqlCommand query = new SqlCommand(
                "INSERT INTO Discovered_Animals VALUES(" +
                    "(SELECT username FROM Sessions WHERE session_key = @sessionKey), " +
                    "@species, @date);"
            );
            query.Parameters.AddWithValue("@sessionKey", session_key);
            query.Parameters.AddWithValue("@species", species);
            query.Parameters.AddWithValue("@date", date);
            Database.Connect();
            Database.Query(query);
            Database.Disconnect();

            result.message = date;
            result.error = false;
            return result;
        }

        private BasicResponse NotifyAnimalCaught(AnimalEncounterData animalData)
        {
            BasicResponse result = new BasicResponse(CAUGHT);

            try
            {
                SqlCommand query = new SqlCommand(
                    "INSERT INTO Player_Animals VALUES(" +
                        "(SELECT username FROM Sessions WHERE session_key = @sessionKey), " +
                        "@encounter_id, @species, @nickname);");

                query.Parameters.AddWithValue("@sessionKey", animalData.session_key);
                query.Parameters.AddWithValue("@encounter_id", animalData.encounter_id);
                query.Parameters.AddWithValue("@species", animalData.species);
                query.Parameters.AddWithValue("@nickname", animalData.nickname);

                SqlCommand query2 = new SqlCommand(
                    "INSERT INTO Animal_History VALUES(" +
                        "(SELECT username FROM Sessions WHERE session_key = @sessionKey)," +
                        "@encounter_id, GETDATE(), @height, @age, @weight, " +
                        "@health_1, @health_2, @biomagnification);"
                );
                query2.Parameters.AddWithValue("@sessionKey", animalData.session_key);
                query2.Parameters.AddWithValue("@encounter_id", animalData.encounter_id);
                query2.Parameters.AddWithValue("@height", animalData.height);
                query2.Parameters.AddWithValue("@age", animalData.age);
                query2.Parameters.AddWithValue("@weight", animalData.weight);
                query2.Parameters.AddWithValue("@health_1", animalData.health1);
                query2.Parameters.AddWithValue("@health_2", animalData.health2);
                query2.Parameters.AddWithValue("@biomagnification", animalData.health3);
                Database.Connect();
                Database.Query(query);
                Database.Disconnect();
                Database.Connect();
                Database.Query(query2);
                Database.Disconnect();
            }
            catch
            {
                result.message = "ERROR: Animal ID already exists";
                return result;
            }
            
            result.error = false;
            return result;
        }

        private BasicResponse NotifyAnimalReleased(AnimalEncounterData encounterData)
        {
            BasicResponse result = new BasicResponse(RELEASED);
            SqlCommand query = new SqlCommand(
                "INSERT INTO Animal_History VALUES(" +
                    "(SELECT username FROM Sessions WHERE session_key = @sessionKey)," +
                    "@encounter_id, @date, @height, @age, @weight," +
                    "@health1, @health2, @biomagnification);" +
                "INSERT INTO Released_Animals VALUES(" +
                    "(SELECT username FROM Sessions WHERE session_key = @sessionKey)," +
                    "@encounter_id, @date);"
            );
            string date = DateTime.Now.ToString();
            query.Parameters.AddWithValue("@sessionKey", encounterData.session_key);
            query.Parameters.AddWithValue("@encounter_id", encounterData.encounter_id);
            query.Parameters.AddWithValue("@date", date);
            query.Parameters.AddWithValue("@height", encounterData.height);
            query.Parameters.AddWithValue("@age", encounterData.age);
            query.Parameters.AddWithValue("@weight", encounterData.weight);
            query.Parameters.AddWithValue("@health1", encounterData.health1);
            query.Parameters.AddWithValue("@health2", encounterData.health2);
            query.Parameters.AddWithValue("@biomagnification", encounterData.health3);
            Database.Connect();
            Database.Query(query);
            Database.Disconnect();
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
