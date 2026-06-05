using Microsoft.Extensions.Logging;

namespace MeetingNotes.MAUI;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();

		// Register mock/local services for faster local development and to avoid
		// making real API calls. These are only registered in DEBUG builds.
		builder.Services.AddSingleton<Services.Interfaces.ISecureTokenService, Services.Local.MockSecureTokenService>();
		builder.Services.AddScoped<Services.Interfaces.IAuthService, Services.Local.MockAuthService>();
#endif

		return builder.Build();
	}
}
