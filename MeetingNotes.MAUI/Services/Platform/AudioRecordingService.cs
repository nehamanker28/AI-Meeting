using System;
using System.IO;
using System.Threading.Tasks;
using MeetingNotes.MAUI.Services.Interfaces;

#if ANDROID
using Android.Media;
#elif IOS || MACCATALYST
using AVFoundation;
using AudioToolbox;
using Foundation;
#endif

namespace MeetingNotes.MAUI.Services.Platform;

public class AudioRecordingService : IAudioRecordingService
{
    private DateTime? _startTime;
    private TimeSpan _accumulatedTime = TimeSpan.Zero;
    private bool _isRecording;
    private bool _isPaused;
    private string? _outputPath;

#if ANDROID
    private MediaRecorder? _mediaRecorder;
#elif IOS || MACCATALYST
    private AVAudioRecorder? _recorder;
#endif

    public TimeSpan Elapsed
    {
        get
        {
            if (!_isRecording) return TimeSpan.Zero;
            if (_isPaused) return _accumulatedTime;
            var startTime = _startTime ?? DateTime.UtcNow;
            return _accumulatedTime + (DateTime.UtcNow - startTime);
        }
    }

    public async Task StartAsync(string outputPath)
    {
        _outputPath = outputPath;

        var status = await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<Microsoft.Maui.ApplicationModel.Permissions.Microphone>();
        if (status != Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
        {
            throw new Exception("Microphone permission not granted.");
        }

#if ANDROID
#pragma warning disable CA1422
        _mediaRecorder = new MediaRecorder();
        _mediaRecorder.SetAudioSource(AudioSource.Mic);
        _mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
        _mediaRecorder.SetAudioEncoder(AudioEncoder.Aac);
        _mediaRecorder.SetOutputFile(outputPath);
        _mediaRecorder.Prepare();
        _mediaRecorder.Start();
#pragma warning restore CA1422
#elif IOS || MACCATALYST
        var audioSession = AVAudioSession.SharedInstance();
        audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord, AVAudioSessionCategoryOptions.DefaultToSpeaker);
        audioSession.SetActive(true);

        var url = NSUrl.FromFilename(outputPath);
        
        var keys = new NSObject[]
        {
            AVAudioSettings.AVFormatIDKey,
            AVAudioSettings.AVSampleRateKey,
            AVAudioSettings.AVNumberOfChannelsKey,
            AVAudioSettings.AVEncoderAudioQualityKey
        };

        var values = new NSObject[]
        {
            NSNumber.FromInt32((int)AudioFormatType.MPEG4AAC),
            NSNumber.FromFloat(44100.0f),
            NSNumber.FromInt32(1),
            NSNumber.FromInt32((int)AVAudioQuality.High)
        };

        var settingsDict = NSDictionary.FromObjectsAndKeys(values, keys);
        NSError? error;
        _recorder = AVAudioRecorder.Create(url, new AudioSettings(settingsDict), out error);
        
        if (error != null || _recorder == null)
        {
            throw new Exception($"Failed to create audio recorder: {error?.LocalizedDescription}");
        }

        _recorder.Record();
#else
        System.Diagnostics.Debug.WriteLine($"Mock Recording started at: {outputPath}");
#endif

        _startTime = DateTime.UtcNow;
        _accumulatedTime = TimeSpan.Zero;
        _isRecording = true;
        _isPaused = false;
    }

    public Task PauseAsync()
    {
        if (_isRecording && !_isPaused)
        {
#if ANDROID
            if (_mediaRecorder != null)
            {
#pragma warning disable CA1422
                _mediaRecorder.Pause();
#pragma warning restore CA1422
            }
#elif IOS || MACCATALYST
            _recorder?.Pause();
#else
            System.Diagnostics.Debug.WriteLine("Mock Recording paused");
#endif
            var startTime = _startTime ?? DateTime.UtcNow;
            _accumulatedTime += DateTime.UtcNow - startTime;
            _isPaused = true;
        }
        return Task.CompletedTask;
    }

    public Task ResumeAsync()
    {
        if (_isRecording && _isPaused)
        {
#if ANDROID
            if (_mediaRecorder != null)
            {
#pragma warning disable CA1422
                _mediaRecorder.Resume();
#pragma warning restore CA1422
            }
#elif IOS || MACCATALYST
            _recorder?.Record();
#else
            System.Diagnostics.Debug.WriteLine("Mock Recording resumed");
#endif
            _startTime = DateTime.UtcNow;
            _isPaused = false;
        }
        return Task.CompletedTask;
    }

    public Task<string> StopAsync()
    {
        if (_isRecording)
        {
#if ANDROID
            if (_mediaRecorder != null)
            {
                try
                {
#pragma warning disable CA1422
                    _mediaRecorder.Stop();
                    _mediaRecorder.Release();
#pragma warning restore CA1422
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error stopping MediaRecorder: {ex.Message}");
                }
                finally
                {
                    _mediaRecorder = null;
                }
            }
#elif IOS || MACCATALYST
            if (_recorder != null)
            {
                _recorder.Stop();
                _recorder.Dispose();
                _recorder = null;
            }
#else
            System.Diagnostics.Debug.WriteLine("Mock Recording stopped");
            if (!string.IsNullOrEmpty(_outputPath))
            {
                File.WriteAllText(_outputPath, "This is mock audio content representing meeting notes.");
            }
#endif
        }

        _isRecording = false;
        _isPaused = false;
        return Task.FromResult(_outputPath ?? string.Empty);
    }
}
