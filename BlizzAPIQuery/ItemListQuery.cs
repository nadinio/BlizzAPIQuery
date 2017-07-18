using System;
using System.Data.SqlClient;

namespace BlizzAPIQuery
{
	class ItemListQuery
	{
		static String connectionString = "Data Source=(local);Initial Catalog=RealmData;"
				+ "Integrated Security=SSPI;Max Pool Size=200;";

		public void getItemInfoFromRealm(String realmName)
		{
			using (SqlConnection connect = new SqlConnection(connectionString))
			{
				connect.Open();
				SqlCommand command = new SqlCommand("SELECT ConnectID FROM RealmList WHERE RealmName='" + realmName + "';", connect);
				int tableNumber = (int)command.ExecuteScalar();

				command = new SqlCommand("SELECT COUNT(DISTINCT item) FROM AHData_" + tableNumber + ";", connect);
				int itemCount = (int)command.ExecuteScalar();

				command = new SqlCommand("SELECT DISTINCT item FROM AHData_" + tableNumber + ";", connect);
				SqlDataReader dr = command.ExecuteReader();

				int[] itemList = new int[itemCount];

				for (int i = 0; i < itemCount; i++)
					if (dr.Read())
						itemList[i] = Int32.Parse(dr[0].ToString());

				Console.ReadLine();
				 
			}
		}
	}
}
