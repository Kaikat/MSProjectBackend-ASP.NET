using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.SqlClient;
namespace WebApplication1.Controllers
{
    public static class ExtensionMethods
    {
        public static void AddParameter(this SqlCommand command, string parameterName, object value)
        {
            if (command.CommandText.Contains(parameterName))
            {
                command.Parameters.AddWithValue(parameterName, value);
            }
        }

        public static string GetQueryText(this SqlCommand command)
        {
            string query = command.CommandText;
            foreach (SqlParameter p in command.Parameters)
            {
                query = query.Replace(p.ParameterName, p.Value.ToString());
            }

            return query;
        }
    }
}