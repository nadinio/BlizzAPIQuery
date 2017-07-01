using System.Collections.Generic;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace BlizzAPIQuery
{
	// Class definition for the root AH query
	public class AHFile
	{
		public string url { get; set; }
		public long lastModified { get; set; }
	}

	public class AHFileRoot
	{
		public List<AHFile> files { get; set; }
	}

	// Class definition for the AH data

	public class AHRealm
	{
		public string name { get; set; }
		public string slug { get; set; }
	}

	public class Modifier
	{
		public int type { get; set; }
		public int value { get; set; }
	}

	public class Auction
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
	}

	public class AHRootObject
	{
		public List<AHRealm> realms { get; set; }
		public List<Auction> auctions { get; set; }
	}

	class AuctionHouseQuery
	{
		static HttpClient ahFileClient = new HttpClient();
		static HttpClient ahDataClient = new HttpClient();

		public void updateAHData()
		{
			getAHRealmData().Wait();
		}

		static async Task getAHRealmData()
		{
			// For the time being this will just pull one realm's AH data
			// I want to be able to handle one realm properly before
			// sifting through hundreds and figuring out what went wrong.

			ahFileClient.BaseAddress = new Uri("https://us.api.battle.net/wow/");
			ahFileClient.DefaultRequestHeaders.Accept.Clear();
			ahFileClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			ahDataClient.BaseAddress = new Uri("http://auction-api-us.worldofwarcraft.com/auction-data/");
			ahDataClient.DefaultRequestHeaders.Accept.Clear();
			ahDataClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			AHFileRoot ahFileRoot = null;
			AHRootObject ahRootData = null;
			

			try
			{
				Console.WriteLine("I'm attempting to get all the AH data. This could take a while...");
				ahFileRoot = await getAPIAHRootFileData("auction/data/stormrage?locale=en_US&apikey=k7rsncmwup6nttk6vzeg6knyw4jrjjzj");

				String ahRootDataPath = (ahFileRoot.files[0].url).Substring(55);
				ahRootData = await getAPIAHRootData(ahRootDataPath);
				Console.WriteLine("I've got the AH data!");
			}
			catch(Exception e)
			{
				Console.WriteLine("Could not connect to the Blizz API to update AH data!\n" + e.Message + "\n" + e.StackTrace);
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
