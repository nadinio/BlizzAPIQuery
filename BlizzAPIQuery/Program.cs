using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

			RealmList realms = null;

			try
			{
				realms = await GetRealmsAsync("realm/status?locale=en_US&apikey=k7rsncmwup6nttk6vzeg6knyw4jrjjzj");
				Console.WriteLine("I've got the realm statuses!");
			}
			catch (Exception e)
			{
				Console.WriteLine("Could not connect to the Blizz API to update realm status!\n" + e.StackTrace);

			}


			try
			{
				InsertRealms(realms);
				Console.WriteLine("Realms have been successfully updated!");
			}
			catch (Exception e)
			{
				Console.WriteLine("Could not insert realms into DB!\n" + e.StackTrace);
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

		static void InsertRealms(RealmList list)
		{
			String connectionString = "Data Source=(local);Initial Catalog=RealmData;"
						+ "Integrated Security=SSPI;";

			Realm[] realmArray = list.realms.ToArray();

			

			for (int i = 0; i < realmArray.Length; i++)
			{
				String[] crArray = realmArray[i].connected_realms.ToArray();

				// First, handle connected realm listings.
				// Just insert if only one in the connected realm list. 
				if (crArray.Length == 1)
					using (SqlConnection connection = new SqlConnection(connectionString))
					{
						SqlCommand command = new SqlCommand("INSERT INTO ConnectedRealms (RealmName) VALUES ('" + realmArray[i].slug +"');", connection);
						command.Connection.Open();
						command.ExecuteNonQuery();
						command.Connection.Close();
					}
				else // else build a connect ID for all connected realms	
					using (SqlConnection connection = new SqlConnection(connectionString))
					{
						SqlCommand command = new SqlCommand("SELECT ConnectID FROM ConnectedRealms WHERE RealmName = '" + realmArray[i].slug + "';", connection);
						try
						{
							command.Connection.Open();
							int result = (int)command.ExecuteScalar();
						}
						catch
						{
							command = new SqlCommand("INSERT INTO ConnectedRealms (RealmName) VALUES ('" + realmArray[i].slug + "');", connection);
							command.ExecuteNonQuery();
							command = new SqlCommand("SET IDENTITY_INSERT ConnectedRealms ON", connection);
							command.ExecuteNonQuery();

							command = new SqlCommand("SELECT ConnectID FROM ConnectedRealms WHERE RealmName = ('" + realmArray[i].slug + "');", connection);
							int connectID = (int)command.ExecuteScalar();

							for (int j = 0; j < crArray.Length; j++)
								if(!crArray[j].Equals(realmArray[i].slug))
								{
									command = new SqlCommand("INSERT INTO ConnectedRealms (ConnectID, RealmName) VALUES (" + connectID +", '" + crArray[j] + "');", connection);
									command.ExecuteNonQuery();
								}

							command = new SqlCommand("SET IDENTITY_INSERT ConnectedRealms OFF", connection);
							command.ExecuteNonQuery();

						}
					}

				// Finally, insert realm data using the connectIDs
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					SqlCommand command = new SqlCommand("SELECT ConnectID FROM ConnectedRealms WHERE RealmName = '" + realmArray[i].slug + "';", connection);
					command.Connection.Open();
					int connectID = (int)command.ExecuteScalar();
					int realmQueue = realmArray[i].queue ? 1 : 0;
					int realmStatus = realmArray[i].status ? 1 : 0;
					String realmName = realmArray[i].name;

					if (realmName.Contains("'"));
						realmName = realmName.

					command = new SqlCommand("INSERT INTO RealmList (RealmType, RealmPop, RealmQueue, RealmStatus,"+ 
						" RealmName, RealmSlug, Battlegroup, locale, timezone, ConnectID) VALUES('" + realmArray[i].type +
						"', '" + realmArray[i].population + "', " + realmQueue + ", " + realmStatus + 
						", '" + realmName + "', '" + realmArray[i].slug + "', '" + realmArray[i].battlegroup + 
						"', '" + realmArray[i].locale + "', '" + realmArray[i].timezone + "', " + connectID + ");", connection);

					command.ExecuteNonQuery();
				}

			}
		}
	}



}
