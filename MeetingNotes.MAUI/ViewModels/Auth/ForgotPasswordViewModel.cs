using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingNotes.MAUI.Services.Interfaces;
using MeetingNotes.MAUI.ViewModels.Base;
using Microsoft.Maui.Controls;

namespace MeetingNotes.MAUI.ViewModels.Auth;

public partial class ForgotPasswordViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private bool _isSuccess;

    public ForgotPasswordViewModel(IAuthService authService)
    {
        Title = "Forgot Password";
        _authService = authService;
    }

    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            SetError("Email address is required.");
            return;
        }

        IsBusy = true;
        ClearError();
        IsSuccess = false;

        try
        {
            var success = await _authService.ForgotPasswordAsync(Email);
            if (success)
            {
                IsSuccess = true;
            }
            else
            {
                SetError("Failed to send reset email. Verify the address.");
            }
        }
        catch
        {
            SetError("An error occurred. Please try again.");
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
