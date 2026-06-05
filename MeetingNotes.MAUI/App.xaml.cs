namespace MeetingNotes.MAUI;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());

#if DEBUG
		// In local development mode, skip the login screen and navigate directly
		// to the Meetings list so developers can iterate faster.
		try
		{
			Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(async () =>
			{
				// Use absolute route to replace the navigation stack
				await Shell.Current.GoToAsync($"//{Core.Constants.NavigationRoutes.MeetingsList}");
			});
		}
		catch
		{
			// swallow any navigation errors during startup in DEBUG
		}
#endif

		return window;
	}
}