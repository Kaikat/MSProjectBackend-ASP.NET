using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Configuration;
using System.Web.Script.Serialization;

namespace WebApplication1.Controllers
{
    public class AnimalsController : ApiController
    {
        public class AnimalDataInput
        {
            public string species;
            public string habitat_level;
            public float? min_age;
            public float? max_age;
            public float? min_weight;
            public float? max_weight;
            public float? min_height;
            public float? max_height;
        }

        public class AnimalData
        {
            public string species;
            public string name;
            public string nahuatl_name;
            public string spanish_name;
            public string description;
            public string habitat_level;
            public float min_size;
            public float max_size;
            public float min_age;
            public float max_age;
            public float min_weight;
            public float max_weight;
            public string colorkey_map_file;
        }

        public class AnimalList
        {
            public List<AnimalData> AnimalData;
            public AnimalList()
            {
                AnimalData = new List<AnimalData>();
            }
        }

        private Database Database = new Database();

        [HttpGet] //All Animals from the Database
        //public IEnumerable<AnimalData> Get()
        public AnimalList Get()
        {
            Database.Connect();
            SqlDataReader reader = Database.Query("SELECT * FROM Animals;");

            AnimalList AnimalList = new AnimalList();
            while (reader.Read())
            {
                AnimalData data = new AnimalData();
                data.species = reader["species"].ToString();
                data.name = reader["animal_name"].ToString();
                data.nahuatl_name = reader["nahuatl_name"].ToString();
                data.spanish_name = reader["spanish_name"].ToString();
                data.description = reader["description"].ToString();
                data.habitat_level = reader["habitat_level"].ToString();
                data.min_size = float.Parse(reader["min_height"].ToString());
                data.max_size = float.Parse(reader["max_height"].ToString());
                data.min_age = float.Parse(reader["min_age"].ToString());
                data.max_age = float.Parse(reader["max_age"].ToString());
                data.min_weight = float.Parse(reader["min_weight"].ToString());
                data.max_weight = float.Parse(reader["max_weight"].ToString());
                data.colorkey_map_file = reader["colorkey_map_file"].ToString();
                AnimalList.AnimalData.Add(data);
            }

            Database.Disconnect();
            return AnimalList;
        }

        [HttpPost]
        //This is for someone querying our database for animals with certain conditions
        public List<AnimalData> Put([FromBody] AnimalDataInput animal)
        {
            string[] columnNames = { "species", "habitat_level" };
            string[] valueNames = { "species", "habitatLevel" };
            bool[] hasValue = { animal.species != null, animal.habitat_level != null };

            StringBuilder queryBuilder = new StringBuilder(QueryBuilder.BuildSelectClause("Animals"));
            queryBuilder.Append(QueryBuilder.BuildWhereClauseFromSnippets(
                new string[] {
                    QueryBuilder.BuildWhereClause(columnNames, valueNames, hasValue),
                    QueryBuilder.BuildComparisonClause("age", animal.min_age == null ? null : "minAge", animal.max_age == null ? null : "maxAge"),
                    QueryBuilder.BuildComparisonClause("weight", animal.min_weight == null ? null : "minWeight", animal.max_weight == null ? null : "maxWeight"),
                    QueryBuilder.BuildComparisonClause("height", animal.min_height == null ? null : "minHeight", animal.max_height == null ? null : "maxHeight")
                }
            ));

            SqlCommand query = new SqlCommand(queryBuilder.ToString());
            query.AddParameter("@species", animal.species);
            query.AddParameter("@habitatLevel", animal.habitat_level);
            query.AddParameter("@minAge", animal.min_age);
            query.AddParameter("@maxAge", animal.max_age);
            query.AddParameter("@minWeight", animal.min_weight);
            query.AddParameter("@maxWeight", animal.max_weight);
            query.AddParameter("@minHeight", animal.min_height);
            query.AddParameter("@maxHeight", animal.max_height);

            AnimalData animalData = new AnimalData();
            animalData.species = query.GetQueryText();
            List<AnimalData> animals = new List<AnimalData>();
            animals.Add(animalData);
            return animals;
            //return Enumerable.Empty<AnimalData>();
        }
    }
}
