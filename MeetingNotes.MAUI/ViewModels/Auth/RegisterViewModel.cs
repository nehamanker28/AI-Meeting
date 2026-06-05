using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingNotes.MAUI.Core.Constants;
using MeetingNotes.MAUI.Services.Interfaces;
using MeetingNotes.MAUI.ViewModels.Base;
using Microsoft.Maui.Controls;

namespace MeetingNotes.MAUI.ViewModels.Auth;

public partial class RegisterViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    public RegisterViewModel(IAuthService authService)
    {
        Title = "Register";
        _authService = authService;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || 
            string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            SetError("All fields are required.");
            return;
        }

        if (Password.Length < 8)
        {
            SetError("Password must be at least 8 characters long.");
            return;
        }

        if (Password != ConfirmPassword)
        {
            SetError("Passwords do not match.");
            return;
        }

        IsBusy = true;
        ClearError();

        try
        {
            var success = await _authService.RegisterAsync(FullName, Email, Password);
            if (success)
            {
                await Shell.Current.GoToAsync($"//{NavigationRoutes.MeetingsList}");
            }
            else
            {
                SetError("Registration failed. This email may already be registered.");
            }
        }
        catch
        {
            SetError("An error occurred during registration. Please try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToLoginAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
