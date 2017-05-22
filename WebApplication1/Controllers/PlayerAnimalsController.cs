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
    public class PlayerAnimalsController : ApiController
    {
        Database Database = new Database();

        public class DiscoveredAnimal
        {
            public string animal_species;
            public string discovered_date;
        }

        public class DiscoveredSpecies
        {
            public List<DiscoveredAnimal> DiscoveredSpeciesData;
            public bool empty;

            public DiscoveredSpecies()
            {
                DiscoveredSpeciesData = new List<DiscoveredAnimal>();
                empty = true;
            }
        }

        public class OwnedAnimal
        {
            public int animal_id;
            public string animal_species;
            public string nickname;
            public float height;
            public float age;
            public float weight;
            public float health_1;
            public float health_2;
            public float health_3;
            public bool released;
        }

        public class OwnedAnimals
        {
            public List<OwnedAnimal> OwnedAnimalData;
            public bool empty;
            public OwnedAnimals()
            {
                OwnedAnimalData = new List<OwnedAnimal>();
                empty = true;
            }
        }

        [HttpGet]
        public object GetPlayerAnimals([FromUri]string session_key, string encounter_type)
        {
            switch(encounter_type)
            {
                case "discovered":
                    return GetPlayersDiscoveredAnimals(session_key);
                case "caught":
                    return GetPlayersAnimals(session_key, false);
                case "released":
                    return GetPlayersAnimals(session_key, true);
                default:
                    return new BasicResponse("player_animals", "invalid encounter type");
            }
        }

        private DiscoveredSpecies GetPlayersDiscoveredAnimals(string session_key)
        {
            SqlCommand query = new SqlCommand(
               "SELECT * FROM Discovered_Animals " +
               "INNER JOIN Sessions ON Sessions.username = Discovered_Animals.username " +
               "WHERE Sessions.session_key = @sessionKey;"
           );
            query.Parameters.AddWithValue("@sessionKey", session_key);
            Database.Connect();

            DiscoveredSpecies discoveredList = new DiscoveredSpecies();
            SqlDataReader reader = Database.Query(query);
            while (reader.Read())
            {
                DiscoveredAnimal animal = new DiscoveredAnimal();
                animal.animal_species = reader["species"].ToString();
                animal.discovered_date = reader["encounter_date"].ToString();
                discoveredList.DiscoveredSpeciesData.Add(animal);
                discoveredList.empty = false;
            }

            Database.Disconnect();
            return discoveredList;
        }

        private OwnedAnimals GetPlayersAnimals(string session_key, bool wasReleased)
        {
            SqlCommand query = new SqlCommand(
                "SELECT * FROM Owned_Animals " +
                "INNER JOIN Sessions ON Sessions.username = Owned_Animals.username " +
                "WHERE Sessions.session_key = @sessionKey AND released = @wasReleased"
            );
            query.Parameters.AddWithValue("@sessionKey", session_key);
            query.Parameters.AddWithValue("@wasReleased", wasReleased);

            OwnedAnimals animals = new OwnedAnimals();
            Database.Connect();
            SqlDataReader reader = Database.Query(query);
            while (reader.Read())
            {
                OwnedAnimal animal = new OwnedAnimal();
                animal.animal_id = reader["encounter_id"].ToInt();
                animal.animal_species = reader["species"].ToString();
                animal.nickname = reader["nickname"].ToString();
                animal.height = reader["height"].ToFloat();
                animal.age = reader["age"].ToFloat();
                animal.weight = reader["weight"].ToFloat();
                animal.health_1 = reader["health_1"].ToFloat();
                animal.health_2 = reader["health_2"].ToFloat();
                animal.health_3 = reader["health_3"].ToFloat();
                animal.released = wasReleased;
                animals.OwnedAnimalData.Add(animal);
                animals.empty = false;
            }

            Database.Disconnect();
            return animals;
        }
    }
}
