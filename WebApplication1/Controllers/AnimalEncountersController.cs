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
            public float health_1;
            public float health_2;
            public float health_3;
            public string encounter_date;
            public string caught_date;
            public float caught_health_1;
            public float caught_health_2;
            public float caught_health_3;
        }

        public class JournalEntries
        {
            public List<AnimalEncounter> JournalEntryData;
            public bool empty;

            public JournalEntries()
            {
                JournalEntryData = new List<AnimalEncounter>();
                empty = true;
            }
        }

        Database Database = new Database();

        [HttpPost]
        public JournalEntries GetAnimalEncounters([FromUri]string session_key)
        {
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
        }
    }
}
