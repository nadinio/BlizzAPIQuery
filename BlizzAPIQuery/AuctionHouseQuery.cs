using System.Collections.Generic;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Threading;
using System.Linq;

namespace BlizzAPIQuery
{

	// Class definition for the root AH query
	class AHFile
	{
		public string url { get; set; }
		public long lastModified { get; set; }
	}
	class AHFileRoot
	{
		public List<AHFile> files { get; set; }
	}

	// Class definition for the AH data

	class AHRealm
	{
		public string name { get; set; }
		public string slug { get; set; }
	}

	class Modifier
	{
		public int type { get; set; }
		public int value { get; set; }
	}

	public class BonusList
	{
		public int bonusListId { get; set; }
	}

	class Auction
	{
		public long auc { get; set; }
		public long item { get; set; }
		public string owner { get; set; }
		public string ownerRealm { get; set; }
		public long bid { get; set; }
		public long buyout { get; set; }
		public int quantity { get; set; }
		public string timeLeft { get; set; }
		public int rand { get; set; }
		public long seed { get; set; }
		public int context { get; set; }
		public List<Modifier> modifiers { get; set; }
		public int? petSpeciesId { get; set; }
		public int? petBreedId { get; set; }
		public int? petLevel { get; set; }
		public int? petQualityId { get; set; }
		public List<BonusList> bonusLists { get; set; }
	}

	class AHRootObject
	{
		public List<AHRealm> realms { get; set; }
		public List<Auction> auctions { get; set; }

		public void Dispose()
		{
			realms = null;
			auctions = null;
		}
	}


	public class AuctionHouseQuery
	{
		static HttpClient ahFileClient = new HttpClient();
		static HttpClient ahDataClient = new HttpClient();

		public void updateAHData()
		{
			getAHRealmData();
		}

		static void getAHRealmData()
		{
			// HTTP Clients for data collection
			ahFileClient.BaseAddress = new Uri("https://us.api.battle.net/wow/");
			ahFileClient.DefaultRequestHeaders.Accept.Clear();
			ahFileClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			ahFileClient.Timeout = TimeSpan.FromMinutes(5);
		
			ahDataClient.BaseAddress = new Uri("http://auction-api-us.worldofwarcraft.com/auction-data/");
			ahDataClient.DefaultRequestHeaders.Accept.Clear();
			ahDataClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			ahDataClient.Timeout = TimeSpan.FromMinutes(5);
			
			String connectionString = "Data Source=(local);Initial Catalog=RealmData;"
						+ "Integrated Security=SSPI;Max Pool Size=200;";

			String[] realms;
			int realmCount;

			// Build the realm list needed. Only 1 realm needed for connected realms
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				SqlCommand command = new SqlCommand("SELECT COUNT(DISTINCT ConnectID) FROM ConnectedRealms", connection);
				command.Connection.Open();
				realmCount = (int)command.ExecuteScalar();

				command = new SqlCommand("SELECT RealmName FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY ConnectID ORDER BY (SELECT 1)) AS rn FROM ConnectedRealms) foo WHERE rn = 1", connection);
				SqlDataReader dr = command.ExecuteReader();

				realms = new String[realmCount];

				for (int i = 0; i < realmCount; i++)
					if (dr.Read())
						realms[i] = dr[0].ToString();

				connection.Close();
			}
			
	

			// Get all the AH data
			try
			{
				Console.WriteLine(DateTime.Now + " I'm attempting to get all the AH data. This could take a while...");

				Task[] ahTaskArray = new Task[realmCount];

				for(int i = 0; i < realmCount; i++)
				{
					int x = i;
					ahTaskArray[i] = Task.Run(async () => await getAHDataThread(x, realms[x]));
				}

				Task.WaitAll(ahTaskArray);

				Console.WriteLine(DateTime.Now + " I've got the AH data!");
			}
			catch (Exception e)
			{
				Console.WriteLine("Could not connect to the Blizz API to update AH data!\n" + e.Message + "\n" + e.StackTrace);
			}

		}


		static async Task getAHDataThread(int index, String realm)
		{
			try
			{
				AHFileRoot fileRoot = await getAPIAHRootFileData("auction/data/" + realm + "?locale=en_US&apikey=k7rsncmwup6nttk6vzeg6knyw4jrjjzj");
				String ahRootDataPath = (fileRoot.files[0].url).Substring(55);
				AHRootObject ahData = await getAPIAHRootData(ahRootDataPath);
				insertData(ahData, realm);
				Console.WriteLine(DateTime.Now + " SUCCESS!! I've succesfully updated " + realm +"'s auction data!");
				ahData.Dispose();
			}catch(Exception e)
			{
				Console.WriteLine(DateTime.Now + " It looks like there was an error updating " + realm + ". It will attempt to update on the next data collection attempt.\n" + e.Message);
			}
		}

		static void insertData(AHRootObject ahData, String realm)
		{
			String connectionString = "Data Source=(local);Initial Catalog=RealmData;"
						+ "Integrated Security=SSPI;Max Pool Size=200;";

			Auction[] auctions = ahData.auctions.ToArray();

			using (SqlConnection connect = new SqlConnection(connectionString))
			{
				SqlCommand command = new SqlCommand("SELECT ConnectID FROM RealmList WHERE RealmSlug = '" + realm + "';", connect);
				command.Connection.Open();
				int connectID = (int)command.ExecuteScalar();

				try
				{
					command = new SqlCommand("SELECT * INTO AHData_" + connectID + " FROM AHDataTemplate;", connect);
					command.ExecuteNonQuery();
				}
				catch (Exception)
				{
					command = new SqlCommand("DELETE FROM AHData_" + connectID + ";", connect);
					command.ExecuteNonQuery();
				}

				for(int i = 0; i < auctions.Length; i++)
				{
					String bonusLists = auctions[i].bonusLists == null ? null : "Placeholderbl";
					String modifiers = auctions[i].modifiers == null ? null : "Placeholdermod";
					String realmName = auctions[i].ownerRealm.Contains("'") ? auctions[i].ownerRealm.Insert(auctions[i].ownerRealm.LastIndexOf("'"), "'") : auctions[i].ownerRealm;

					command = new SqlCommand("INSERT INTO AHData_" + connectID +
						" VALUES(" + auctions[i].auc + ", " + auctions[i].item + ", '" + auctions[i].owner + "', '" + 
						realmName + "', " + auctions[i].bid + ", " + auctions[i].buyout + ", " +
						auctions[i].quantity + ", '" + auctions[i].timeLeft + "', " + auctions[i].rand + ", " + 
						auctions[i].seed + ", " + auctions[i].context + ", " +
						(bonusLists == null ? "null, " : "'" + bonusLists + "', " ) + 
						(modifiers == null ? "null, " : "'" + modifiers + "', ") + 
						(auctions[i].petSpeciesId == null ? 0 : auctions[i].petSpeciesId) + ", " +
						(auctions[i].petBreedId == null ? 0 : auctions[i].petBreedId) + ", " +
						(auctions[i].petLevel == null ? 0 : auctions[i].petLevel) + ", " +
						(auctions[i].petQualityId == null ? 0 :auctions[i].petQualityId) + ");", connect);

					command.ExecuteNonQuery();
				}
				connect.Close();
			}

		}

		static async Task<AHFileRoot> getAPIAHRootFileData(String path)
		{
			AHFileRoot ahFileRoot = null;
			HttpResponseMessage response = await ahFileClient.GetAsync(path);

			String responseData = "";

			if (response.IsSuccessStatusCode)
				responseData = await response.Content.ReadAsStringAsync();

			JObject ahRootData = JObject.Parse(responseData);
			ahFileRoot = JsonConvert.DeserializeObject<AHFileRoot>(responseData);

			return ahFileRoot;

		}
		
		static async Task<AHRootObject> getAPIAHRootData(String path)
		{
			AHRootObject ahRootObject = null;
			HttpResponseMessage response = await ahDataClient.GetAsync(path);
			String responseData = "";

			if (response.IsSuccessStatusCode)
				responseData = await response.Content.ReadAsStringAsync();

			JObject realmData = JObject.Parse(responseData);
			ahRootObject = JsonConvert.DeserializeObject<AHRootObject>(responseData);

			return ahRootObject;
		}

	}


}
