using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data.SqlClient;

namespace WebApplication1.Controllers
{
    public class RecommendationController : ApiController
    {
        Database Database = new Database();

        public class MajorInterestsTable
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
                    InterestsFor.Add(reader["Major"].ToString().ToEnum<Major>(), GetInterestDataFromReader(reader)); 
                }

                Database.Disconnect();
            }

            private Interests GetInterestDataFromReader(SqlDataReader reader)
            {
                Interests interests = new Interests();
                Array arrayOfInterests = (Interest[])Enum.GetValues(typeof(Interest));
                foreach(Interest interest in arrayOfInterests)
                {
                    interests.Preference[interest] = reader[interest.ToString()].ToInt();
                }

                return interests;
            }
        }

        public class Interests
        {
            public Dictionary<Interest, int> Preference;
            public Interests()
            {
                Preference = new Dictionary<Interest, int>();
                Array arrayOfInterests = (Interest[])Enum.GetValues(typeof(Interest));
                foreach(Interest interest in arrayOfInterests)
                {
                    Preference.Add(interest, -1);
                }
            }
        }

        public class MajorPreference
        {
            public string Major;
            public float Value;

            public MajorPreference(Major major, float value)
            {
                Major = major.ToString();
                Value = value;
            }
        }

        [HttpGet]
        public Object GetRecommendedList([FromUri]string username)
        {
            List<MajorPreference> majorMatches = new List<MajorPreference>();
            majorMatches = GetTopMajorMatchesForPlayer(username, 25);
            try
            {
                //majorMatches.AddRange(GetRecommendedMajors(username));
            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message;
            }
            //List<Location> locationsToVisit = GetMajorLocations(majorMatches);
            return majorMatches;
        }

        //////////////////////////////////////////////////////////////////////////////
        // Functions for getting the majors that match the player's preferences best
        //////////////////////////////////////////////////////////////////////////////

        private List<MajorPreference> GetTopMajorMatchesForPlayer(string username, int topXmatches)
        {
            MajorInterestsTable majorsTable = new MajorInterestsTable();
            Interests playerInterestsFromDB = GetPlayerInterests(username);

            List<MajorPreference> playerPreferenceValues = new List<MajorPreference>();
            Array arrayOfMajors = (Major[])Enum.GetValues(typeof(Major));
            Array arrayOfInterests = (Interest[])Enum.GetValues(typeof(Interest));
            foreach (Major major in arrayOfMajors)
            {
                int majorInterestValue = 0;
                int totalInterestsForMajor = 0;
                foreach (Interest interest in arrayOfInterests)
                {
                    totalInterestsForMajor += majorsTable.InterestsFor[major].Preference[interest];
                    majorInterestValue += playerInterestsFromDB.Preference[interest] * majorsTable.InterestsFor[major].Preference[interest];
                }

                playerPreferenceValues.Add(new MajorPreference(major, (float)majorInterestValue / (float)totalInterestsForMajor));
            }

            playerPreferenceValues.Sort((y, x) => (x.Value).CompareTo(y.Value));
            return playerPreferenceValues.GetRange(0, topXmatches);
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

            Array arrayOfInterests = (Interest[])Enum.GetValues(typeof(Interest));
            SqlDataReader reader = Database.Query(query);
            while (reader.Read())
            {
                foreach (Interest interest in arrayOfInterests)
                {
                    playerInterests.Preference[interest] = reader[interest.ToString()].ToInt();
                }
            }

            Database.Disconnect();
            return playerInterests;
        }

        private List<MajorPreference> GetRecommendedMajors(string username)
        {
            List<MajorPreference> recommendedMajors = new List<MajorPreference>();

            return recommendedMajors;
        }

        private List<Location> GetMajorLocations(List<MajorPreference> majors)
        {
            List<Location> majorLocations = new List<Location>();

            return majorLocations;
        }

        //////////////////////////////////////////////////////////////////////////////
        // Functions for recommending majors to a player based on other player's
        // preferences who are similar to them
        //////////////////////////////////////////////////////////////////////////////


    }
}
