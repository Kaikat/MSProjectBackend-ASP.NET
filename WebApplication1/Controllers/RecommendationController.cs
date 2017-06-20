using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace WebApplication1.Controllers
{
    public class RecommendationController : ApiController
    {
        Database Database = new Database();
        const int TOP_X_MAJORS = 5;

        /*public class MajorInterestsTable
        {
            Database Database = new Database();
            public Dictionary<Major, Interests> InterestsFor;

            public MajorInterestsTable()
            {
                InterestsFor = new Dictionary<Major, Interests>();

                SqlCommand query = new SqlCommand("SELECT * FROM Major_Interests");
                Database.Connect();
                SqlDataReader reader = Database.Query(query);
                while (reader.Read())
                {
                    Major major = reader["Major"].ToString().ToEnum<Major>();
                    if (!InterestsFor.ContainsKey(major))
                    {
                        InterestsFor.Add(major, new Interests());
                    }

                    InterestsFor[major].Preference.Add(reader["Interest"].ToString().ToEnum<Interest>(), reader["Interest_Value"].ToFloat()); 
                }

                Database.Disconnect();
            }
        }*/

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

        [HttpGet]
        public Object GetRecommendedList([FromUri]string username)
        {
            List<MajorPreference> majorMatches = new List<MajorPreference>();
            //majorMatches = GetTopMajorMatchesForPlayer(username, TOP_X_MAJORS);
            return GetTopMajorMatchesForPlayer(username, TOP_X_MAJORS); //majorMatches;
        }

        //////////////////////////////////////////////////////////////////////////////
        // Functions for getting the majors that match the player's preferences best
        //////////////////////////////////////////////////////////////////////////////
        private //List<MajorPreference>
            Object GetTopMajorMatchesForPlayer(string username, int topXscores)  
        {
            //MajorInterestsTable majorsTable = new MajorInterestsTable();
            Interests playerInterestsFromDB = GetPlayerInterests(username);

            List<MajorPreference> playerPreferenceValues = new List<MajorPreference>();
            Array arrayOfMajors = (Major[])Enum.GetValues(typeof(Major));
            Array arrayOfInterests = (Interest[])Enum.GetValues(typeof(Interest));

            try
            {
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
                        highestValue += highValue;//highValue > highestValue ? highValue : highestValue;
                        lowestValue += lowValue;//lowValue < lowestValue ? lowValue : lowestValue;

                        majorValue += Convert.ToDouble(playerInterestsFromDB.Preference[interest]) *
                            Weights.Matrix[Convert.ToInt32(interest)][Convert.ToInt32(major)];
                        //oldMajorValue = ((majorValue - lowestValue) / (highestValue - lowestValue)) + 1.0;
                        //majorValue += oldMajorValue;
                    }
                    
                    majorValue = ((majorValue - lowestValue) / (highestValue - lowestValue));
                    playerPreferenceValues.Add(new MajorPreference(major, majorValue));
                }
                playerPreferenceValues.Sort((y, x) => (x.Value).CompareTo(y.Value));
                int topXscoreIndex = GetTopXIndex(playerPreferenceValues, topXscores);
                //return playerPreferenceValues.GetRange(0, topXscoreIndex);
                return playerPreferenceValues;
            }
            catch(Exception e)
            {
                return e.Message;
            }
           /* try
            {
                return Weights.Matrix;
            }
            catch (Exception e)
            {
                return Weights.Matrix;
                //return Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;//"Open File Error: " + e.Message;
            }*/
            /* foreach (Major major in arrayOfMajors)
             {
                 float majorInterestValue = 0.0f;
                 float totalInterestsForMajor = 0.0f;

                 foreach (Interest interest in majorsTable.InterestsFor[major].Preference.Keys)
                 {
                     totalInterestsForMajor += majorsTable.InterestsFor[major].Preference[interest];
                     majorInterestValue += playerInterestsFromDB.Preference[interest] * majorsTable.InterestsFor[major].Preference[interest];
                 }

                 playerPreferenceValues.Add(new MajorPreference(major, (float)majorInterestValue / (float)totalInterestsForMajor));
             }

             playerPreferenceValues.Sort((y, x) => (x.Value).CompareTo(y.Value));
             int topXscoreIndex = GetTopXIndex(playerPreferenceValues, topXscores);
             return playerPreferenceValues.GetRange(0, topXscoreIndex);*/
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

        private List<Location> GetMajorLocations(List<MajorPreference> majors)
        {
            List<Location> majorLocations = new List<Location>();

            return majorLocations;
        }
    }
}
