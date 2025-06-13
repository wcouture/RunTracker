using RunTracker.Models;

namespace RunTracker.Services;

public class AuthenticationResult {
    public bool Success { get; set; }
    public UserAccount? UserAccount { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IAuthenticationService
{
    Task<bool> IsAuthenticated();
    Task<AuthenticationResult> GetCurrentUser();
    Task<AuthenticationResult> Login(string email, string password);
    Task<AuthenticationResult> Register(string username, string email, string password);
    Task<AuthenticationResult> Logout();
}