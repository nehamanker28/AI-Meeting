namespace MeetingNotes.MAUI;

public partial class AppShell : Shell
{
	public AppShell(Views.Meeting.MeetingsTabPage meetingsTabPage, Views.Meeting.ProfileTabPage profileTabPage)
	{
		InitializeComponent();

		Items.Add(new TabBar
		{
			Items =
			{
				new Tab
				{
					Title = "Meetings",
					Route = "MeetingsTab",
					Items =
					{
						new ShellContent
						{
							Route = "MeetingsTabPage",
							Content = meetingsTabPage
						}
					}
				},
				new Tab
				{
					Title = "Profile",
					Route = "ProfileTab",
					Items =
					{
						new ShellContent
						{
							Route = "ProfileTabPage",
							Content = profileTabPage
						}
					}
				}
			}
		});

		Routing.RegisterRoute(Core.Constants.NavigationRoutes.CreateMeeting,
			new Core.Helpers.DiRouteFactory<Views.create_meeting.CreateMeetingPage>());
	}
}
