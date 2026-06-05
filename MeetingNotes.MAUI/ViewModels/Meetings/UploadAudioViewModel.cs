using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingNotes.MAUI.Core.Constants;
using MeetingNotes.MAUI.Core.Helpers;
using MeetingNotes.MAUI.Services.Interfaces;
using MeetingNotes.MAUI.ViewModels.Base;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace MeetingNotes.MAUI.ViewModels.Meetings;

public partial class UploadAudioViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IMeetingService _meetingService;
    private Guid _meetingId;

    [ObservableProperty]
    private string _selectedFileName = "No file selected";

    [ObservableProperty]
    private string _selectedFileSize = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UploadCommand))]
    private bool _isFileSelected;

    [ObservableProperty]
    private bool _isUploading;

    [ObservableProperty]
    private double _uploadProgress;

    private FileResult? _selectedFile;

    public UploadAudioViewModel(IMeetingService meetingService)
    {
        Title = "Upload Audio";
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
    private async Task SelectFileAsync()
    {
        ClearError();
        try
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { "public.audio" } },
                { DevicePlatform.Android, new[] { "audio/*" } },
                { DevicePlatform.WinUI, new[] { ".mp3", ".wav", ".m4a", ".aac", ".ogg" } }
            });

            var options = new PickOptions
            {
                PickerTitle = "Please select an audio file",
                FileTypes = customFileType
            };

            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                var length = stream.Length;

                if (length > AppConstants.MaxAudioFileSizeInBytes)
                {
                    SetError($"File exceeds the maximum limit of {FileSizeHelper.FormatBytes(AppConstants.MaxAudioFileSizeInBytes)}");
                    return;
                }

                _selectedFile = result;
                SelectedFileName = result.FileName;
                SelectedFileSize = FileSizeHelper.FormatBytes(length);
                IsFileSelected = true;
            }
        }
        catch (Exception ex)
        {
            SetError($"Error selecting file: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(IsFileSelected))]
    private async Task UploadAsync()
    {
        if (_selectedFile == null) return;

        IsUploading = true;
        UploadProgress = 0;
        ClearError();

        try
        {
            using var stream = await _selectedFile.OpenReadAsync();
            await _meetingService.UploadAudioAsync(_meetingId, stream, _selectedFile.FileName, progress =>
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
}
