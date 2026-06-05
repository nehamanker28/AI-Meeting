using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MeetingNotes.MAUI.Models;
using MeetingNotes.MAUI.Services.Interfaces;
using MeetingNotes.MAUI.ViewModels.Base;
using Microsoft.Maui.Controls;

namespace MeetingNotes.MAUI.ViewModels.Content;

public partial class SummaryViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IMeetingService _meetingService;
    private readonly ILocalCacheService _cacheService;
    private Guid _meetingId;

    [ObservableProperty]
    private SummaryDto? _summary;

    public SummaryViewModel(IMeetingService meetingService, ILocalCacheService cacheService)
    {
        Title = "Summary";
        _meetingService = meetingService;
        _cacheService = cacheService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) && idObj != null)
        {
            _meetingId = Guid.Parse(idObj.ToString()!);
            _ = LoadSummaryAsync();
        }
    }

    public async Task LoadSummaryAsync()
    {
        IsBusy = true;
        ClearError();
        try
        {
            SummaryDto summary;
            try
            {
                summary = await _meetingService.GetSummaryAsync(_meetingId);
            }
            catch
            {
                var detail = await _cacheService.GetMeetingDetailAsync(_meetingId);
                if (detail?.Summary != null)
                {
                    summary = detail.Summary;
                    SetError("Offline: Loaded from cache.");
                }
                else
                {
                    throw;
                }
            }

            Summary = summary;
        }
        catch (Exception ex)
        {
            SetError("Failed to load summary.");
            System.Diagnostics.Debug.WriteLine($"LoadSummary error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
