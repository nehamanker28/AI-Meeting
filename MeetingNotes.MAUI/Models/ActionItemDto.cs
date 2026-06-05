using System;

namespace MeetingNotes.MAUI.Models;

public class ActionItemDto
{
    public Guid Id { get; set; }
    public string Task { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string Priority { get; set; } = "medium"; // low, medium, high
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
}
