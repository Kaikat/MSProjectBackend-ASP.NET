using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace WebApplication1.Controllers
{
    public static class QueryBuilder
    {
        public static string BuildSelectClause(string tableName)
        {
            return $"SELECT * FROM {tableName}";
        }

        //tableColumns, valueNames, values must be the same length
        public static string Build(string tableName, string [] tableColumns, string [] valueNames, bool [] hasValue)
        {
            StringBuilder query = new StringBuilder("SELECT * FROM ");
            query.Append(tableName);

            return query.Append(BuildWhereClause(tableColumns, valueNames, hasValue)).ToString();
        }

        public static string BuildWhereClause(string [] tableColumns, string [] valueNames, bool [] hasValue)
        {
            bool appended = false;
            StringBuilder whereClause = new StringBuilder(" WHERE ");
            for (int i = 0; i < tableColumns.Length; i++)
            {
                if (hasValue[i])
                {
                    if (appended)
                    {
                        whereClause.Append(" AND ");
                    }

                    whereClause.Append(tableColumns[i] + " = @" + valueNames[i]);
                    appended = true;
                }
            }

            return appended ? whereClause.ToString() : "";
        }

        public static string BuildWhereClauseFromSnippets(string [] whereSnippets)
        {
            StringBuilder whereClause = new StringBuilder("");
            bool appended = false;
            for (int i = 0; i < whereSnippets.Length; i++)
            {
                if (whereSnippets[i] != null && whereSnippets[i] != string.Empty)
                {
                    if (appended)
                    {
                        whereClause.Append(" AND ");
                    }

                    if (!appended && !whereSnippets[i].Contains("WHERE"))
                    {
                        whereClause.Append(" WHERE ");
                    }

                    whereClause.Append(whereSnippets[i]);
                    appended = true;
                }
            }

            whereClause.Append(";");
            return appended ? whereClause.ToString() : "";
        }

        public static string BuildComparisonClause(string columnName, string minimumVariableName, string maximumVariableName)
        {
            StringBuilder comparisonClause = new StringBuilder("");
            string minimumClause = $"{columnName} >= @{minimumVariableName}";
            string maximumClause = $"{columnName} <= @{maximumVariableName}";

            if (minimumVariableName != null)
            {
                comparisonClause.Append(minimumClause);
            }
            if (minimumVariableName != null && maximumVariableName != null)
            {
                comparisonClause.Append(" AND ");
            }
            if (maximumVariableName != null)
            {
                comparisonClause.Append(maximumClause);
            }

            return comparisonClause.ToString();
        }
    }
}