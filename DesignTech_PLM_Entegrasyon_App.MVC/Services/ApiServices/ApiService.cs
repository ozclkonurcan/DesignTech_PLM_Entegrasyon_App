using System.Text;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Services.ApiServices
{
    public class ApiService
    {
        private readonly IWebHostEnvironment _env;

        public ApiService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> PostDataAsync(string endpoint, string jsonContent)
        {
            // Windows servisinde ortam adını belirleme
            var environmentName = _env.EnvironmentName;
            var apiUrl = environmentName == "Development" ? "https://localhost:7277" : "/api/abouts";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl}/api/{endpoint}");
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                request.Content = content;

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }
    }

}
