using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RunTracker.Models;
using Microsoft.AspNetCore.Components;

namespace RunTracker.Components.Pages;

public partial class Edit : ComponentBase
{
    [Inject]
    public required IHttpClientFactory HttpClientFactory { get; set; }

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [Parameter]
    public int Id { get; set; }
    
    private Run _edittingRun { get; set; }
    private DateTime _runDate { get; set; }

    public Edit()
    {
        _edittingRun = new Run() { Duration = new Duration(), Label = "" };
        _runDate = new DateTime();
    }

    protected override async Task OnInitializedAsync()
    {
        var httpClient = HttpClientFactory.CreateClient("RunTracker");

        using HttpResponseMessage response = await httpClient.GetAsync("/run/" + Id);
        if (response.IsSuccessStatusCode)
        {
            var runResponse = await response.Content.ReadFromJsonAsync<Run>();
            if (runResponse is null)
            {
                Console.WriteLine("Failed to load run object with id: {id}", Id);
            }
            else
            {
                _edittingRun = runResponse;
            }
        }
        else
        {
            Console.WriteLine("Failed request run entry for id: {id}", Id);
        }
    }

    public async Task Submit()
    {
        var httpClient = HttpClientFactory.CreateClient("RunTracker");

        var runData = new StringContent(JsonSerializer.Serialize(_edittingRun), Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await httpClient.PutAsync("/run/" + Id.ToString(), runData);

        if (response.IsSuccessStatusCode)
        {
            NavigationManager.NavigateTo("/");
        }
        else
        {
            Console.WriteLine("Failed to update run entry for id: {id}\n{run}", Id, _edittingRun);
        }
    }
}