using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data.SqlClient;
using System.IO;
using System.Web;
using Utilities;
using System.Text;

namespace WebApplication1.Controllers
{
    public class RecommendationController : ApiController
    {
        Database Database = new Database();
        const int TOP_X_MAJORS = 5;

        public class Interests
        {
            public Dictionary<Interest, float> Preference;
            public Interests()
            {
                Preference = new Dictionary<Interest, float>();
            }
        }

        public class MajorPreference
        {
            public string Major;
            public double Value;

            public MajorPreference(Major major, double value)
            {
                Major = major.ToString();
                Value = value;
            }
        }

        public class MajorLocation
        {
            public MajorPreference MajorPreference;
            public string Location;

            public MajorLocation(MajorPreference majorPreference, string location)
            {
                MajorPreference = majorPreference;
                Location = location;
            } 
        }

        public class SurveyData
        {
            public string session_key;
            public List<InterestData> interests;
        }

        public class InterestData
        {
            public string interest;
            public int value;
        }

        [HttpGet]
        public Object GetRecommendedList([FromUri]string username)
        {
            List<MajorPreference> majorMatches = new List<MajorPreference>();
            return MapMajorsToLocations(GetTopMajorMatchesForPlayer(username, TOP_X_MAJORS));
        }

        [HttpPost]
        public Object SaveRatings([FromBody] SurveyData surveyData)
        {
            BasicResponse response = new BasicResponse();
            response.error = true;

            string username = GetUsername(surveyData.session_key);
            if (username == string.Empty)
            {
                response.message = "Invalid session key";
                return response;
            }
            List<Interest> arrayOfInterests = ((Interest[])Enum.GetValues(typeof(Interest))).Cast<Interest>().ToList();
            StringBuilder queries = new StringBuilder();
            string query = "INSERT INTO Player_Interests VALUES('{0}', '{1}', {2});";
            for (int i = 0; i < surveyData.interests.Count; i++)
            {
                if (arrayOfInterests.Contains(surveyData.interests[i].interest.ToEnum<Interest>()))
                {
                    queries.Append(string.Format(query, username, surveyData.interests[i].interest, surveyData.interests[i].value));
                }
                else
                {
                    response.message = surveyData.interests[i].interest.ToString() + " is an invalid interest";
                }
            }

            SqlCommand command = new SqlCommand(queries.ToString());
            Database.Connect();
            Database.Query(command);
            Database.Disconnect();

            response.error = false;
            return response;
        }

        //////////////////////////////////////////////////////////////////////////////
        // Get username from a session key
        //////////////////////////////////////////////////////////////////////////////
        private string GetUsername(string sessionKey)
        {
            string username = string.Empty;
            SqlCommand query = new SqlCommand(
                "SELECT username FROM Sessions " +
                "WHERE session_key = @sessionKey");
            query.Parameters.AddWithValue("@sessionKey", sessionKey);
            Database.Connect();
            SqlDataReader reader = Database.Query(query);
            if (reader.HasRows)
            {
                reader.Read();
                username = reader["username"].ToString();
            }
            Database.Disconnect();
            return username;
        }

        //////////////////////////////////////////////////////////////////////////////
        // Functions for getting the majors that match the player's preferences best
        //////////////////////////////////////////////////////////////////////////////
        private List<MajorPreference> GetTopMajorMatchesForPlayer(string username, int topXscores)  
        {
            Interests playerInterestsFromDB = GetPlayerInterests(username);
            List<MajorPreference> playerPreferenceValues = new List<MajorPreference>();
            Array arrayOfMajors = (Major[])Enum.GetValues(typeof(Major));
            Array arrayOfInterests = (Interest[])Enum.GetValues(typeof(Interest));

            foreach (Major major in arrayOfMajors)
            {
                double majorValue = 0.0;
                double highestValue = 0.0;
                double lowestValue = 0.0;
                foreach (Interest interest in arrayOfInterests)
                {
                    double weight = Weights.Matrix[Convert.ToInt32(interest)][Convert.ToInt32(major)];

                    double lowValue = weight < 0 ? weight * 5.0 : weight * 1.0;
                    double highValue = weight > 0 ? weight * 5.0 : weight * 1.0;
                    highestValue += highValue;
                    lowestValue += lowValue;

                    majorValue += Convert.ToDouble(playerInterestsFromDB.Preference[interest]) *
                        Weights.Matrix[Convert.ToInt32(interest)][Convert.ToInt32(major)];
                }
                    
                majorValue = ((majorValue - lowestValue) / (highestValue - lowestValue));
                playerPreferenceValues.Add(new MajorPreference(major, majorValue));
            }
            playerPreferenceValues.Sort((y, x) => (x.Value).CompareTo(y.Value));
            int topXscoreIndex = GetTopXIndex(playerPreferenceValues, topXscores);
            return playerPreferenceValues.GetRange(0, topXscoreIndex);
        }

        private Interests GetPlayerInterests(string username)
        {
            Interests playerInterests = new Interests();
            SqlCommand query = new SqlCommand(
               "SELECT * FROM Player_Interests " +
               //"INNER JOIN Sessions ON Sessions.username = Player_Interests.username " +
               "WHERE username = @username"
            //"Sessions.session_key = @sessionKey;"
            );
            query.Parameters.AddWithValue("@username", username);
            Database.Connect();
            SqlDataReader reader = Database.Query(query);
            while (reader.Read())
            {
                Interest interest = reader["Interest"].ToString().ToEnum<Interest>();
                playerInterests.Preference[interest] = reader["Preference_Value"].ToFloat();
            }

            Database.Disconnect();
            return playerInterests;
        }

        private int GetTopXIndex(List<MajorPreference> majorPreferences, int topXscores)
        {
            int index = 0;
            double oldValue = majorPreferences[0].Value;
            for (int i = 1, numberOfDifferentValues = 0; i < majorPreferences.Count && numberOfDifferentValues < topXscores; i++)
            {
                double currentValue = majorPreferences[i].Value;
                if (currentValue != oldValue)
                {
                    numberOfDifferentValues++;
                    oldValue = currentValue;
                }
                index++;
            }

            return index;
        }

        //////////////////////////////////////////////////////////////////////////////
        // Functions for mapping majors to locations
        //////////////////////////////////////////////////////////////////////////////
        private Dictionary<Major, List<string>> GetMajorLocations()
        {
            Dictionary<Major, List<string>> majorLocations = new Dictionary<Major, List<string>>();
            SqlCommand query = new SqlCommand(
                "SELECT majors.Major, GPS_Locations.Location_Name " +
                "FROM Major_Locations AS majors " +
                "INNER JOIN GPS_Locations ON GPS_Locations.location_id = majors.location_id"
            );
            Database.Connect();
            SqlDataReader reader = Database.Query(query);
            while (reader.Read())
            {
                Major majorKey = reader["Major"].ToString().ToEnum<Major>();
                if (!majorLocations.ContainsKey(majorKey))
                {
                    majorLocations[majorKey] = new List<string>();
                }

                majorLocations[majorKey].Add(reader["Location_Name"].ToString());
            }
            Database.Disconnect();
            return majorLocations;
        }

        private List<MajorLocation> MapMajorsToLocations(List<MajorPreference> majorPreferences)
        {
            Dictionary<Major, List<string>> allMajorLocations = GetMajorLocations();
            List<MajorLocation> majorLocations = new List<MajorLocation>();
            foreach (MajorPreference preference in majorPreferences)
            {
                List<string> locations = allMajorLocations[preference.Major.ToEnum<Major>()];
                foreach (string location in locations)
                {
                    majorLocations.Add(new MajorLocation(preference, location));
                }
            }
            return majorLocations;
        }
    }
}
