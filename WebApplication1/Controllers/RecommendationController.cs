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

using NReco.CF.Taste.Impl.Recommender;
using NReco.CF.Taste.Neighborhood;
using NReco.CF.Taste.Impl.Neighborhood;
using NReco.CF.Taste.Impl.Similarity;

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
            majorMatches = GetTopMajorMatchesForPlayer(username, TOP_X_MAJORS);
            /*Object obj = "hull";
            try
            {
                //majorMatches.AddRange(GetRecommendedMajors(username));
                obj = GetRecommendedMajors(username);
            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message + " " + (string) obj;
            }
            //List<Location> locationsToVisit = GetMajorLocations(majorMatches);
            return (string)obj;//
            */
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
        public class UserDataModel
        {
            public IDataModel DataModel;
            public long UserID;

            public UserDataModel()
            {
                UserID = 0;
            }
        }

        private Object //List<MajorPreference> 
            GetRecommendedMajors(string username)
        {
            const int NEAREST_NEIGHBORS = 5;
            const int NUMBER_OF_RECOMMENDATIONS = 2;
            List<MajorPreference> recommendedMajors = new List<MajorPreference>();
            Object dataModel = Load(username);
            /*
            PearsonCorrelationSimilarity pearsonSimilarity = new PearsonCorrelationSimilarity(dataModel.DataModel);
            GenericUserSimilarity userSimilarity = new GenericUserSimilarity(pearsonSimilarity, dataModel.DataModel);
            NearestNUserNeighborhood userNeighborhood = new NearestNUserNeighborhood(NEAREST_NEIGHBORS, userSimilarity, dataModel.DataModel);
            userNeighborhood.GetUserNeighborhood(dataModel.UserID);
            GenericUserBasedRecommender recommender = new GenericUserBasedRecommender(dataModel.DataModel, userNeighborhood, userSimilarity);
            var recommendedUsers = recommender.Recommend(dataModel.UserID, NUMBER_OF_RECOMMENDATIONS);
            */
            // return recommendedMajors;
            return dataModel;// recommendedUsers;
        }


        private Object Load(string username)
        {
            UserDataModel userDataModel = new UserDataModel();

            long userID = 0;
            FastByIDMap<IList<IPreference>> data = new FastByIDMap<IList<IPreference>>();
            SqlCommand query = new SqlCommand("SELECT * FROM Player_Interests");
            Database.Connect();
            SqlDataReader reader = Database.Query(query);

            //string debugger = "";
            while (reader.Read())
            {
                long interestID = Convert.ToInt64(reader["interest"].ToString().ToEnum<Interest>());
                //debugger += interestID.ToString() + ", ";
                //debugger += reader["interest"].ToString().ToEnum<Interest>().ToString();
                if (reader["username"].ToString() == username)
                {
                    userDataModel.UserID = userID;
                }

                IList<IPreference> userPreferences = data.Get(userID);
                if (userPreferences == null)
                {
                    userPreferences = new List<IPreference>(Enum.GetNames(typeof(Interest)).Length);
                    data.Put(userID, userPreferences);
                }

                float interestValue = reader["preference_value"].ToFloat();
                userPreferences.Add(new GenericPreference(userID, interestID, interestValue));
                userID++;
            }

             Database.Disconnect();
            
             var newData = new FastByIDMap<IPreferenceArray>(data.Count());
             foreach (var entry in data.EntrySet())
             {
                 var prefList = (List<IPreference>)entry.Value;
                 newData.Put(entry.Key, (IPreferenceArray)new GenericUserPreferenceArray(prefList));
             }

             userDataModel.DataModel = new GenericDataModel(newData);
             return userDataModel;
             //return "grr";
        }

        private List<Location> GetMajorLocations(List<MajorPreference> majors)
        {
            List<Location> majorLocations = new List<Location>();

            return majorLocations;
        }
    }
}
