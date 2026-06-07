using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MeetingNotes.MAUI.Core.Constants;
using MeetingNotes.MAUI.Http;
using MeetingNotes.MAUI.Services.Api;
using MeetingNotes.MAUI.Services.Export;
using MeetingNotes.MAUI.Services.Interfaces;
using MeetingNotes.MAUI.Services.Local;
using MeetingNotes.MAUI.Services.LocalApi;
using MeetingNotes.MAUI.Services.Platform;
using MeetingNotes.MAUI.ViewModels.Auth;
using MeetingNotes.MAUI.ViewModels.Content;
using MeetingNotes.MAUI.ViewModels.Meetings;
using MeetingNotes.MAUI.ViewModels.Profile;

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
#endif

		builder.Services.AddSingleton<ISecureTokenService, SecureTokenService>();
		builder.Services.AddSingleton<ILocalCacheService, LocalCacheService>();
		builder.Services.AddSingleton<IAudioRecordingService, AudioRecordingService>();
		builder.Services.AddSingleton<LocalApiHttpMessageHandler>();

		builder.Services.AddHttpClient<IMeetingService, MeetingService>(client =>
		{
			client.BaseAddress = new Uri("https://local.api");
			client.Timeout = TimeSpan.FromSeconds(AppConstants.ApiTimeoutSeconds);
		}).ConfigurePrimaryHttpMessageHandler<LocalApiHttpMessageHandler>();

		builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
		{
			client.BaseAddress = new Uri("https://local.api");
			client.Timeout = TimeSpan.FromSeconds(AppConstants.ApiTimeoutSeconds);
		}).ConfigurePrimaryHttpMessageHandler<LocalApiHttpMessageHandler>();

		builder.Services.AddHttpClient<IChatService, ChatService>(client =>
		{
			client.BaseAddress = new Uri("https://local.api");
			client.Timeout = TimeSpan.FromSeconds(AppConstants.ApiTimeoutSeconds);
		}).ConfigurePrimaryHttpMessageHandler<LocalApiHttpMessageHandler>();

		builder.Services.AddTransient<IExportService, ExportService>();

		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<RegisterViewModel>();
		builder.Services.AddTransient<ForgotPasswordViewModel>();
		builder.Services.AddTransient<ChatViewModel>();
		builder.Services.AddTransient<MeetingsListViewModel>();
		builder.Services.AddTransient<ProfileViewModel>();

		return builder.Build();
	}
}
