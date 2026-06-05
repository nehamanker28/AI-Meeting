using System;
using System.Collections.Generic;

namespace MeetingNotes.MAUI.Models;

public class SummaryDto
{
    public Guid MeetingId { get; set; }
    public string? ExecutiveSummary { get; set; }
    public string? DetailedSummary { get; set; }
    public List<string> Highlights { get; set; } = new();
    public List<ActionItemDto> ActionItems { get; set; } = new();
    public List<DecisionDto> Decisions { get; set; } = new();
}
