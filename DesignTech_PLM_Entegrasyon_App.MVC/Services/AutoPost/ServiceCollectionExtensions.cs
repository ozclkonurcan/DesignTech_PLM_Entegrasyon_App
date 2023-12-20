using DesignTech_PLM_Entegrasyon_App.MVC.Helper;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddChangeNoticeIfValid(this IServiceCollection services)
	{
		if (CanAddService())
		{
			services.AddHostedService<ChangeNoticeService>();
		}

		return services;
	}

	private static bool CanAddService()
	{
		// Gerekli kontroller
		return true;
	}
}