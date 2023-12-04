namespace DesignTech_PLM_Entegrasyon_App.MVC.Models.Login
{
	public class User
	{
		public string Username { get; set; }
		public string Password { get; set; }

		public bool IsLoggedIn { get; set; }
		public DateTime LastLoginDate { get; set; }
		public TimeSpan LastLoginTime { get; set; }
	}
}
