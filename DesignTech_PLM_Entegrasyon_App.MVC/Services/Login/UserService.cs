using DesignTech_PLM_Entegrasyon_App.MVC.Models.Login;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Services.Login
{
    //public interface IUserService
    //{
    //    List<User> GetUsers();
    //    void AddUser(User user);
    //}

    //public class UserService : IUserService
    //{
    //    private const string JsonFilePath = "wwwroot\\LoginJson\\users.json";

    //    //public List<User> GetUsers()
    //    //{
    //    //    if (File.Exists(JsonFilePath))
    //    //    {
    //    //        string json = File.ReadAllText(JsonFilePath);
    //    //        return JsonConvert.DeserializeObject<List<User>>(json);
    //    //    }

    //    //    return new List<User>();
    //    //}

    //    //public void AddUser(User user)
    //    //{
    //    //    List<User> users = GetUsers();
    //    //    user.Id = Guid.NewGuid();
    //    //    users.Add(user);

    //    //    string json = JsonConvert.SerializeObject(users, Formatting.Indented);
    //    //    File.WriteAllText(JsonFilePath, json);
    //    //}
    //}
}
