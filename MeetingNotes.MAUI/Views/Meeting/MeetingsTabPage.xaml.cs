using CommunityToolkit.Mvvm.Input;
using MeetingNotes.MAUI.ViewModels.Meetings;

namespace MeetingNotes.MAUI.Views.Meeting;

public partial class MeetingsTabPage : ContentPage
{
    private readonly MeetingsListViewModel _viewModel;

    public MeetingsTabPage(MeetingsListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.MeetingCards.Count == 0 && _viewModel.LoadMeetingsCommand is IAsyncRelayCommand loadCommand)
        {
            await loadCommand.ExecuteAsync(null);
        }
        else
        {
            await _viewModel.OnNavigatedToAsync();
        }
    }
}
