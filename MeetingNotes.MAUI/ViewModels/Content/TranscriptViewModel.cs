using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MeetingNotes.MAUI.Models;
using MeetingNotes.MAUI.Services.Interfaces;
using MeetingNotes.MAUI.ViewModels.Base;
using Microsoft.Maui.Controls;

namespace MeetingNotes.MAUI.ViewModels.Content;

public partial class TranscriptViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IMeetingService _meetingService;
    private readonly ILocalCacheService _cacheService;
    private Guid _meetingId;

    [ObservableProperty]
    private TranscriptDto? _transcript;

    public TranscriptViewModel(IMeetingService meetingService, ILocalCacheService cacheService)
    {
        Title = "Transcript";
        _meetingService = meetingService;
        _cacheService = cacheService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) && idObj != null)
        {
            _meetingId = Guid.Parse(idObj.ToString()!);
            _ = LoadTranscriptAsync();
        }
    }

    public async Task LoadTranscriptAsync()
    {
        IsBusy = true;
        ClearError();
        try
        {
            TranscriptDto transcript;
            try
            {
                transcript = await _meetingService.GetTranscriptAsync(_meetingId);
            }
            catch
            {
                var detail = await _cacheService.GetMeetingDetailAsync(_meetingId);
                if (detail?.Transcript != null)
                {
                    transcript = detail.Transcript;
                    SetError("Offline: Loaded from cache.");
                }
                else
                {
                    throw;
                }
            }

            Transcript = transcript;
        }
        catch (Exception ex)
        {
            SetError("Failed to load transcript.");
            System.Diagnostics.Debug.WriteLine($"LoadTranscript error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
