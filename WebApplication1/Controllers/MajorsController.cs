using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data.SqlClient;

namespace WebApplication1.Controllers
{
    public class MajorsController : ApiController
    {
        public class MajorEntryData
        {
            public string major;
            public string major_name;
            public string description;
        }

        public class MajorEntriesResponse
        {
            public bool empty;
            public List<MajorEntryData> MajorEntries;

            public MajorEntriesResponse()
            {
                MajorEntries = new List<MajorEntryData>();
                empty = true;
            }
        }

        private Database Database = new Database();
        
        [HttpGet]
        public MajorEntriesResponse Get()
        {
            Database.Connect();
            SqlDataReader reader = Database.Query(
                "SELECT m.major, m.major_name, m.major_description " +
                "FROM Major_Descriptions as m"
            );

            MajorEntriesResponse majors = new MajorEntriesResponse();
            while (reader.Read())
            {
                MajorEntryData data = new MajorEntryData();
                data.major = reader["major"].ToString();
                data.major_name = reader["major_name"].ToString();
                data.description = reader["major_description"].ToString();
                majors.MajorEntries.Add(data);
                majors.empty = false;
            }

            Database.Disconnect();
            return majors;
        }
    }
}