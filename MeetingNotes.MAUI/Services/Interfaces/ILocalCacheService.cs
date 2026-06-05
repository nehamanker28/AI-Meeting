using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeetingNotes.MAUI.Models;

namespace MeetingNotes.MAUI.Services.Interfaces;

public interface ILocalCacheService
{
    Task SaveMeetingsAsync(List<MeetingDto> meetings);
    Task<List<MeetingDto>> GetMeetingsAsync(string? search = null);
    Task SaveMeetingDetailAsync(MeetingDetailDto detail);
    Task<MeetingDetailDto?> GetMeetingDetailAsync(Guid id);
    Task ClearCacheAsync();
}
