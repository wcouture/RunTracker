using System.Net.Http;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using RunTracker.Models;

namespace RunTracker.Services;

// <summary>
// The AuthenticationService is used to authenticate users and manage user accounts
// </summary>
public class AuthenticationService : IAuthenticationService
{
    // <summary>
    // The HttpClient to use to make requests to the server
    // </summary>
    private readonly HttpClient _httpClient;

    // <summary>
    // The ProtectedSessionStorage to use to store the authToken
    // </summary>
    private readonly ProtectedSessionStorage _protectedSessionStorage;

    // <summary>
    // Constructor for the AuthenticationService
    // </summary>
    // <param name="HttpClientFactory">The HttpClientFactory to use to create the HttpClient</param>
    // <param name="ProtectedSessionStorage">The ProtectedSessionStorage to use to store the authToken</param>
    public AuthenticationService(IHttpClientFactory HttpClientFactory, ProtectedSessionStorage ProtectedSessionStorage)
    {
        _httpClient = HttpClientFactory.CreateClient("RunTracker");
        _protectedSessionStorage = ProtectedSessionStorage;
    }

    // <summary>
    // Checks if the user is authenticated by checking if the authToken is set in the session storage
    // </summary>
    // <returns>A boolean indicating if the user is authenticated</returns>
    public async Task<bool> IsAuthenticated() {
        var authToken = await _protectedSessionStorage.GetAsync<string>("authToken");
        return authToken.Value is not null;
    }

    // <summary>
    // Gets the current user by getting the authToken from the session storage and using it to make a request to the server
    // </summary>
    // <returns>An AuthenticationResult containing the success status and the user account if successful</returns>
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

    // <summary>
    // Logs in a user by making a request to the server to get the user account
    // </summary>
    // <param name="email">The email of the user</param>
    // <param name="password">The password of the user</param>
    // <returns>An AuthenticationResult containing the success status and the user account if successful</returns>
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

    // <summary>
    // Registers a user by making a request to the server to create a new user account
    // </summary>
    // <param name="username">The username of the user</param>
    // <param name="email">The email of the user</param>
    // <param name="password">The password of the user</param>
    // <returns>An AuthenticationResult containing the success status</returns>
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

    // <summary>
    // Logs out a user by deleting the authToken from the session storage
    // </summary>
    // <returns>An AuthenticationResult containing the success status</returns>
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