using System;
using System.Threading.Tasks;
using MeetingNotes.MAUI.Models;
using MeetingNotes.MAUI.Services.Interfaces;

namespace MeetingNotes.MAUI.Services.Local;

public class MockAuthService : IAuthService
{
    private readonly ISecureTokenService _tokenService;

    public MockAuthService(ISecureTokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public Task<bool> LoginAsync(string email, string password)
    {
        // Accept any credentials for local development
        // store fake tokens
        return Task.Run(async () =>
        {
            await _tokenService.SetTokensAsync("local-access-token", "local-refresh-token");
            return true;
        });
    }

    public Task<bool> RegisterAsync(string fullName, string email, string password)
    {
        return Task.FromResult(true);
    }

    public Task<bool> ForgotPasswordAsync(string email)
    {
        return Task.FromResult(true);
    }

    public Task<UserDto?> GetProfileAsync()
    {
        var user = new UserDto
        {
            Id = Guid.NewGuid(),
            FullName = "Local User",
            Email = "local@local.dev",
            AvatarUrl = null,
            CreatedAt = DateTime.UtcNow
        };
        return Task.FromResult<UserDto?>(user);
    }

    public Task<bool> UpdateProfileAsync(string fullName, string? avatarUrl)
    {
        return Task.FromResult(true);
    }

    public Task LogoutAsync()
    {
        return _tokenService.ClearAllTokensAsync();
    }
}
