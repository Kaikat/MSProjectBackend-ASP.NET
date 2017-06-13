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

        public class DiscoveredSpeciesData
        {
            public string animal_species;
            public string discovered_date;
        }

        public class DiscoveredSpecies
        {
            public bool empty;
            public List<DiscoveredSpeciesData> DiscoveredSpeciesData;

            public DiscoveredSpecies()
            {
                DiscoveredSpeciesData = new List<DiscoveredSpeciesData>();
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
        }

        public class OwnedAnimals
        {
            public bool empty;
            public List<OwnedAnimal> OwnedAnimalData;
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
                    return GetPlayersCaughtAnimals(session_key);
                case "released":
                    return GetPlayersReleasedAnimals(session_key);
                default:
                    return new BasicResponse("player_animals", "invalid encounter type: " + encounter_type);
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
                DiscoveredSpeciesData animal = new DiscoveredSpeciesData();
                animal.animal_species = reader["species"].ToString();
                animal.discovered_date = reader["encounter_date"].ToString();
                discoveredList.DiscoveredSpeciesData.Add(animal);
                discoveredList.empty = false;
            }

            Database.Disconnect();
            return discoveredList;
        }

        private OwnedAnimals GetPlayersCaughtAnimals(string session_key)
        {
           OwnedAnimals animals = new OwnedAnimals();
            SqlCommand query = new SqlCommand(
                "SELECT a.encounter_id, b.species, b.nickname, a.height, a.age, a.weight, " +
                "a.health_1, a.health_2, a.biomagnification, a.encounter_date, Released_Animals.release_date " +
                "FROM Animal_History as a " +
                "INNER JOIN Sessions ON Sessions.username = a.username " +
                "INNER JOIN Player_Animals as b ON b.username = a.username and b.encounter_id = a.encounter_id " +
                "INNER JOIN(SELECT encounter_id, min(encounter_date) as caught_date " +
                    "FROM Animal_History as suba " +
                    "INNER JOIN Sessions on Sessions.username = suba.username " +
                    "WHERE Sessions.session_key = @sessionKey " +
                    "GROUP BY encounter_id) as mindate " +
                "ON mindate.encounter_id = a.encounter_id AND mindate.caught_date = a.encounter_date " +
                "LEFT JOIN Released_Animals ON Released_Animals.username = a.username and " +
                "Released_Animals.encounter_id = a.encounter_id " +
                "WHERE Sessions.session_key = @sessionKey and Released_Animals.encounter_id is null"
            );
            query.Parameters.AddWithValue("@sessionKey", session_key);
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
                animal.health_3 = reader["biomagnification"].ToFloat();
                animals.OwnedAnimalData.Add(animal);
                animals.empty = false;
            }

            Database.Disconnect();
            return animals;
        }

        private OwnedAnimals GetPlayersReleasedAnimals(string session_key)
        {
            OwnedAnimals animals = new OwnedAnimals();
            SqlCommand query = new SqlCommand(
                "SELECT a.encounter_id, b.species, b.nickname, a.height, a.age, a.weight, " +
                "a.health_1, a.health_2, a.biomagnification, a.encounter_date, Released_Animals.release_date " +
                "FROM Animal_History as a " +
                "INNER JOIN Sessions ON Sessions.username = a.username " +
                "INNER JOIN Player_Animals as b ON b.encounter_id = a.encounter_id and a.username = b.username " +
                "INNER JOIN(SELECT encounter_id, max(encounter_date) as release_date " +
                    "FROM Animal_History as suba " +
                    "INNER JOIN Sessions on Sessions.username = suba.username " +
                    "WHERE Sessions.session_key = @sessionKey " +
                    "GROUP BY encounter_id) as maxdate " +
                "ON maxdate.encounter_id = a.encounter_id AND maxdate.release_date = a.encounter_date " +
                "LEFT JOIN Released_Animals ON Released_Animals.username = a.username and " +
                "Released_Animals.encounter_id = a.encounter_id and Released_Animals.release_date = maxdate.release_date " +
                "WHERE Sessions.session_key = @sessionKey and Released_Animals.encounter_id is not null"
            );
            query.Parameters.AddWithValue("@sessionKey", session_key);
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
                animal.health_3 = reader["biomagnification"].ToFloat();
                animals.OwnedAnimalData.Add(animal);
                animals.empty = false;
            }

            Database.Disconnect();
            return animals;
        }
    }
}
