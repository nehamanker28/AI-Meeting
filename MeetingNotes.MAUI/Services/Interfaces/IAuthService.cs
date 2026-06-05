using System.Threading.Tasks;
using MeetingNotes.MAUI.Models;

namespace MeetingNotes.MAUI.Services.Interfaces;

public interface IAuthService
{
    Task<bool> LoginAsync(string email, string password);
    Task<bool> RegisterAsync(string fullName, string email, string password);
    Task<bool> ForgotPasswordAsync(string email);
    Task<UserDto?> GetProfileAsync();
    Task<bool> UpdateProfileAsync(string fullName, string? avatarUrl);
    Task LogoutAsync();
}
