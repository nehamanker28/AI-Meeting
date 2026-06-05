using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeetingNotes.MAUI.Models;

namespace MeetingNotes.MAUI.Services.Interfaces;

public interface IChatService
{
    Task<List<ChatMessageDto>> GetHistoryAsync(Guid meetingId);
    Task<ChatMessageDto> SendMessageAsync(Guid meetingId, string question, List<ChatMessageDto> history);
}
