using System.Threading.Tasks;

namespace MeetingNotes.MAUI.Services.Interfaces;

public interface ISecureTokenService
{
    Task<string?> GetAccessTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    Task SetTokensAsync(string accessToken, string refreshToken);
    Task ClearAllTokensAsync();
}
