using Dapper;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Repository
{
	public interface IGenericRepository<T> where T : class
	{

		Task<IEnumerable<T>> GetPageData(string tableName, int pageNumber, int pageSize);
		Task<IEnumerable<T>> GetAll(string tableName);
		Task<IEnumerable<T>> GetAllWithWhere(string tableName,DynamicParameters parameters);
		Task<T> GetById(string tableName, string idA2A2);
		Task Delete(string tableName, string idA2A2);
		Task Add(string tableName, T entity);
		Task Update(string tableName, T entity);
	}
}
