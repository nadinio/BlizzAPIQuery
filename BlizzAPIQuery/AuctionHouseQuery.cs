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
		public int auc { get; set; }
		public int item { get; set; }
		public string owner { get; set; }
		public string ownerRealm { get; set; }
		public int bid { get; set; }
		public int buyout { get; set; }
		public int quantity { get; set; }
		public string timeLeft { get; set; }
		public int rand { get; set; }
		public int seed { get; set; }
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
		static HttpClient client = new HttpClient();

		public void updateAHData()
		{
			getAHRealmData().Wait();
		}

		static async Task getAHRealmData()
		{
			// For the time being this will just pull one realm's AH data
			// I want to be able to handle one realm properly before
			// sifting through hundreds and figuring out what went wrong.

			client.BaseAddress = new Uri("https://us.api.battle.net/wow/");
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			AHFileRoot ahRoot = null;
			AHRootObject
			

			try
			{
				Console.WriteLine("I'm attempting to get all the AH data. This could take a while...");
				ahRoot = await getAPIAHRootData("auction/data/executus?locale=en_US&apikey=k7rsncmwup6nttk6vzeg6knyw4jrjjzj");
				Console.WriteLine("I've got the AH data!");
			}
			catch(Exception e)
			{
				Console.WriteLine("Could not connect to the Blizz API to update AH data!\n" + e.Message + "\n" + e.StackTrace);
			}

		}

		static async Task<AHFileRoot> getAPIAHRootData(String path)
		{
			AHFileRoot ahRoot = null;
			HttpResponseMessage response = await client.GetAsync(path);

			String responseData = "";

			if (response.IsSuccessStatusCode)
				responseData = await response.Content.ReadAsStringAsync();

			JObject ahRootData = JObject.Parse(responseData);
			ahRoot = JsonConvert.DeserializeObject<AHFileRoot>(responseData);

			return ahRoot;

		}

	}
}
