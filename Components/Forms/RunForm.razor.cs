using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using RunTracker.Models;

namespace RunTracker.Components.Forms;

public partial class RunForm
{
    [Parameter]
    public int RunId { get; set; }

    [Inject]
    public required NavigationManager NavManager { get; set; }

    [Inject]
    public required IHttpClientFactory HttpClientFactory { get; set; }

    [SupplyParameterFromForm]
    private Run? _run { get; set; } = new() { Duration = new() };
    [SupplyParameterFromForm]
    private DateTime _runDate { get; set; } = DateTime.Today;

    private string _formHeader = "Add Run";
    private string _postUrl = "/run";
    private HttpClient _httpClient = null;
    private Func<string?, HttpContent?, CancellationToken, Task<HttpResponseMessage>> _httpFunction = null;

    protected override async Task OnInitializedAsync()
    {   
        _httpClient = HttpClientFactory.CreateClient("RunTracker");
        if (RunId != 0)
        {
            await InitializeData();
            _postUrl = $"/run/{RunId}";
            _httpFunction = _httpClient.PutAsync;
            _formHeader = "Edit Run";
        }
        else
        {
            _httpFunction = _httpClient.PostAsync;
        }
    }

    public async Task InitializeData() {

        using HttpResponseMessage response = await _httpClient.GetAsync("/run/" + RunId);
        if (response.IsSuccessStatusCode)
        {
            var runResponse = await response.Content.ReadFromJsonAsync<Run>();
            if (runResponse is null)
            {
                Console.WriteLine("Failed to load run object with id: {id}", RunId);
            }
            else
            {
                _run = runResponse;
                Console.WriteLine(_run);
                _runDate = DateTime.Parse(_run.Label ?? "");
            }
        }
        else
        {
            Console.WriteLine("Failed request run entry for id: {id}", RunId);
        }
    }

    public async Task HandleSubmit()
    {
        if (_run is null)
            return;

        _run.Label = DateOnly.FromDateTime(_runDate).ToString();

        var jsonData = new StringContent(JsonSerializer.Serialize(_run), Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await _httpFunction(_postUrl, jsonData, CancellationToken.None);
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