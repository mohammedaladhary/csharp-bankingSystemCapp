using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
namespace BankingSystemCApp
{
	public class UserDataManager
	{
        private const string UserDataFilePath = "userdata.json";

        public List<User> LoadUserData()
        {
            if (File.Exists(UserDataFilePath))
            {
                string json = File.ReadAllText(UserDataFilePath);
                return JsonConvert.DeserializeObject<List<User>>(json);
            }
            else
            {
                return new List<User>();
            }
        }

        public void SaveUserData(List<User> users)
        {
            string json = JsonConvert.SerializeObject(users);
            File.WriteAllText(UserDataFilePath, json);
        }
    }
}