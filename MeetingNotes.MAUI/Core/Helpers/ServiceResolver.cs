using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

namespace MeetingNotes.MAUI.Core.Helpers;

public static class ServiceResolver
{
    public static T GetRequiredService<T>() where T : notnull
    {
        var services = IPlatformApplication.Current?.Services;
        if (services == null)
        {
            throw new InvalidOperationException("Application service provider is not available.");
        }

        return services.GetRequiredService<T>();
    }
}
