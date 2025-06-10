using RunTracker.Models;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using RunTracker.Components.Layout;

namespace RunTracker.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject]
    public required IHttpClientFactory HttpClientFactory { get; set; }

    [Inject]
    public required NavigationManager NavManager { get; set; }

    [CascadingParameter]
    public HttpContext? HttpContext { get; set; }
    public int UserId {get; set;}

    private IEnumerable<Run>? _runList;
    
    private Duration? _averagePace { get; set; }
    private Duration? _topPace { get; set; }
    private double _averageDistance { get; set; }
    private double _topDistance { get; set; }

    private async Task LoadRunData()
    {
        if (UserId == 0)
        {
            return;
        }

        try
        {
            var httpClient = HttpClientFactory.CreateClient("RunTracker");

            using HttpResponseMessage response = await httpClient.GetAsync($"/runs/{UserId}");

            if (response.IsSuccessStatusCode)
            {
                _runList = await response.Content.ReadFromJsonAsync<IEnumerable<Run>>();
                SetRunStats();
            }
            else
            {
                Console.WriteLine($"Failed to load run list. Status Code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LoadRunData: {ex.Message}");
        }
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            if (HttpContext?.User?.Identity?.IsAuthenticated ?? false)
            {   
                Claim? userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is not null)
                {
                    UserId = int.Parse(userIdClaim.Value);
                    await LoadRunData();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnInitializedAsync: {ex.Message}");
        }
    }

    private void SetRunStats()
    {
        double totalDistance = 0;
        Duration totalPace = new Duration();

        double maxDistance = 0;
        Duration minPace = new Duration() { Hours = 99 };

        foreach (Run run in _runList ?? [])
        {
            Duration pace = Run.Pace(run);

            totalDistance += run.Mileage;
            totalPace.Add(pace);

            if (run.Mileage > maxDistance)
            {
                maxDistance = run.Mileage;
            }

            bool isPaceQuicker = minPace.GreaterThan(pace);
            if (isPaceQuicker)
            {
                minPace.Hours = pace.Hours;
                minPace.Minutes = pace.Minutes;
                minPace.Seconds = pace.Seconds;
            }
        }

        _averageDistance = totalDistance / _runList?.Count() ?? 0;
        _topDistance = maxDistance;
        _topPace = minPace;

        double totalPaceValue = Duration.MinuteValue(totalPace);
        double avgPaceValue = totalPaceValue / (_runList?.Count() ?? 1);

        int hours = (int)Math.Floor(avgPaceValue / 60);
        int minutes = (int)Math.Floor(avgPaceValue - hours * 60.0);
        int seconds = (int)((avgPaceValue - Math.Floor(avgPaceValue)) * 60);

        _averagePace = new Duration() { Hours = hours, Minutes = minutes, Seconds = seconds };
    }

    public void AddRun()
    {
        NavManager.NavigateTo("/add");
    }
}