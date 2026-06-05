using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using MeetingNotes.MAUI.Core.Constants;
using MeetingNotes.MAUI.Services.Interfaces;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;

namespace MeetingNotes.MAUI.Http;

public class AuthHttpHandler : DelegatingHandler
{
    private readonly ISecureTokenService _tokenService;
    private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);

    public AuthHttpHandler(ISecureTokenService tokenService)
    {
        _tokenService = tokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var token = await _tokenService.GetAccessTokenAsync();
        if (token != null)
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _refreshSemaphore.WaitAsync(ct);
            try
            {
                var currentToken = await _tokenService.GetAccessTokenAsync();
                
                if (currentToken != token)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", currentToken);
                    return await base.SendAsync(request, ct);
                }

                var newToken = await TryRefreshAsync(ct);
                if (newToken == null)
                {
                    await _tokenService.ClearAllTokensAsync();
                    CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new SessionExpiredMessage());
                    return response;
                }
                
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                response = await base.SendAsync(request, ct);
            }
            finally { _refreshSemaphore.Release(); }
        }

        return response;
    }

    private async Task<string?> TryRefreshAsync(CancellationToken ct)
    {
        var refreshToken = await _tokenService.GetRefreshTokenAsync();
        if (refreshToken == null) return null;

        try
        {
            using var client = new HttpClient();
            client.Timeout = System.TimeSpan.FromSeconds(AppConstants.ApiTimeoutSeconds);
            
            var requestUrl = $"{AppConstants.ApiBaseUrl}/api/v1/auth/refresh";
            
            var payload = new Dictionary<string, string>
            {
                { "refresh_token", refreshToken }
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(requestUrl, content, ct);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(ct);
                var tokens = JsonSerializer.Deserialize<TokenResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (tokens != null && !string.IsNullOrEmpty(tokens.AccessToken) && !string.IsNullOrEmpty(tokens.RefreshToken))
                {
                    await _tokenService.SetTokensAsync(tokens.AccessToken, tokens.RefreshToken);
                    return tokens.AccessToken;
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to refresh token: {ex.Message}");
        }

        return null;
    }

    private class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}

public class SessionExpiredMessage { }
