using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingNotes.MAUI.Core.Constants;
using MeetingNotes.MAUI.Services.Interfaces;
using MeetingNotes.MAUI.ViewModels.Base;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace MeetingNotes.MAUI.ViewModels.Meetings;

public partial class RecordingViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IAudioRecordingService _recorder;
    private readonly IMeetingService _meetingService;
    private Guid _meetingId;
    private CancellationTokenSource? _timerCts;

    [ObservableProperty]
    private TimeSpan _elapsed;

    [ObservableProperty]
    private bool _isRecording;

    [ObservableProperty]
    private bool _isPaused;

    [ObservableProperty]
    private bool _isUploading;

    [ObservableProperty]
    private double _uploadProgress;

    public RecordingViewModel(IAudioRecordingService recorder, IMeetingService meetingService)
    {
        Title = "Record Meeting";
        _recorder = recorder;
        _meetingService = meetingService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) && idObj != null)
        {
            _meetingId = Guid.Parse(idObj.ToString()!);
        }
    }

    [RelayCommand]
    private async Task StartRecordingAsync()
    {
        ClearError();
        try
        {
            var extension = DeviceInfo.Platform == DevicePlatform.iOS ? "m4a" : "mp4";
            var path = Path.Combine(FileSystem.CacheDirectory, $"{_meetingId}.{extension}");
            
            await _recorder.StartAsync(path);
            IsRecording = true;
            IsPaused = false;
            Elapsed = TimeSpan.Zero;
            
            _timerCts?.Cancel();
            _timerCts = new CancellationTokenSource();
            _ = RunTimerAsync(_timerCts.Token);
        }
        catch (Exception ex)
        {
            SetError($"Failed to start recording: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task PauseResumeAsync()
    {
        if (IsPaused)
        {
            await _recorder.ResumeAsync();
            IsPaused = false;
        }
        else
        {
            await _recorder.PauseAsync();
            IsPaused = true;
        }
    }

    [RelayCommand]
    private async Task StopAndUploadAsync()
    {
        _timerCts?.Cancel();
        
        IsRecording = false;
        IsPaused = false;
        IsUploading = true;
        UploadProgress = 0;
        
        try
        {
            var filePath = await _recorder.StopAsync();
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                throw new Exception("Recording file not found.");
            }

            using var stream = File.OpenRead(filePath);
            var fileName = Path.GetFileName(filePath);
            
            await _meetingService.UploadAudioAsync(_meetingId, stream, fileName, progress =>
            {
                UploadProgress = progress;
            });
            
            await Shell.Current.GoToAsync($"///{NavigationRoutes.MeetingsList}/{NavigationRoutes.MeetingDetail}?id={_meetingId}");
        }
        catch (Exception ex)
        {
            SetError($"Upload failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"UploadAudio error: {ex.Message}");
        }
        finally
        {
            IsUploading = false;
        }
    }

    private async Task RunTimerAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(1000, token);
                if (!IsPaused)
                {
                    Elapsed = _recorder.Elapsed;
                }
            }
        }
        catch (TaskCanceledException)
        {
            // Ignored
        }
    }
}
