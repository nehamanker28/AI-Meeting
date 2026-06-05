using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingNotes.MAUI.Core.Constants;
using MeetingNotes.MAUI.Models;
using MeetingNotes.MAUI.Services.Interfaces;
using MeetingNotes.MAUI.ViewModels.Base;
using Microsoft.Maui.Controls;

namespace MeetingNotes.MAUI.ViewModels.Meetings;

public partial class MeetingsListViewModel : BaseViewModel
{
    private readonly IMeetingService _meetingService;
    private readonly ILocalCacheService _cacheService;

    [ObservableProperty]
    private ObservableCollection<MeetingDto> _meetings = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _isEmpty;

    public MeetingsListViewModel(IMeetingService meetingService, ILocalCacheService cacheService)
    {
        Title = "Meetings";
        _meetingService = meetingService;
        _cacheService = cacheService;
    }

    partial void OnSearchQueryChanged(string value)
    {
        SearchCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadMeetingsAsync()
    {
        IsBusy = true;
        ClearError();
        try
        {
            List<MeetingDto> result;
            try
            {
                result = await _meetingService.GetMeetingsAsync(SearchQuery);
                if (string.IsNullOrEmpty(SearchQuery))
                {
                    await _cacheService.SaveMeetingsAsync(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load from API: {ex.Message}");
                result = await _cacheService.GetMeetingsAsync(SearchQuery);
                SetError("Offline mode: Loaded from local cache.");
            }

            Meetings = new ObservableCollection<MeetingDto>(result);
            IsEmpty = !Meetings.Any();
        }
        catch (Exception ex)
        {
            SetError("Failed to load meetings.");
            System.Diagnostics.Debug.WriteLine($"LoadMeetings error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadMeetingsAsync();
    }

    [RelayCommand]
    private async Task NavigateToMeetingAsync(MeetingDto meeting)
    {
        if (meeting == null) return;
        await Shell.Current.GoToAsync($"{NavigationRoutes.MeetingDetail}?id={meeting.Id}");
    }

    [RelayCommand]
    private async Task DeleteMeetingAsync(MeetingDto meeting)
    {
        if (meeting == null) return;

        bool confirmed = await Shell.Current.DisplayAlert(
            "Delete Meeting",
            $"Are you sure you want to delete '{meeting.Title}'? This cannot be undone.",
            "Delete", "Cancel");

        if (!confirmed) return;

        Meetings.Remove(meeting);
        IsEmpty = !Meetings.Any();

        try
        {
            await _meetingService.DeleteMeetingAsync(meeting.Id);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to delete from API: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "Failed to delete from server. Try again when online.", "OK");
            await LoadMeetingsAsync();
        }
    }

    [RelayCommand]
    private async Task NavigateToCreateAsync()
    {
        await Shell.Current.GoToAsync(NavigationRoutes.CreateMeeting);
    }
}
