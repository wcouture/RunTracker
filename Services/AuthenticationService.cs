using System.Net.Http;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using RunTracker.Models;

namespace RunTracker.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly ProtectedSessionStorage _protectedSessionStorage;

    public AuthenticationService(IHttpClientFactory HttpClientFactory, ProtectedSessionStorage ProtectedSessionStorage)
    {
        _httpClient = HttpClientFactory.CreateClient("RunTracker");
        _protectedSessionStorage = ProtectedSessionStorage;
    }

    public async Task<bool> IsAuthenticated() {
        var authToken = await _protectedSessionStorage.GetAsync<string>("authToken");
        return authToken.Value is not null;
    }

    public async Task<AuthenticationResult> GetCurrentUser() {
        var authToken = await _protectedSessionStorage.GetAsync<string>("authToken");
        if (authToken.Value is not null) {
            var response = await _httpClient.GetAsync("/user/find/" + authToken.Value);
            if (response.IsSuccessStatusCode) {
                UserAccount userAccount = await response.Content.ReadFromJsonAsync<UserAccount>() ?? new UserAccount();
                return new AuthenticationResult {
                    Success = true,
                    UserAccount = userAccount
                };
            }
            else {
                return new AuthenticationResult {
                    Success = false,
                    ErrorMessage = await response.Content.ReadAsStringAsync()
                };
            }
        }
        else {
            return new AuthenticationResult {
                Success = false,
                ErrorMessage = "No authentication token found"
            };
        }
    }

    public async Task<AuthenticationResult> Login(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) {
            return new AuthenticationResult {
                Success = false,
                ErrorMessage = "Email and password are required"
            };
        }

        UserAccount userAccount = new UserAccount {
            Email = email,
            Password = password
        };

        var response = await _httpClient.PostAsJsonAsync("/user/login", userAccount);
        if (response.IsSuccessStatusCode) {
            UserAccount? newAccount = await response.Content.ReadFromJsonAsync<UserAccount>();
            if (newAccount is null) {
                return new AuthenticationResult {
                    Success = false,
                    ErrorMessage = "Failed to parse response: " + await response.Content.ReadAsStringAsync()
                };
            }

            await _protectedSessionStorage.SetAsync("authToken", newAccount.Id.ToString());
            return new AuthenticationResult {
                Success = true,
                UserAccount = newAccount
            };
        }
        else {
            return new AuthenticationResult {
                Success = false,
                ErrorMessage = await response.Content.ReadAsStringAsync()
            };
        }
    }

    public async Task<AuthenticationResult> Register(string username, string email, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) {
            return new AuthenticationResult {
                Success = false,
                ErrorMessage = "Username, email, and password are required"
            };
        }

        UserAccount userAccount = new UserAccount {
            Username = username,
            Email = email,
            Password = password
        };

        var response = await _httpClient.PostAsJsonAsync("/user/register", userAccount);
        if (response.IsSuccessStatusCode) {
            return new AuthenticationResult {
                Success = true,
            };
        }
        else {
            return new AuthenticationResult {
                Success = false,
                ErrorMessage = await response.Content.ReadAsStringAsync()
            };
        }
    }

    public async Task<AuthenticationResult> Logout()
    {
        var authToken = await _protectedSessionStorage.GetAsync<string>("authToken");
        if (authToken.Value is not null) {
            await _protectedSessionStorage.DeleteAsync("authToken");
            return new AuthenticationResult {
                Success = true
            };
        }
        else {
            return new AuthenticationResult {
                Success = false,
                ErrorMessage = "No authentication token found"
            };
        }
    }
}