using Microsoft.Data.SqlClient;
using System.Data;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models.DapperContext
{
	public class Context
	{
		private readonly IConfiguration _configuration;
		private readonly string _connectionString;
		public Context(IConfiguration configuration)
		{
			_configuration = configuration;
			_connectionString = _configuration.GetConnectionString("Plm");
		}

		public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
	}
}
