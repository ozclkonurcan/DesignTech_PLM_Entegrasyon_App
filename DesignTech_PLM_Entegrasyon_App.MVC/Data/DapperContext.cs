using Microsoft.Data.SqlClient;
using System.Data;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Data
{
	public class DapperContext : IDapperContext
	{

		private readonly IConfiguration _configuration;
		private readonly string _connString;

		public DapperContext(IConfiguration configuration)
		{
			_configuration = configuration;
			_connString = _configuration.GetConnectionString("Plm");
		}

		public IDbConnection CreateConnection() => new SqlConnection(_connString);
	}
}
