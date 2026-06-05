using System;
using System.IO;
using System.Threading.Tasks;
using MeetingNotes.MAUI.Services.Interfaces;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;

namespace MeetingNotes.MAUI.Services.Export;

public class ExportService : IExportService
{
    private readonly IMeetingService _meetingService;

    public ExportService(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task ExportAndShareAsync(Guid meetingId, string title, string format)
    {
        var data = await _meetingService.ExportMeetingAsync(meetingId, format);
        
        var ext = format.ToLower() switch
        {
            "pdf" => "pdf",
            "markdown" or "md" => "md",
            _ => "txt"
        };

        var safeTitle = string.Join("_", title.Split(Path.GetInvalidFileNameChars()));
        var fileName = $"{safeTitle}.{ext}";
        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

        await File.WriteAllBytesAsync(filePath, data);

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = $"Export Meeting Notes: {title}",
            File = new ShareFile(filePath)
        });
    }
}
