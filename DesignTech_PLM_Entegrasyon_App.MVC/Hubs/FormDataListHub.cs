using Dapper;
using DesignTech_PLM_Entegrasyon_App.MVC.Models.LogTable;
using DesignTech_PLM_Entegrasyon_App.MVC.Repository;
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
		private readonly IGenericRepository<Change_Notice_LogTable> _change_Notice_LogTable;
		public FormDataListHub(IHttpClientFactory httpClientFactory, IConfiguration configuration, IGenericRepository<Change_Notice_LogTable> change_Notice_LogTable = null)
		{
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
			_change_Notice_LogTable = change_Notice_LogTable;
		}

		public async Task SendFormDataCount()
		{
			try
			{

		
			//string connectionString = _configuration.GetConnectionString("Plm");
			//using IDbConnection connection = new SqlConnection(connectionString);
			string schema = _configuration["Catalog"];


			// Sorguları optimize edin ve sadece gerekli sütunları çekin
			//var Change_Notice_LogTableList = connection.Query<Change_Notice_LogTable>($"SELECT * FROM {schema}.Change_Notice_LogTable").OrderByDescending(x => x.ProcessTimestamp).ToList();
			var Change_Notice_LogTableList = (await _change_Notice_LogTable.GetAll(schema+ ".Change_Notice_LogTable")).OrderByDescending(x => x.ProcessTimestamp).ToList();

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

			
			//string connectionString = _configuration.GetConnectionString("Plm");
			//using IDbConnection connection = new SqlConnection(connectionString);
			string schema = _configuration["Catalog"];


				// Sorguları optimize edin ve sadece gerekli sütunları çekin
				//var Change_Notice_LogTableList = connection.Query<Change_Notice_LogTable>($"SELECT * FROM {schema}.Change_Notice_LogTable").OrderByDescending(x => x.ProcessTimestamp).ToList();
				var Change_Notice_LogTableList = (await _change_Notice_LogTable.GetAll(schema + ".Change_Notice_LogTable")).OrderByDescending(x => x.ProcessTimestamp).ToList();
				Change_Notice_LogTableList.ForEach(log =>
				{
					log.ProcessTimestamp = Convert.ToDateTime(log.ProcessTimestamp).AddHours(3);
				});
				var durumTakip = false;
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
