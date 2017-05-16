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


namespace WebApplication1.Controllers
{
   /* public class Animal
    {
        public string species;
        public int size;
    }*/
    public class ValuesController : ApiController
    {
        Dictionary<int, string> values = new Dictionary<int, string>();
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Tamuyal"].ConnectionString;
           
            SqlConnection connection;
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT COUNT(*) FROM Animals");
                String sql = sb.ToString();
                string result = "yay?";
                SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Animals", connection);

                try
                {
                    SqlDataReader reader = command.ExecuteReader();
                    /*while(reader.Read())
                    {
                        result = reader.GetString(0);
                    }*/
                    reader.Read();
                    result = reader.GetInt32(0).ToString();
                }
                catch
                {
                    return "EXECUTE FAILED";
                }
                /*using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = reader.GetString(0);
                    }
                }
            connection.Close();
            return result;*/
                return result ;
            }
            catch
            {
                return "CONNECTION FAILED";
            }
        }

        public int Get(int id, int id2)
        {
            return id + id2;
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        /*public string Put(int id, [FromBody]string value)
        {
            return value;
        }*/

       /* [HttpPut]
        public int Put(int id, [FromBody]Animal animal)
        {
            return animal.size;
        }*/
        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
