using System;

namespace MeetingNotes.MAUI.Models;

public class TranscriptDto
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public string RawText { get; set; } = string.Empty;
    public string? Language { get; set; }
    public int WordCount { get; set; }
}
