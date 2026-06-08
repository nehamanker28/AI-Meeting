using MeetingNotes.MAUI.Core.Helpers;
using MeetingNotes.MAUI.ViewModels.Profile;

namespace MeetingNotes.MAUI.Views.Meeting;

public partial class ProfileTabPage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfileTabPage()
    {
        InitializeComponent();
        _viewModel = ServiceResolver.GetRequiredService<ProfileViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.OnNavigatedToAsync();
    }
}
