namespace DesignTech_PLM_Entegrasyon_App.MVC.Middleware
{
	public class ErrorMiddleware
	{
		private readonly RequestDelegate _next;

		public ErrorMiddleware(RequestDelegate next)
		{
			_next = next;
		}


		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
			await _next(context);
			}
			catch (Exception ex)
			{
				Console.WriteLine("");
				throw;
			}
		}

	}
}
