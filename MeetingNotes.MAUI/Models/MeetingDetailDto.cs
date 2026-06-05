using System;

namespace MeetingNotes.MAUI.Models;

public class MeetingDetailDto : MeetingDto
{
    public TranscriptDto? Transcript { get; set; }
    public SummaryDto? Summary { get; set; }
}
