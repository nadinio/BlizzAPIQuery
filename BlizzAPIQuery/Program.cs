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



	class Program
	{
		

		static void Main(string[] args)
		{
			/*while (true)
			{ 
				String userInput = Console.ReadLine();
				Console.Write(userInput);
			}*/

			APIQuery realmList = new APIQuery();
			realmList.updateRealmList();

			
		}	
	}
}
