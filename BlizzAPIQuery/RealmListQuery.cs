using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace BlizzAPIQuery
{

	// Data Classes for JSON conversion
	public class Realm
	{
		public string type { get; set; }
		public string population { get; set; }
		public bool queue { get; set; }
		public bool status { get; set; }
		[Key]
		public string name { get; set; }
		public string slug { get; set; }
		public string battlegroup { get; set; }
		public string locale { get; set; }
		public string timezone { get; set; }
		public virtual List<string> connected_realms { get; set; }

		/*public string connected_realmsString
		{
			get { return string.Join(",", connected_realms); }
			set { connected_realms = new List<string>(value.Split(',')); }
		}*/
	}
	public class RealmList
	{
		public List<Realm> realms { get; set; }
	}

	public class RealmContext : DbContext
	{
		[Key]
		public DbSet<Realm> realms { get; set; }
		//public DbSet<List<string>> connectedRealms { get; set; }
	}

	class RealmListQuery
	{
		static HttpClient client = new HttpClient();

		public void updateRealmList()
		{
			Console.WriteLine(DateTime.Now + " Updating realm lists...");
			getRealmsAndInsert().Wait();
		}

		// Gets data from API and pushes into database
		static async Task getRealmsAndInsert()
		{
			client.BaseAddress = new Uri("https://us.api.battle.net/wow/");
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			List<Realm> realms = null;

			// API Call
			try
			{
				RealmList rlObject = await getAPIRealmsAsync("realm/status?locale=en_US&apikey=k7rsncmwup6nttk6vzeg6knyw4jrjjzj");
				realms = rlObject.realms;
				Console.WriteLine(DateTime.Now + " I've downloaded the realm statuses!");
			}
			catch (Exception e)
			{
				Console.WriteLine(DateTime.Now + " Could not connect to the Blizz API to update realm status!\n" + e.Message + "\n" + e.StackTrace);
			}

			// Database Insert
			try
			{
				using (var db = new RealmContext())
				{
					foreach(Realm r in realms){
						db.realms.Add(r);
						//db.connectedRealms.Add(r.connected_realms);
					}
                    db.SaveChanges();
                   
				}
				Console.WriteLine(DateTime.Now + " Realms have been successfully updated!\n");
			}
			catch (Exception e)
			{
				Console.WriteLine(DateTime.Now + " Could not insert realms into DB!\n" + e.Message + "\n" + e.StackTrace);
			}
		}

		// Pulls the data from the API
		static async Task<RealmList> getAPIRealmsAsync(String path)
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
