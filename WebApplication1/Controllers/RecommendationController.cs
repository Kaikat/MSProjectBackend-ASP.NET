using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data.SqlClient;
using NReco.CF.Taste.Model;
using NReco.CF.Taste.Common;
using NReco.CF.Taste.Impl.Common;
using NReco.CF.Taste.Impl.Model;

namespace WebApplication1.Controllers
{
    public class RecommendationController : ApiController
    {
        Database Database = new Database();
        const int TOP_X_MAJORS = 5;

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
                    Major major = reader["Major"].ToString().ToEnum<Major>();
                    if (!InterestsFor.ContainsKey(major))
                    {
                        InterestsFor.Add(major, new Interests());
                    }

                    InterestsFor[major].Preference.Add(reader["Interest"].ToString().ToEnum<Interest>(), reader["Interest_Value"].ToFloat()); 
                }

                Database.Disconnect();
            }
        }

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
            try
            {
                majorMatches = GetTopMajorMatchesForPlayer(username, TOP_X_MAJORS);
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

        private List<MajorPreference> GetTopMajorMatchesForPlayer(string username, int topXscores)
        {
            MajorInterestsTable majorsTable = new MajorInterestsTable();
            Interests playerInterestsFromDB = GetPlayerInterests(username);

            List<MajorPreference> playerPreferenceValues = new List<MajorPreference>();
            Array arrayOfMajors = (Major[])Enum.GetValues(typeof(Major));
            Array arrayOfInterests = (Interest[])Enum.GetValues(typeof(Interest));
            foreach (Major major in arrayOfMajors)
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
            float oldValue = majorPreferences[0].Value;
            for (int i = 1, numberOfDifferentValues = 0; i < majorPreferences.Count && numberOfDifferentValues < topXscores; i++)
            {
                float currentValue = majorPreferences[i].Value;
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
        // Functions for recommending majors to a player based on other player's
        // preferences who are similar to them
        //////////////////////////////////////////////////////////////////////////////
        private List<MajorPreference> GetRecommendedMajors(string username)
        {
            List<MajorPreference> recommendedMajors = new List<MajorPreference>();

            return recommendedMajors;
        }


        /*private IDataModel Load(string username)
        {
            int currentPlayerID = -1;

            FastByIDMap<IList<IPreference>> data = new FastByIDMap<IList<IPreference>>();
            SqlCommand query = new SqlCommand("SELECT * FROM Player_Interests");
            Database.Connect();
            SqlDataReader reader = Database.Query(query);

            int id = 0;
            while (reader.Read())
            {

                id++;
            }

            Database.Disconnect();


            var hasPrefVal = !String.IsNullOrEmpty(PrefValFld);

            using (var dbRdr = SelectCmd.ExecuteReader())
            {
                while (dbRdr.Read())
                {
                    long userID = Convert.ToInt64(dbRdr[UserIdFld]);
                    long itemID = Convert.ToInt64(dbRdr[ItemIdFld]);

                    var userPrefs = data.Get(userID);
                    if (userPrefs == null)
                    {
                        userPrefs = new List<IPreference>(3);
                        data.Put(userID, userPrefs);
                    }

                    if (hasPrefVal)
                    {
                        var prefVal = Convert.ToSingle(dbRdr[PrefValFld]);
                        userPrefs.Add(new GenericPreference(userID, itemID, prefVal));
                    }
                    else
                    {
                        userPrefs.Add(new BooleanPreference(userID, itemID));
                    }
                }

            }

            var newData = new FastByIDMap<IPreferenceArray>(data.Count());
            foreach (var entry in data.EntrySet())
            {
                var prefList = (List<IPreference>)entry.Value;
                newData.Put(entry.Key, hasPrefVal ?
                    (IPreferenceArray)new GenericUserPreferenceArray(prefList) :
                    (IPreferenceArray)new BooleanUserPreferenceArray(prefList));
            }
            return new GenericDataModel(newData);
        }*/








        private List<Location> GetMajorLocations(List<MajorPreference> majors)
        {
            List<Location> majorLocations = new List<Location>();

            return majorLocations;
        }
    }
}
