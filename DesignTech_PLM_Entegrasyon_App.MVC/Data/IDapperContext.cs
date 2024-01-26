using System.Data;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Data
{
	public interface IDapperContext
	{
		IDbConnection CreateConnection();
	}
}
