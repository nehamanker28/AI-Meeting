using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace MeetingNotes.MAUI.Core.Helpers;

public sealed class DiRouteFactory<TPage> : RouteFactory where TPage : Element
{
    public override Element GetOrCreate()
    {
        var services = IPlatformApplication.Current?.Services;
        if (services == null)
        {
            throw new InvalidOperationException("Application service provider is not available.");
        }

        return services.GetRequiredService<TPage>();
    }

    public override Element GetOrCreate(IServiceProvider services)
    {
        return services.GetRequiredService<TPage>();
    }
}