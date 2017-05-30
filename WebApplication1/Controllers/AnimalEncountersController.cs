using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data.SqlClient;

namespace WebApplication1.Controllers
{
    public class AnimalEncountersController : ApiController
    {
        public class AnimalEncounter
        {
            public int animal_id;
            public string species;
            public string encounter_type;
            public string caught_date;
            public float caught_health_1;
            public float caught_health_2;
            public float caught_health_3;
            public string released_date;
            public float released_health_1;
            public float released_health_2;
            public float released_health_3;
        }

        public class JournalEntries
        {
            public bool empty;
            public List<AnimalEncounter> JournalEntryData;

            public JournalEntries()
            {
                JournalEntryData = new List<AnimalEncounter>();
                empty = true;
            }
        }

        Database Database = new Database();

        [HttpGet]
        public object GetAnimalEncounters([FromUri]string session_key)
        {
            try
            {
                JournalEntries entries = new JournalEntries();
                SqlCommand query = new SqlCommand(
                   "SELECT a.username, a.encounter_id, b.species, " +
                   "(CASE WHEN Released_Animals.release_date is null THEN 'Caught' ELSE 'Released' END) as encounter_type, " +
                   "mindate.caught_date, a.health_1 as caught_health1, a.health_2 as caught_health2, a.biomagnification as caught_biomagnification, " +
                   "released_data.release_date, released_data.health_1 as released_health1, released_data.health_2 as released_health2, " +
                   "released_data.biomagnification as released_biomagnification " +

                   "FROM Animal_History as a " +
                   "INNER JOIN Sessions ON Sessions.username = a.username " +
                   "INNER JOIN Player_Animals as b ON b.encounter_id = a.encounter_id and a.username = b.username " +

                   "INNER JOIN(SELECT encounter_id, min(encounter_date) as caught_date " +
                      "FROM Animal_History as suba " +
                      "INNER JOIN Sessions on Sessions.username = suba.username " +
                      "WHERE Sessions.session_key = @sessionKey " +
                      "GROUP BY encounter_id) as mindate " +
                   "ON mindate.encounter_id = a.encounter_id AND mindate.caught_date = a.encounter_date " +

                   "LEFT JOIN(SELECT released.username, released.encounter_id, released.release_date, " +
                   "Animal_History.health_1, Animal_History.health_2, Animal_History.biomagnification " +
                   "FROM Released_Animals as released " +
                   "INNER JOIN Animal_History ON Animal_History.username = released.username " +
                   "AND Animal_History.encounter_id = released.encounter_id AND Animal_History.encounter_date = released.release_date) as released_data " +
                   "ON released_data.encounter_id = a.encounter_id AND released_data.username = a.username " +


                   "LEFT JOIN Released_Animals ON Released_Animals.encounter_id = a.encounter_id and Released_Animals.username = a.username " +
                   "WHERE Sessions.session_key = @sessionKey " +
                   "ORDER BY " +
                   "CASE WHEN released_data.release_date is null THEN mindate.caught_date " +
                   "ELSE released_data.release_date END " +
                   "DESC"

                   /*"SELECT a.username, a.encounter_id, b.species, " +
                   "(CASE WHEN Released_Animals.release_date is null THEN 'Caught' ELSE 'Released' END) as encounter_type, " +
                   "mindate.caught_date, a.health_1 as caught_health1, a.health_2 as caught_health2, a.biomagnification as caught_biomagnification, " +
                   "released_data.release_date, released_data.health_1 as released_health1, released_data.health_2 as released_health2, " +
                   "released_data.biomagnification as released_biomagnification " +

                   "FROM Animal_History as a " +
                   "INNER JOIN Sessions ON Sessions.username = a.username " +
                   "INNER JOIN Player_Animals as b ON b.encounter_id = a.encounter_id and a.username = b.username " +

                   "INNER JOIN(SELECT encounter_id, min(encounter_date) as caught_date " +
                   "FROM Animal_History GROUP BY encounter_id) as mindate " +
                   "ON mindate.encounter_id = a.encounter_id AND mindate.caught_date = a.encounter_date " +

                   "LEFT JOIN(SELECT released.username, released.encounter_id, released.release_date, " +
                   "Animal_History.health_1, Animal_History.health_2, Animal_History.biomagnification " +
                   "FROM Released_Animals as released " +
                   "INNER JOIN Animal_History ON Animal_History.username = released.username " +
                   "AND Animal_History.encounter_id = released.encounter_id AND Animal_History.encounter_date = released.release_date) as released_data " +
                   "ON released_data.encounter_id = a.encounter_id AND released_data.username = a.username " +


                   "LEFT JOIN Released_Animals ON Released_Animals.encounter_id = a.encounter_id " +
                   "WHERE Sessions.session_key = @sessionKey " +
                   "ORDER BY " +
                   "CASE WHEN released_data.release_date is null THEN mindate.caught_date " +
                   "ELSE released_data.release_date END " +
                   "DESC "*/
                );
                query.Parameters.AddWithValue("@sessionKey", session_key);
                Database.Connect();
                SqlDataReader reader = Database.Query(query);

                //"SELECT a.username, a.encounter_id, b.species, " +
                //"(CASE WHEN Released_Animals.release_date is null THEN 'Caught' ELSE 'Released' END) as encounter_type, " +
                //"mindate.caught_date, a.health_1 as caught_health1, a.health_2 as caught_health2, a.biomagnification as caught_biomagnification, " +
                //"released_data.release_date, released_data.health_1 as released_health1, released_data.health_2 as released_health2, " +
                //"released_data.biomagnification as released_biomagnification 

                while (reader.Read())
                {
                    AnimalEncounter encounter = new AnimalEncounter();
                    encounter.animal_id = Int32.Parse(reader["encounter_id"].ToString());
                    encounter.species = reader["species"].ToString();
                    encounter.encounter_type = reader["encounter_type"].ToString();
                    encounter.caught_date = reader["caught_date"].ToString();
                    encounter.caught_health_1 = reader["caught_health1"].ToFloat();
                    encounter.caught_health_2 = reader["caught_health2"].ToFloat();
                    encounter.caught_health_3 = reader["caught_biomagnification"].ToFloat();
                    encounter.released_date = "";

                    if (encounter.encounter_type == "Released")
                    {
                        encounter.released_date = reader["release_date"].ToString();
                        encounter.released_health_1 = reader["released_health1"].ToFloat();
                        encounter.released_health_2 = reader["released_health2"].ToFloat();
                        encounter.released_health_3 = reader["released_biomagnification"].ToFloat();
                    }
                    entries.JournalEntryData.Add(encounter);
                    entries.empty = false;
                }

                Database.Disconnect();
                return entries;
            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message;
            }
            /*
            JournalEntries entries = new JournalEntries();
            SqlCommand query = new SqlCommand(
                @"SELECT a.encounter_id, a.species, a.encounter_type, 
                    a.health_1, a.health_2, a.health_3, a.encounter_date, 
                    c.encounter_date as caught_date, c.health_1 as c_health_1, 
                    c.health_2 as c_health_2, c.health_3 as c_health_3 
                FROM Animal_Encounters as a 
                LEFT JOIN Animal_Encounters as c 
                    ON a.encounter_id = c.encounter_id and a.username = c.username 
                    AND a.encounter_type = 'released' and c.encounter_type = 'caught', 
                (
                    SELECT encounter_id, max(encounter_date) as max_date 
                    FROM Animal_Encounters
            INNER JOIN Sessions on Sessions.username = Animal_Encounters.username
                    WHERE Sessions.session_key = @sessionKey
                    GROUP BY encounter_id
                ) as b 
                WHERE a.encounter_id = b.encounter_id and a.encounter_date = b.max_date 
                ORDER BY a.encounter_date desc "
            );

            query.Parameters.AddWithValue("@sessionKey", session_key);
            Database.Connect();
            SqlDataReader reader = Database.Query(query);

            while (reader.Read())
            {
                AnimalEncounter encounter = new AnimalEncounter();
                encounter.animal_id = Int32.Parse(reader["encounter_id"].ToString());
                encounter.species = reader["species"].ToString();
                encounter.encounter_type = reader["encounter_type"].ToString();
                encounter.health_1 = reader["health_1"].ToFloat();
                encounter.health_2 = reader["health_2"].ToFloat();
                encounter.health_3 = reader["health_3"].ToFloat();
                encounter.encounter_date = reader["encounter_date"].ToString();

                if (encounter.encounter_type == "released")
                {
                    encounter.caught_date = reader["caught_date"].ToString();
                    encounter.caught_health_1 = reader["c_health_1"].ToFloat();
                    encounter.caught_health_2 = reader["c_health_2"].ToFloat();
                    encounter.caught_health_3 = reader["c_health_3"].ToFloat();
                }
                entries.JournalEntryData.Add(encounter);
                entries.empty = false;
            }

            Database.Disconnect();
            return entries;
            */
        }
    }
}
