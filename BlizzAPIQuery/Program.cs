using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace BlizzAPIQuery
{

	// Data Classes for JSON conversion
	public class Realm
	{
		public string type { get; set; }
		public string population { get; set; }
		public bool queue { get; set; }
		public bool status { get; set; }
		public string name { get; set; }
		public string slug { get; set; }
		public string battlegroup { get; set; }
		public string locale { get; set; }
		public string timezone { get; set; }
		public List<string> connected_realms { get; set; }
	}
	public class RealmList
	{
		public List<Realm> realms { get; set; }
	}

	class Program
	{
		static HttpClient client = new HttpClient();

		static void Main(string[] args)
		{
			/*while (true)
			{ 
				String userInput = Console.ReadLine();
				Console.Write(userInput);
			}*/


			RunAsync().Wait();
		}


		static async Task RunAsync()
		{
			client.BaseAddress = new Uri("https://us.api.battle.net/wow/");
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			try
			{
				RealmList realms = await GetRealmsAsync("realm/status?locale=en_US&apikey=k7rsncmwup6nttk6vzeg6knyw4jrjjzj");
			}
			catch(Exception e)
			{
				Console.WriteLine("Could not connect to the Blizz API to update realm status!");
			}

			Console.ReadLine();
		}

		static async Task<RealmList> GetRealmsAsync(String path)
		{
			RealmList realms = null;
			HttpResponseMessage response = await client.GetAsync(path);

			String responseData = "";

			if (response.IsSuccessStatusCode)
				responseData = await response.Content.ReadAsStringAsync();


			JObject realmData = JObject.Parse(responseData);
			realms = JsonConvert.DeserializeObject<RealmList>(responseData);

			return realms;
		}
	}


}
