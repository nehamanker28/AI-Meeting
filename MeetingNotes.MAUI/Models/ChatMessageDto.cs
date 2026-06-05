using System;

namespace MeetingNotes.MAUI.Models;

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public string Role { get; set; } = string.Empty; // user, assistant
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
