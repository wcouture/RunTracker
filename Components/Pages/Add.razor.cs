
using System.Text;
using System.Text.Json;
using RunTracker.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;


namespace RunTracker.Components.Pages;

public partial class Add : ComponentBase
{
    [SupplyParameterFromForm]
    private Run? _newRun { get; set; }

    [SupplyParameterFromForm]
    private DateTime _runDate { get; set; } = DateTime.Today;

    [Inject]
    public required IHttpClientFactory HttpClientFactory { get; set; }

    [Inject]
    public required NavigationManager NavManager { get; set; }

    protected override void OnInitialized()
    {
        _newRun ??= new();
        _newRun.Duration ??= new();
    }

    public async Task Submit()
    {
        if (_newRun is null)
            return;

        _newRun.Label = DateOnly.FromDateTime(_runDate).ToString();

        var httpClient = HttpClientFactory.CreateClient("RunTracker");
        var jsonData = new StringContent(JsonSerializer.Serialize(_newRun), Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await httpClient.PostAsync("/run", jsonData);

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