using RunTracker.Models;
using Microsoft.AspNetCore.Components;

namespace RunTracker.Components.Pages;

public partial class Delete : ComponentBase
{
    [Inject]
    public required NavigationManager NavManager { get; set; }

    [Inject]
    public required IHttpClientFactory HttpClientFactory { get; set; }

    [Parameter]
    public int Id { get; set; }

    private Run _deleteRun { get; set; }

    public Delete()
    {
        _deleteRun = new Run() { Duration = new Duration(), Label="" };
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
                _deleteRun = runResponse;
            }
        }
        else
        {
            Console.WriteLine("Failed request run entry for id: {id}", Id);
        }
    }

    public void Cancel()
    {
        NavManager.NavigateTo("/");
    }

    public async Task Confirm()
    {
        var httpClient = HttpClientFactory.CreateClient("RunTracker");
        using HttpResponseMessage response = await httpClient.DeleteAsync("/run/" + Id.ToString());

        if (response.IsSuccessStatusCode)
        {
            NavManager.NavigateTo("/");
        }
        else
        {
            Console.WriteLine("Failed to delete run entry is {id}, status code: {status_code}", Id, response.StatusCode);
        }
    }
}