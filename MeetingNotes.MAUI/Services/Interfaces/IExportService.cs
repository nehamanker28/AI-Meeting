using System;
using System.Threading.Tasks;

namespace MeetingNotes.MAUI.Services.Interfaces;

public interface IExportService
{
    Task ExportAndShareAsync(Guid meetingId, string title, string format);
}
