using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data.SqlClient;

namespace WebApplication1.Controllers
{
    public class PlayerAnimalsController : ApiController
    {
        Database Database = new Database();

        //TODO: Get Player Animals (caught/released/discovered)
        // UNTESTED
        [HttpGet] // All Animals for Player
        public AnimalList Get()
        {
            // Prepare query for database
            SqlCommand query = new SqlCommand("SELECT * FROM Animals WHERE username = @username;");
            SqlDataReader reader = Database.Query("SELECT * FROM Animals WHERE username = @username;");
            
            AnimalList PlayerAnimals = new AnimalList();
            return PlayerAnimals;
        }
    }
}
