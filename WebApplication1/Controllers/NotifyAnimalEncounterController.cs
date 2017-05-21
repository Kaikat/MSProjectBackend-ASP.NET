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
            public string encounter_id;
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

            SqlCommand query = new SqlCommand(
                "INSERT INTO Discovered_Animals VALUES(" +
                    "(SELECT username FROM Sessions WHERE session_key = @sessionKey), " +
                    "@species, GETDATE());");
            query.Parameters.AddWithValue("@sessionKey", session_key);
            query.Parameters.AddWithValue("@species", species);
            Database.Connect();
            Database.Query(query);
            Database.Disconnect();
            result.error = false;
            return result;
        }

        private BasicResponse NotifyAnimalCaught(AnimalEncounterData animalData)
        {
            BasicResponse result = new BasicResponse(CAUGHT);
            SqlCommand query = new SqlCommand(
                "INSERT INTO Owned_Animals VALUES(" +
                    "(SELECT username FROM Sessions WHERE session_key = @sessionKey), " +
                "@encounter_id, @species, " +
                "@nickname, @height, @age, @weight, @health_1, @health_2, @health_3, 0);"
            );
            query.Parameters.AddWithValue("@sessionKey", animalData.session_key);
            query.Parameters.AddWithValue("@encounter_id", animalData.encounter_id);
            query.Parameters.AddWithValue("@species", animalData.species);
            query.Parameters.AddWithValue("@nickname", animalData.nickname);
            query.Parameters.AddWithValue("@height", animalData.height);
            query.Parameters.AddWithValue("@age", animalData.age);
            query.Parameters.AddWithValue("@weight", animalData.weight);
            query.Parameters.AddWithValue("@health_1", animalData.health1);
            query.Parameters.AddWithValue("@health_2", animalData.health2);
            query.Parameters.AddWithValue("@health_3", animalData.health3);
            Database.Connect();
            Database.Query(query);
            Database.Disconnect();

            SqlCommand insertQuery = new SqlCommand(
                "INSERT INTO Animal_Encounters VALUES(" +
                    "(SELECT username FROM Sessions WHERE session_key = @sessionKey), " +
                "@encounter_id, @species, @caught, @health1, @health2, @health3, GETDATE());"
            );
            insertQuery.Parameters.AddWithValue("@sessionKey", animalData.session_key);
            insertQuery.Parameters.AddWithValue("@encounter_id", animalData.encounter_id);
            insertQuery.Parameters.AddWithValue("@species", animalData.species);
            insertQuery.Parameters.AddWithValue("@caught", CAUGHT);
            insertQuery.Parameters.AddWithValue("@health1", animalData.health1);
            insertQuery.Parameters.AddWithValue("@health2", animalData.health2);
            insertQuery.Parameters.AddWithValue("@health3", animalData.health3);
            Database.Connect();
            Database.Query(insertQuery);
            Database.Disconnect();

            result.error = false;
            return result;
        }

        private BasicResponse NotifyAnimalReleased(AnimalEncounterData encounterData)
        {
            BasicResponse result = new BasicResponse(RELEASED);
            SqlCommand query = new SqlCommand(
                "UPDATE Owned_Animals SET released = 1 " +
                "WHERE encounter_id = @encounter_id AND username = " +
                    "(SELECT username FROM Sessions WHERE session_key = @sessionKey);"
            );
            query.Parameters.AddWithValue("@encounter_id", encounterData.encounter_id);
            query.Parameters.AddWithValue("@sessionKey", encounterData.session_key);
            Database.Connect();
            Database.Query(query);
            Database.Disconnect();

            SqlCommand insertQuery = new SqlCommand(
                "INSERT INTO Animal_Encounters VALUES(" +
                    "(SELECT username FROM Sessions WHERE session_key = @sessionKey), " +
                "@encounter_id, @species, @released, @health1, @health2, @health3, GETDATE());"
            );
            insertQuery.Parameters.AddWithValue("@sessionKey", encounterData.session_key);
            insertQuery.Parameters.AddWithValue("@encounter_id", encounterData.encounter_id);
            insertQuery.Parameters.AddWithValue("@species", encounterData.species);
            insertQuery.Parameters.AddWithValue("@released", RELEASED);
            insertQuery.Parameters.AddWithValue("@health1", encounterData.health1);
            insertQuery.Parameters.AddWithValue("@health2", encounterData.health2);
            insertQuery.Parameters.AddWithValue("@health3", encounterData.health3);
            Database.Connect();
            Database.Query(insertQuery);
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
