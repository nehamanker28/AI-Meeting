using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingNotes.MAUI.Core.Constants;
using MeetingNotes.MAUI.Models;
using MeetingNotes.MAUI.Services.Interfaces;
using MeetingNotes.MAUI.ViewModels.Base;
using Microsoft.Maui.Controls;

namespace MeetingNotes.MAUI.ViewModels.Meetings;

public partial class MeetingDetailViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IMeetingService _meetingService;
    private readonly ILocalCacheService _cacheService;
    private readonly IExportService _exportService;
    private Guid _meetingId;
    private CancellationTokenSource? _pollingCts;

    [ObservableProperty]
    private MeetingDetailDto? _meeting;

    [ObservableProperty]
    private string _status = "pending";

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private bool _isReady;

    [ObservableProperty]
    private bool _isFailed;

    public MeetingDetailViewModel(IMeetingService meetingService, ILocalCacheService cacheService, IExportService exportService)
    {
        _meetingService = meetingService;
        _cacheService = cacheService;
        _exportService = exportService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) && idObj != null)
        {
            _meetingId = Guid.Parse(idObj.ToString()!);
            Title = "Meeting Detail";
            _ = LoadDetailsAsync();
        }
    }

    public async Task LoadDetailsAsync()
    {
        IsBusy = true;
        ClearError();
        _pollingCts?.Cancel();

        try
        {
            MeetingDetailDto detail;
            try
            {
                detail = await _meetingService.GetMeetingAsync(_meetingId);
                await _cacheService.SaveMeetingDetailAsync(detail);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load detail from API: {ex.Message}");
                var cached = await _cacheService.GetMeetingDetailAsync(_meetingId);
                if (cached != null)
                {
                    detail = cached;
                    SetError("Offline mode: Loaded from local cache.");
                }
                else
                {
                    throw;
                }
            }

            Meeting = detail;
            Status = detail.Status;
            UpdateState(Status);

            if (IsProcessing)
            {
                _pollingCts = new CancellationTokenSource();
                _ = StartPollingStatusAsync(_pollingCts.Token);
            }
        }
        catch (Exception ex)
        {
            SetError("Failed to load meeting details.");
            System.Diagnostics.Debug.WriteLine($"LoadDetails error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateState(string currentStatus)
    {
        IsProcessing = currentStatus is "transcribing" or "summarising" or "embedding" or "pending";
        IsReady = currentStatus == "completed";
        IsFailed = currentStatus == "failed";
    }

    private async Task StartPollingStatusAsync(CancellationToken token)
    {
        var attempts = 0;
        try
        {
            while (!token.IsCancellationRequested && attempts < AppConstants.MaxPollAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(AppConstants.PollIntervalSeconds), token);
                attempts++;

                var statusDto = await _meetingService.GetStatusAsync(_meetingId);
                Status = statusDto.Status;
                UpdateState(Status);

                if (IsReady || IsFailed)
                {
                    await LoadDetailsAsync();
                    break;
                }
            }
        }
        catch (TaskCanceledException)
        {
            // Ignored
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Polling error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ExportAsync(string format)
    {
        if (Meeting == null) return;

        IsBusy = true;
        try
        {
            await _exportService.ExportAndShareAsync(_meetingId, Meeting.Title, format);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Export Failed", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void OnNavigatedFrom()
    {
        _pollingCts?.Cancel();
    }
}
