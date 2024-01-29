using Dapper;
using DesignTech_PLM_Entegrasyon_App.MVC.Data;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Repository
{
	public class GenericRepository<T> :IGenericRepository<T> where T : class
	{
		private readonly IDapperContext _context;
        public GenericRepository(IDapperContext context)
        {
            _context = context;
        }
		public async Task<IEnumerable<T>> GetAll(string tableName)
		{
			using (var connection = _context.CreateConnection())
			{
				string query = $"SELECT * FROM {tableName}";
				return await connection.QueryAsync<T>(query);
			}
		}

		public async Task<IEnumerable<T>> GetPageData(string tableName, int pageNumber, int pageSize)
		{
			try
			{
				using (var connection = _context.CreateConnection())
				{
					string query = $"SELECT * FROM {tableName} ORDER BY ProcessTimestamp OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY";
					return await connection.QueryAsync<T>(query);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}
		}


		public async Task<IEnumerable<T>> GetAllWithWhere(string tableName, DynamicParameters parameters)
		{
			using (var connection = _context.CreateConnection())
			{
				string query = $"SELECT * FROM {tableName} WHERE ";
				query += string.Join(" AND ", parameters.ParameterNames.Select(value => $"{value} = @{value}"));
				return await connection.QueryAsync<T>(query,parameters);
			}
		}
		public async Task<T> GetById(string tableName, string idA2A2)
		{
			using (var connection = _context.CreateConnection())
			{
				string query = $"SELECT * FROM {tableName} WHERE idA2A2 = @idA2A2";
				return await connection.QuerySingleOrDefaultAsync<T>(query, new {idA2A2 = idA2A2});
			}
		}
		public async Task Delete(string tableName, string idA2A2)
		{
			using (var connection = _context.CreateConnection())
			{
				string query = $"DELETE FROM {tableName} WHERE idA2A2 = @idA2A2";
				await connection.ExecuteAsync(query, new { idA2A2 = idA2A2 });
			}
		}

		public async Task Add(string tableName, T entity)
		{
			using (var connection = _context.CreateConnection())
			{
				var _EntityTypeOf = typeof(T);
				var _GetProperties = _EntityTypeOf.GetProperties().Where(x => x.Name != "idA2A2");
				DynamicParameters _DynamicParameters = new();

				foreach (var property in _GetProperties)
				{
					var value =  property.GetValue(entity);
					_DynamicParameters.Add("@"+property.Name, value);
				}
				var idProperty = _EntityTypeOf.GetProperty("idA2A2");
				if(idProperty != null)
				{
					string instertQuery = $"INSERT INTO {tableName} ({string.Join(", ", _GetProperties.Select(p => p.Name))})" + $"VALUES ({string.Join(", ", _GetProperties.Select(p => "@" + p.Name))})";
					await connection.ExecuteAsync(instertQuery, _DynamicParameters);
				}
				else
				{
					throw new ArgumentException("Entity must have an 'idA2A2' property");
				}
			}
		}



		public async Task Update(string tableName, T entity)
		{
			using (var connection = _context.CreateConnection())
			{
				var _EntityTypeOf = typeof(T);
				var _GetProperties = _EntityTypeOf.GetProperties().Where(x => x.Name != "idA2A2");
				DynamicParameters _DynamicParameters = new();

				foreach (var property in _GetProperties)
				{
					var value = property.GetValue(entity);
					_DynamicParameters.Add("@" + property.Name, value);
				}
				var idProperty = _EntityTypeOf.GetProperty("idA2A2");
				if (idProperty != null)
				{
					string updateQuery = $"UPDATE {tableName} SET ({string.Join(", ", _GetProperties.Where(p => p.Name != "idA2A2").Select(p => p.Name + "= @"+p.Name))} " + $"WHERE idA2A2 = @idA2A2";
					await connection.ExecuteAsync(updateQuery, _DynamicParameters);
				}
				else
				{
					throw new ArgumentException("Entity must have an 'idA2A2' property");
				}
			}
		}

	
	}
}
