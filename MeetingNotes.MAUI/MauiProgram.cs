using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MeetingNotes.MAUI.Core.Constants;
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
using MeetingNotes.MAUI.Views.Meeting;
using MeetingNotes.MAUI.Views.create_meeting;

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
		builder.Services.AddScoped<IAuthService,MockAuthService>();
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

		// builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
		// {
		// 	client.BaseAddress = new Uri("https://local.api");
		// 	client.Timeout = TimeSpan.FromSeconds(AppConstants.ApiTimeoutSeconds);
		// }).ConfigurePrimaryHttpMessageHandler<LocalApiHttpMessageHandler>();

		// builder.Services.AddHttpClient<IChatService, ChatService>(client =>
		// {
		// 	client.BaseAddress = new Uri("https://local.api");
		// 	client.Timeout = TimeSpan.FromSeconds(AppConstants.ApiTimeoutSeconds);
		// }).ConfigurePrimaryHttpMessageHandler<LocalApiHttpMessageHandler>();

		// builder.Services.AddTransient<IExportService, ExportService>();
		builder.Services.AddSingleton<AppShell>();
		builder.Services.AddTransient<MeetingsTabPage>();
		builder.Services.AddTransient<ProfileTabPage>();
		builder.Services.AddTransient<CreateMeetingPage>();

		// builder.Services.AddTransient<LoginViewModel>();
		// builder.Services.AddTransient<RegisterViewModel>();
		// builder.Services.AddTransient<ForgotPasswordViewModel>();
		// builder.Services.AddTransient<ChatViewModel>();
		builder.Services.AddTransient<MeetingsListViewModel>();
		builder.Services.AddTransient<ProfileViewModel>();
		builder.Services.AddTransient<CreateMeetingViewModel>();

		return builder.Build();
	}
}
