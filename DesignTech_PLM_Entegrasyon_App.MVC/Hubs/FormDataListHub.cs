using Dapper;
using DesignTech_PLM_Entegrasyon_App.MVC.Models.LogTable;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Hubs
{
    public class FormDataListHub : Hub
    {
		private readonly IConfiguration _configuration;
		private readonly IHttpClientFactory _httpClientFactory;

		public FormDataListHub(IHttpClientFactory httpClientFactory, IConfiguration configuration)
		{
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
		}

		public async Task SendFormDataCount()
		{
			try
			{

		
			string connectionString = _configuration.GetConnectionString("Plm");
			string schema = _configuration["Catalog"];

			using IDbConnection connection = new SqlConnection(connectionString);

			// Sorguları optimize edin ve sadece gerekli sütunları çekin
			var Change_Notice_LogTableList = connection.Query<Change_Notice_LogTable>($"SELECT * FROM {schema}.Change_Notice_LogTable").OrderByDescending(x => x.ProcessTimestamp).ToList();

			if (Change_Notice_LogTableList is not null)
			{
				var value = Change_Notice_LogTableList.Count();
				await Clients.All.SendAsync("ReceiveFormDataCount",value);
			}
			}
			catch (Exception)
			{

			}
		}

		public async Task SendFormData()
		{
			try
			{

			
			string connectionString = _configuration.GetConnectionString("Plm");
			string schema = _configuration["Catalog"];

			using IDbConnection connection = new SqlConnection(connectionString);

			// Sorguları optimize edin ve sadece gerekli sütunları çekin
			var Change_Notice_LogTableList = connection.Query<Change_Notice_LogTable>($"SELECT * FROM {schema}.Change_Notice_LogTable").OrderByDescending(x => x.ProcessTimestamp).ToList();

			if (Change_Notice_LogTableList is not null)
			{
				var value = Change_Notice_LogTableList;
				await Clients.All.SendAsync("ReceiveFormData", value);
			}
			}
			catch (Exception)
			{

				throw;
			}

		}



	}
}
