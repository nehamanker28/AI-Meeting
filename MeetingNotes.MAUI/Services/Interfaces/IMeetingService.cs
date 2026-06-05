using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MeetingNotes.MAUI.Models;

namespace MeetingNotes.MAUI.Services.Interfaces;

public interface IMeetingService
{
    Task<List<MeetingDto>> GetMeetingsAsync(string? search = null, int page = 1);
    Task<MeetingDetailDto> GetMeetingAsync(Guid id);
    Task<Guid> CreateMeetingAsync(string title, DateTime meetingDate, string? description);
    Task UploadAudioAsync(Guid meetingId, Stream stream, string fileName, Action<double>? onProgress = null);
    Task<MeetingStatusDto> GetStatusAsync(Guid meetingId);
    Task<SummaryDto> GetSummaryAsync(Guid meetingId);
    Task<TranscriptDto> GetTranscriptAsync(Guid meetingId);
    Task DeleteMeetingAsync(Guid meetingId);
    Task<byte[]> ExportMeetingAsync(Guid meetingId, string format);
}
