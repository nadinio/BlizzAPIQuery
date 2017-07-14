using System;

namespace BlizzAPIQuery
{
	class Program
	{
		static void Main(string[] args)
		{
			while (true)
			{
				Console.WriteLine("Commmands: \nrl \t\t Update the realmlist.\nahupdate \t Downloads and updates all AH data.");
				String userInput = Console.ReadLine();
				switch(userInput)
				{
					case "rl":
						RealmListQuery realmList = new RealmListQuery();
						realmList.updateRealmList();
						break;
					case "ahupdate":
						AuctionHouseQuery ahData = new AuctionHouseQuery();
						ahData.updateAHData();
						break;
					default:
						Console.WriteLine("Invalid command.");
						break;

				}
			}

			/*
			RealmListQuery realmList = new RealmListQuery();
			realmList.updateRealmList();
			

			AuctionHouseQuery ahData = new AuctionHouseQuery();
			ahData.updateAHData();
			*/



		}	
	}
}
