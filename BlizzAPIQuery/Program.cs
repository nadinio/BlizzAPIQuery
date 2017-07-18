using System;

namespace BlizzAPIQuery
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0)
				action(null);
			else
				action(args[0]);
		}
		
		static void action(String action)
		{
			Boolean exit = false;
			while (!exit)
			{
				Console.WriteLine("Commmands: \nrl \t\t Update the realmlist.\nahupdate \t Downloads and updates all AH data." +
					"\nexit \t\tExits the application");

				String userInput;

				if (action == null)
					userInput = Console.ReadLine();
				else
					userInput = action;	

				
				switch (userInput)
				{
					case "rl":
						RealmListQuery realmList = new RealmListQuery();
						realmList.updateRealmList();
						break;
					case "ahupdate":
						AuctionHouseQuery ahData = new AuctionHouseQuery();
						ahData.updateAHData();
						break;
					case "exit":
						exit = true;
						break;
					default:
						Console.WriteLine("Invalid command.");
						break;
				}

				if (action != null)
					break;
			}
		}
	}
}
