using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using RunTracker.Models;

namespace RunTracker.Components.Forms;
/// <summary>
/// The RunForm component is responsible for handling the creation and editing of run entries in the application.
/// It provides a form interface for users to input run details including distance, duration, and date/label.
/// </summary>
/// <remarks>
/// This component serves two primary purposes:
/// 1. Creating new run entries when RunId is not provided
/// 2. Editing existing run entries when RunId is specified
/// 
/// The component dynamically adjusts its behavior based on whether it's being used for creation or editing:
/// - For new entries, it uses POST requests to create new runs
/// - For existing entries, it uses PUT requests to update runs
/// 
/// The form includes fields for:
/// - Distance (mileage)
/// - Duration (hours, minutes, seconds)
/// - Date/Label
/// 
/// The component handles form submission, data validation, and navigation after successful operations.
/// </remarks>
public partial class RunForm
{
    [CascadingParameter]
    public required HttpContext HttpContext { get; set; }
    // Injects and dependencies
    [Inject]
    public required NavigationManager NavManager { get; set; }

    [Inject]
    public required IHttpClientFactory HttpClientFactory { get; set; }

    // Parameters
    [Parameter]
    public int RunId { get; set; }

    [SupplyParameterFromForm]
    private Run _run { get; set; } = new() { Duration = new(), Date = DateTime.Today };

    // Private fields
    private string _formHeader = "Add Run";
    private string _httpUrl = "/run/create";
    private HttpClient _httpClient = null!;
    private Func<string?, HttpContent?, CancellationToken, Task<HttpResponseMessage>> _httpFunction = null!;

    /// <summary>
    /// Initializes the component and sets up the HTTP client and request function based on whether the form is being used for creation or editing.
    /// </summary>
    /// <remarks>
    /// This method is called when the component is initialized. It creates the HTTP client and sets up the request function
    /// based on whether the form is being used for creation or editing.
    /// </remarks>
    protected override async Task OnInitializedAsync()
    {   
        _httpClient = HttpClientFactory.CreateClient("RunTracker");
        
        if (RunId != 0)
        {
            await InitializeData();
            _httpUrl = $"/run/update/{RunId}";
            _httpFunction = _httpClient.PutAsync;
            _formHeader = "Edit Run";
        }
        else
        {
            _run!.UserId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _httpFunction = _httpClient.PostAsync;
        }
    }

    /// <summary>
    /// Initializes the form data by fetching the run entry with the specified ID from the server.
    /// If successful, populates the form with the run data and sets the run date from the label.
    /// </summary>
    /// <remarks>
    /// This method is called when editing an existing run entry. It makes a GET request to fetch the run data
    /// and updates the form fields accordingly. If the request fails, an error message is logged to the console.
    /// </remarks>
    public async Task InitializeData() {
        try {
            string url = "/run/find/" + RunId;
            using HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var runResponse = await response.Content.ReadFromJsonAsync<Run>();
                if (runResponse is not null)
                {
                    _run.Id = runResponse.Id;
                    _run.UserId = runResponse.UserId;
                    _run.Mileage = runResponse.Mileage;
                    _run.Duration = runResponse.Duration;
                    _run.Label = runResponse.Label;
                    _run.Date = runResponse.Date;
                    Console.WriteLine($"Editting Run object: {_run}");
                }
                else
                {
                    Console.WriteLine($"Failed to load run object with id: {RunId}");
                }
            }
            else
            {
                Console.WriteLine($"Failed request run entry for id: {RunId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in InitializeData: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the form submission by sending the run data to the server.
    /// </summary>
    /// <remarks>
    /// This method is called when the form is submitted. It:
    /// - Updates the run label with the formatted date
    /// - Serializes the run data to JSON
    /// - Sends a POST or PUT request to the server depending on whether this is a new or existing run
    /// - Navigates to the home page on success
    /// - Logs an error message to the console on failure
    /// </remarks>
    public async Task HandleSubmit()
    {
        
        Console.WriteLine($"Date value: {_run.Date}");
        Console.WriteLine($"Label value: {_run.Label}");
        Console.WriteLine($"Mileage value: {_run.Mileage}");
        Console.WriteLine($"Duration value: {_run.Duration}");

        var jsonData = new StringContent(JsonSerializer.Serialize(_run), Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await _httpFunction(_httpUrl, jsonData, CancellationToken.None);
        if (response.IsSuccessStatusCode)
        {
            NavManager.NavigateTo("/");
        }
        else
        {
            Console.WriteLine($"Failed to add run. Status code: {response.StatusCode}");
        }
    }
}