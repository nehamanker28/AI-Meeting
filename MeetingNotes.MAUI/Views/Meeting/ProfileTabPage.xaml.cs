using MeetingNotes.MAUI.ViewModels.Profile;

namespace MeetingNotes.MAUI.Views.Meeting;

public partial class ProfileTabPage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfileTabPage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    // protected override async void OnAppearing()
    // {
    //     base.OnAppearing();

    //     await _viewModel.OnNavigatedToAsync();
    // }
}
