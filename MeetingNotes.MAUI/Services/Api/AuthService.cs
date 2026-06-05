using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MeetingNotes.MAUI.Core.Constants;
using MeetingNotes.MAUI.Models;
using MeetingNotes.MAUI.Services.Interfaces;

namespace MeetingNotes.MAUI.Services.Api;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ISecureTokenService _tokenService;

    public AuthService(HttpClient httpClient, ISecureTokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _httpClient.BaseAddress = new Uri(AppConstants.ApiBaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(AppConstants.ApiTimeoutSeconds);
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var payload = new { email, password };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/v1/auth/login", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokens = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tokens != null && !string.IsNullOrEmpty(tokens.AccessToken) && !string.IsNullOrEmpty(tokens.RefreshToken))
                {
                    await _tokenService.SetTokensAsync(tokens.AccessToken, tokens.RefreshToken);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
        }

        return false;
    }

    public async Task<bool> RegisterAsync(string fullName, string email, string password)
    {
        try
        {
            var payload = new { full_name = fullName, email, password };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/v1/auth/register", content);
            
            if (response.IsSuccessStatusCode)
            {
                return await LoginAsync(email, password);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Register error: {ex.Message}");
        }

        return false;
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        try
        {
            var payload = new { email };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/v1/auth/forgot-password", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ForgotPassword error: {ex.Message}");
            return false;
        }
    }

    public async Task<UserDto?> GetProfileAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/profile");
            var token = await _tokenService.GetAccessTokenAsync();
            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<UserDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetProfile error: {ex.Message}");
        }

        return null;
    }

    public async Task<bool> UpdateProfileAsync(string fullName, string? avatarUrl)
    {
        try
        {
            var payload = new { full_name = fullName, avatar_url = avatarUrl };
            var request = new HttpRequestMessage(HttpMethod.Put, "/api/v1/auth/profile")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            
            var token = await _tokenService.GetAccessTokenAsync();
            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateProfile error: {ex.Message}");
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            var token = await _tokenService.GetRefreshTokenAsync();
            if (token != null)
            {
                var payload = new Dictionary<string, string> { { "refresh_token", token } };
                var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout")
                {
                    Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                };
                
                var accessToken = await _tokenService.GetAccessTokenAsync();
                if (accessToken != null)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }

                await _httpClient.SendAsync(request);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
        }
        finally
        {
            await _tokenService.ClearAllTokensAsync();
        }
    }

    private class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
