namespace DesignTech_PLM_Entegrasyon_App.MVC.Middleware
{
	public class LogMiddleware
	{
		private readonly RequestDelegate _next;

		public LogMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			Console.WriteLine("LogMiddleware Before");
			await _next(context);
			Console.WriteLine("LogMiddleware After");
		}
	}
}
