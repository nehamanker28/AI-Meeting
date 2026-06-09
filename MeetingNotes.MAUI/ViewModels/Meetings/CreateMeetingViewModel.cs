using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingNotes.MAUI.Core.Constants;
using MeetingNotes.MAUI.Services.Interfaces;
using MeetingNotes.MAUI.ViewModels.Base;
using Microsoft.Maui.Controls;

namespace MeetingNotes.MAUI.ViewModels.Meetings;

public partial class CreateMeetingViewModel : BaseViewModel
{
    private readonly IMeetingService _meetingService;

    [ObservableProperty]
    private string _meetingTitle = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private DateTime _meetingDate = DateTime.Today;

    [ObservableProperty]
    private string _remoteSourceUrl = string.Empty;

    public CreateMeetingViewModel(IMeetingService meetingService)
    {
        Title = "New Meeting";
        _meetingService = meetingService;
    }

    [RelayCommand]
    private async Task CreateAndRecordAsync()
    {
        var id = await SaveMeetingAsync();
        if (id != Guid.Empty)
        {
            await Shell.Current.GoToAsync($"{NavigationRoutes.RecordMeeting}?id={id}");
        }
    }

    [RelayCommand]
    private async Task CreateAndUploadAsync()
    {
        var id = await SaveMeetingAsync();
        if (id != Guid.Empty)
        {
            await Shell.Current.GoToAsync($"{NavigationRoutes.UploadAudio}?id={id}");
        }
    }

    [RelayCommand]
    private async Task ProcessRemoteLinkAsync()
    {
        if (string.IsNullOrWhiteSpace(RemoteSourceUrl))
        {
            SetError("Please paste a source link.");
            return;
        }

        if (string.IsNullOrWhiteSpace(MeetingTitle))
        {
            MeetingTitle = $"Remote Import {DateTime.Now:MMM d, yyyy h:mm tt}";
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            Description = $"Source URL: {RemoteSourceUrl}";
        }

        await CreateAndUploadAsync();
    }

    private async Task<Guid> SaveMeetingAsync()
    {
        if (string.IsNullOrWhiteSpace(MeetingTitle))
        {
            SetError("Title is required.");
            return Guid.Empty;
        }

        IsBusy = true;
        ClearError();

        try
        {
            var id = await _meetingService.CreateMeetingAsync(MeetingTitle, MeetingDate, Description);
            return id;
        }
        catch (Exception ex)
        {
            SetError("Failed to create meeting. Please check your connection.");
            System.Diagnostics.Debug.WriteLine($"SaveMeeting error: {ex.Message}");
            return Guid.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
