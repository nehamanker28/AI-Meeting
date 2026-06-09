using MeetingNotes.MAUI.ViewModels.Meetings;

namespace MeetingNotes.MAUI.Views.create_meeting;

public partial class CreateMeetingPage : ContentPage
{
    public CreateMeetingPage(CreateMeetingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnHelpClicked(object? sender, EventArgs e)
    {
        await DisplayAlert("Upload Audio", "Use local archive to pick a file or paste a remote source URL to process.", "OK");
    }
}
