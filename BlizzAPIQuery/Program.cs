using System;

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

			RealmListQuery realmList = new RealmListQuery();
			realmList.updateRealmList();

			Console.ReadLine();
		}	
	}
}
