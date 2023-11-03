using Microsoft.Data.SqlClient;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Dtos
{
    public class PlmDatabase
    {
        private readonly IConfiguration _configuration;

        public PlmDatabase(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public QueryFactory Connect()
        {
            string connectionString = _configuration.GetConnectionString("Plm");
            var connection = new SqlConnection(connectionString);
            var compiler = new SqlServerCompiler();

            var db = new QueryFactory(connection, compiler);
            return db;
        }
    }
}
