using System.Threading.Tasks;
using MeetingNotes.MAUI.Services.Interfaces;

namespace MeetingNotes.MAUI.Services.Local;

public class MockSecureTokenService : ISecureTokenService
{
    private string? _accessToken;
    private string? _refreshToken;

    public Task<string?> GetAccessTokenAsync()
    {
        return Task.FromResult(_accessToken);
    }

    public Task<string?> GetRefreshTokenAsync()
    {
        return Task.FromResult(_refreshToken);
    }

    public Task SetTokensAsync(string accessToken, string refreshToken)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;
        return Task.CompletedTask;
    }

    public Task ClearAllTokensAsync()
    {
        _accessToken = null;
        _refreshToken = null;
        return Task.CompletedTask;
    }
}
