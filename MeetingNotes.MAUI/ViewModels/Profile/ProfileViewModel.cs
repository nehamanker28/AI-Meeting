using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingNotes.MAUI.Core.Constants;
using MeetingNotes.MAUI.Services.Interfaces;
using MeetingNotes.MAUI.ViewModels.Base;
using Microsoft.Maui.Controls;

namespace MeetingNotes.MAUI.ViewModels.Profile;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    public ProfileViewModel(IAuthService authService)
    {
        Title = "Profile";
        _authService = authService;
    }

    public override async Task OnNavigatedToAsync()
    {
        await LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        IsBusy = true;
        ClearError();
        try
        {
            var user = await _authService.GetProfileAsync();
            if (user != null)
            {
                FullName = user.FullName;
                Email = user.Email;
            }
        }
        catch (Exception ex)
        {
            SetError("Failed to load profile.");
            System.Diagnostics.Debug.WriteLine($"LoadProfile error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName))
        {
            SetError("Name cannot be empty.");
            return;
        }

        IsBusy = true;
        ClearError();
        try
        {
            var success = await _authService.UpdateProfileAsync(FullName, null);
            if (success)
            {
                await Shell.Current.DisplayAlert("Success", "Profile updated successfully.", "OK");
            }
            else
            {
                SetError("Failed to update profile.");
            }
        }
        catch (Exception ex)
        {
            SetError("An error occurred. Try again.");
            System.Diagnostics.Debug.WriteLine($"SaveProfile error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert("Logout", "Are you sure you want to logout?", "Logout", "Cancel");
        if (!confirm) return;

        IsBusy = true;
        try
        {
            await _authService.LogoutAsync();
            await Shell.Current.GoToAsync($"///{NavigationRoutes.Login}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
            await Shell.Current.GoToAsync($"///{NavigationRoutes.Login}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
