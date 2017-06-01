using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data.SqlClient;
using System.IO;

namespace WebApplication1.Controllers
{
    public class GenerateAnimalController : ApiController
    {
        private const string GEOG_IDEAS_AIRSTRIP_LINK = "http://aten.geog.ucsb.edu/Data/AirstripALL_table1.txt";
        private const int NUM_SENSOR_ROW_BYTES = 512;
        private const int NUM_SENSOR_ROW_ELEMENTS = 51;
        public class GennedAnimalData
        {
            public string animal_species;
            public int animal_id;
            public float health_1;
            public float health_2;
            public float health_3;
            public float size;
            public float weight;
            public float age;

            public GennedAnimalData(string species, float h1, float h2, float h3, float animal_size, float animal_weight, float animal_age)
            {
                animal_species = species;
                health_1 = h1;
                health_2 = h2;
                health_3 = h3;
                size = animal_size;
                weight = animal_weight;
                age = animal_age;
            }
        }

        Database Database = new Database();
        Random Random = new Random();

        //http://tamuyal.azurewebsites.net/api/generateanimal?session_key=I9l5PCI5FNpFlDUlvMgB7w==&species=Tiger
        [HttpGet]
        public GennedAnimalData GenerateAnimal([FromUri]string session_key, string species)
        {
            SqlCommand query = new SqlCommand(
                "SELECT min_height, max_height, min_age, max_age, " +
                "min_weight, max_weight FROM Animals WHERE species = @species;");
            query.Parameters.AddWithValue("@species", species);
            Database.Connect();
            SqlDataReader reader = Database.Query(query);
            reader.Read();

            GennedAnimalData animal = new GennedAnimalData(species,
                CalculateHealthFactor1(species), CalculateHealthFactor2(species), CalculateBiomagnificationFactor(species),
                RandomizeInRange(float.Parse(reader["min_height"].ToString()), float.Parse(reader["max_height"].ToString())),
                RandomizeInRange(float.Parse(reader["min_weight"].ToString()), float.Parse(reader["max_weight"].ToString())),
                RandomizeInRange(float.Parse(reader["min_age"].ToString()), float.Parse(reader["max_age"].ToString())));
            Database.Disconnect();
            animal.animal_id = CalculateAnimalID(session_key);
            return animal;
        }

        private int CalculateAnimalID(string sessionKey)
        {
            SqlCommand query = new SqlCommand(
                "SELECT MAX(encounter_id) as animal_count FROM Animal_History " +
                "INNER JOIN Sessions ON Sessions.username = Animal_History.username " +
                "WHERE Sessions.session_key = @session_key");
            query.Parameters.AddWithValue("@session_key", sessionKey);
            Database.Connect();
            SqlDataReader reader = Database.Query(query);
            reader.Read();
            string animalCount = reader["animal_count"].ToString();
            int animalID = animalCount == "" ? 1 : Int32.Parse(reader["animal_count"].ToString()) + 1;
            Database.Disconnect();
            return animalID;
        }

        //TODO: Andrew and Kevin
        //      Use data from sensors
        private float RandomizeInRange(float minimum, float maximum)
        {
            float randomNumber = (float)Random.Next((int)minimum, (int)maximum + 1) / maximum;
            randomNumber *= (maximum - minimum);
            randomNumber += minimum;
            randomNumber = (float)Math.Floor(randomNumber * 100.0f) / 100.0f;
            return randomNumber;
        }

        private float CalculateHealthFactor1(string species)
        {
            return Random.Next(0, 50) / 50.0f;
        }

        private float CalculateHealthFactor2(string species)
        {
            return Random.Next(0, 100) / 100.0f;
        }

        private float CalculateBiomagnificationFactor(string species)
        {
            return Random.Next(0, 200) / 200.0f;
        }

        // Calculate health factor using linear model on sensor data, where last row of online text file is pulled
        // http://aten.geog.ucsb.edu/Data/AirstripALL_table1.txt
        // the post is temporary -- for testing
        [HttpPost]
        public float CalculateHealthFactorFromSensor()
        {
            // Get length of file
            int contentLength = 0;

            WebRequest lengthRequest = WebRequest.Create(GEOG_IDEAS_AIRSTRIP_LINK);
            lengthRequest.Method = "HEAD";
            using (WebResponse resp = lengthRequest.GetResponse())
            {
                int.TryParse(resp.Headers.Get("Content-Length"), out contentLength);
            }

            // If successful, try to get last row of data
            List<string> lastRowArr;
            HttpWebRequest rowRequest = WebRequest.Create(GEOG_IDEAS_AIRSTRIP_LINK) as HttpWebRequest;
            rowRequest.AddRange(contentLength - NUM_SENSOR_ROW_BYTES, contentLength - 1);
            using (WebResponse resp = rowRequest.GetResponse())
            {
                StreamReader readStream = new StreamReader(resp.GetResponseStream());
                string read = readStream.ReadToEnd();
                List<string> rows = new List<string>(read.Split(new string[] { "\r\n" }, StringSplitOptions.None));
                rows.RemoveAll(item => item.Length == 0);
                lastRowArr = new List<string>(rows.Last().Split(','));
            }

            // If row was successfully parsed, generate number from it
            if (lastRowArr.Count == NUM_SENSOR_ROW_ELEMENTS)
            {
                List<float> explanatoryVars = new List<float> { 1.0f, 2.0f, 3.0f };
            }
            else
            {
                return Random.Next(0, 100) / 100.0f;
            }
        }
    }
}
