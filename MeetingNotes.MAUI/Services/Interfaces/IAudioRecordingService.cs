using System;
using System.Threading.Tasks;

namespace MeetingNotes.MAUI.Services.Interfaces;

public interface IAudioRecordingService
{
    Task StartAsync(string outputPath);
    Task PauseAsync();
    Task ResumeAsync();
    Task<string> StopAsync();
    TimeSpan Elapsed { get; }
}
