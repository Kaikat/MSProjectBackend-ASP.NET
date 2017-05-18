using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Net;
using System.Data.SqlClient;

namespace WebApplication1.Controllers
{
    public class Database
    {
        private const string XML_CONNECTION_STRING_NAME = "Tamuyal";

        public SqlConnection Connection { private set; get; }

        public void Connect()
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[XML_CONNECTION_STRING_NAME].ConnectionString;
            Connection = new SqlConnection(connectionString);
            Connection.Open();
        }

        public SqlDataReader Query(string query)
        {
            SqlCommand command = new SqlCommand(query, Connection);
            return command.ExecuteReader();
        }

        public SqlDataReader Query(SqlCommand command)
        {
            command.Connection = Connection;
            return command.ExecuteReader();
        }

        public void Disconnect()
        {
            Connection.Close();
        }
    }
}