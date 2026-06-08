using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingNotes.MAUI.Core.Constants;
using MeetingNotes.MAUI.Core.Helpers;
using MeetingNotes.MAUI.Models;
using MeetingNotes.MAUI.Services.Interfaces;
using MeetingNotes.MAUI.ViewModels.Base;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace MeetingNotes.MAUI.ViewModels.Meetings;

public partial class MeetingsListViewModel : BaseViewModel
{
    private readonly IMeetingService _meetingService;
    private readonly ILocalCacheService _cacheService;

    [ObservableProperty]
    private ObservableCollection<MeetingDto> _meetings = new();

    [ObservableProperty]
    private ObservableCollection<MeetingCardUiModel> _meetingCards = new();

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
            MeetingCards = new ObservableCollection<MeetingCardUiModel>(Meetings.Select(MapToCard));
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

    [RelayCommand]
    private async Task OpenMenuAsync()
    {
        await Shell.Current.DisplayAlert("Menu", "Side menu can be connected here.", "OK");
    }

    [RelayCommand]
    private async Task OpenGlobalSearchAsync()
    {
        await Shell.Current.GoToAsync(NavigationRoutes.Search);
    }

    [RelayCommand]
    private async Task RetryMeetingAsync(MeetingDto? meeting)
    {
        if (meeting == null)
        {
            return;
        }

        await LoadMeetingsAsync();
    }

    private static MeetingCardUiModel MapToCard(MeetingDto meeting)
    {
        var normalizedStatus = (meeting.Status ?? string.Empty).Trim().ToLowerInvariant();
        var statusText = normalizedStatus switch
        {
            "completed" => "Ready",
            "ready" => "Ready",
            "failed" => "Failed",
            "processing" => "Processing",
            "transcribing" => "Processing",
            "summarising" => "Processing",
            "embedding" => "Processing",
            "pending" => "Pending",
            _ => "Pending"
        };

        var statusBackgroundColor = statusText switch
        {
            "Ready" => Color.FromArgb("#DDE7FF"),
            "Processing" => Color.FromArgb("#FFF1D6"),
            "Failed" => Color.FromArgb("#FCE8E8"),
            _ => Color.FromArgb("#EEF1F7")
        };

        var statusTextColor = statusText switch
        {
            "Ready" => Color.FromArgb("#2456B8"),
            "Processing" => Color.FromArgb("#A35B00"),
            "Failed" => Color.FromArgb("#B23A41"),
            _ => Color.FromArgb("#4D5364")
        };

        return new MeetingCardUiModel
        {
            Meeting = meeting,
            Title = meeting.Title,
            RelativeTime = DateTimeHelper.ToRelativeTime(meeting.MeetingDate),
            StatusText = statusText,
            StatusBackgroundColor = statusBackgroundColor,
            StatusTextColor = statusTextColor,
            StatusDotColor = statusTextColor,
            ShowSummaryTag = statusText == "Ready",
            SummaryTagText = "AI SUMMARY READY",
            ShowOverflow = statusText != "Failed",
            ShowRetry = statusText == "Failed"
        };
    }
}

public class MeetingCardUiModel
{
    public required MeetingDto Meeting { get; init; }
    public string Title { get; init; } = string.Empty;
    public string RelativeTime { get; init; } = string.Empty;

    public string StatusText { get; init; } = string.Empty;
    public Color StatusBackgroundColor { get; init; } = Colors.Transparent;
    public Color StatusTextColor { get; init; } = Colors.Black;
    public Color StatusDotColor { get; init; } = Colors.Black;

    public bool ShowSummaryTag { get; init; }
    public string SummaryTagText { get; init; } = string.Empty;

    public bool ShowOverflow { get; init; }
    public bool ShowRetry { get; init; }
}
