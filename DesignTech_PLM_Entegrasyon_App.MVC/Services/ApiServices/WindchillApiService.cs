using System.Text;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Services.ApiServices
{
	public class WindchillApiService
	{
		private readonly IWebHostEnvironment _env;

		public WindchillApiService(IWebHostEnvironment env)
		{
			_env = env;
		}


		public async Task<string> GetApiData(string baseUrl, string endpoint)
		{
			// Windows servisinde ortam adını belirleme

			try
			{

			using (var client = new HttpClient())
			{
					var request = new HttpRequestMessage(HttpMethod.Get, $"http://{baseUrl}/Windchill/servlet/odata/{endpoint}?$skiptoken=-25");

					request.Headers.Add("CSRF_NONCE", "qWhBSqh2RWM43KBJy1ktOPFAcgcI78YF+hxwAvxPEldU79p6kTggHcA7CjsBktgCmA91e8ZdMl4C7Zd5m1t0c5hHcFEM6Zp8+DgxefgnFQlV7c0h4ik2EN8sN0h6meYChj0jc+oOHQgXsuUCyB12EMkjeA==");
				request.Headers.Add("Authorization", "Basic UExNLTE6RGVzLjIzIVRlY2g=");
				request.Headers.Add("Prefer", "odata.maxpagesize=2500");
					var content = new StringContent("{\r\n\"NoOfFiles\":1\r\n}", null, "application/json");
					request.Content = content;


					var response = await client.SendAsync(request);
				response.EnsureSuccessStatusCode();

				return await response.Content.ReadAsStringAsync();
			}


			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}

		//public async Task<string> GetApiData(string baseUrl, string endpoint)
		//{
		//	try
		//	{

		
		//	var request = new HttpRequestMessage(HttpMethod.Get, $"http://{baseUrl}/Windchill/servlet/odata/{endpoint}");
		//	request.Headers.Add("CSRF_NONCE", "qWhBSqh2RWM43KBJy1ktOPFAcgcI78YF+hxwAvxPEldU79p6kTggHcA7CjsBktgCmA91e8ZdMl4C7Zd5m1t0c5hHcFEM6Zp8+DgxefgnFQlV7c0h4ik2EN8sN0h6meYChj0jc+oOHQgXsuUCyB12EMkjeA==");
		//	request.Headers.Add("Authorization", "Basic UExNLTE6RGVzLjIzIVRlY2g=");

		//	string jsonData = "{\r\n\"NoOfFiles\":1\r\n}";

		//	if (!string.IsNullOrEmpty(jsonData))
		//	{
		//		var content = new StringContent(jsonData, null, "application/json");
		//		request.Content = content;
		//	}

		//	var response = await _httpClient.SendAsync(request);

		//	if (response.IsSuccessStatusCode)
		//	{
		//		return await response.Content.ReadAsStringAsync();
		//	}
		//	else
		//	{
		//		// Handle error cases
		//		return null;
		//	}
		//	}
		//	catch (Exception ex)
		//	{
		//		return ex.Message;
		//	}
		//}


	}
}
