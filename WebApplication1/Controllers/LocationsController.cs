using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data.SqlClient;

namespace WebApplication1.Controllers
{
    public class LocationsController : ApiController
    {
        public class LocationData
        {
            public int location_id;
            public string location_name;
            public double x_coordinate;
            public double y_coordinate;
            public string description;
            public string species;
        }

        public class LocationsList
        {
            public bool empty;
            public List<LocationData> LocationData;

            public LocationsList()
            {
                LocationData = new List<LocationData>();
            }
        }

        Database Database = new Database();

        [HttpGet]
        public LocationsList Get()
        {
            Database.Connect();
            SqlDataReader reader = Database.SimpleQuery(
                "SELECT g.location_id, g.location_name, g.x_coordinate, g.y_coordinate, g.description, a.species " +
                "FROM GPS_Locations as g LEFT JOIN Animal_Locations as a ON g.location_id = a.location_id"
            );

            LocationsList locations = new LocationsList();
            while (reader.Read())
            {
                LocationData data = new LocationData();
                data.location_id = int.Parse(reader["location_id"].ToString());
                data.location_name = reader["location_name"].ToString();
                data.x_coordinate = double.Parse(reader["x_coordinate"].ToString());
                data.y_coordinate = double.Parse(reader["y_coordinate"].ToString());
                data.description = reader["description"].ToString();
                data.species = reader["species"].ToString();
                locations.LocationData.Add(data);                
            }

            Database.Disconnect();
            return locations;
        }
    }
}
