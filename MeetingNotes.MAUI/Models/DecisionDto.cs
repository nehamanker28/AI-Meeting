using System;

namespace MeetingNotes.MAUI.Models;

public class DecisionDto
{
    public Guid Id { get; set; }
    public string DecisionText { get; set; } = string.Empty;
    public string? MadeBy { get; set; }
}
