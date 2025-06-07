using RunTracker.Models;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;

namespace RunTracker.Components.Pages;

public partial class Home : ComponentBase
{
    [CascadingParameter]
    public required HttpContext HttpContext { get; set; }

    [Inject]
    public required IHttpClientFactory HttpClientFactory { get; set; }

    [Inject]
    public required NavigationManager NavManager { get; set; }

    private IEnumerable<Run>? _runList;
    
    private Duration? _averagePace { get; set; }
    private Duration? _topPace { get; set; }
    private double _averageDistance { get; set; }
    private double _topDistance { get; set; }

    private LineChart lineChart = default!;
    private LineChartOptions lineChartOptions = default!;
    private ChartData chartData = default!;

    private void InitializeChartData()
    {
        var labels = new List<string>();
        var datasets = new List<IChartDataset>();

        var dataset1 = new LineChartDataset()
        {
            Data = new List<double?>(),
            BackgroundColor = ColorUtility.CategoricalTwelveColors[4],
            BorderColor = ColorUtility.CategoricalTwelveColors[4],
            BorderWidth = 2,
            HoverBorderWidth = 4,
            Datalabels = new() { Alignment = Alignment.End, Anchor = Anchor.End }
        };

        foreach (Run run in _runList ?? [])
        {
            labels.Add(run.Label ?? "RUN");
            Duration pace = Run.Pace(run);
            double paceValue = Duration.MinuteValue(pace);
            paceValue = Math.Round(paceValue, 2);
            dataset1.Data.Add(paceValue);
        }

        datasets.Add(dataset1);

        chartData = new ChartData { Labels = labels, Datasets = datasets };

        lineChartOptions = new LineChartOptions();
        lineChartOptions.Responsive = true;
        lineChartOptions.Interaction = new Interaction { Mode = InteractionMode.Y };
        lineChartOptions.IndexAxis = "x";
        lineChartOptions.Layout.Padding = 0;
        

        lineChartOptions.Scales.X!.Title = new ChartAxesTitle { Text = "Run", Color="grey", Display = true };
        lineChartOptions.Scales.Y!.Title = new ChartAxesTitle { Text = "Pace (Minutes)", Color="grey", Display = true };
        lineChartOptions.Scales.X.Grid = new ChartAxesGrid() { Color = "rgba(255, 255, 255, 0.3)" };
        lineChartOptions.Scales.Y.Grid = new ChartAxesGrid() { Color = "rgba(255, 255, 255, 0.3)" };

        lineChartOptions.Plugins.Legend.Display = false;
        lineChartOptions.Plugins.Title = new ChartPluginsTitle() { Color="#DDD", Text="Recent Run Paces" };
        lineChartOptions.Plugins.Title.Display = true;
        lineChartOptions.Plugins.Datalabels.Color = "white";
    }

    private async Task LoadRunData()
    {
        var httpClient = HttpClientFactory.CreateClient("RunTracker");

        using HttpResponseMessage response = await httpClient.GetAsync("/runs");

        if (response.IsSuccessStatusCode)
        {
            _runList = await response.Content.ReadFromJsonAsync<IEnumerable<Run>>();
            InitializeChartData();
            SetRunStats();
        }
        else
        {
            Console.WriteLine($"Failed to load run list. Status Code: {response.StatusCode}");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (chartData is null)
        {
            await LoadRunData();
            InitializeChartData();
        }

        if (firstRender)
        {
            await lineChart.InitializeAsync(chartData ?? new ChartData(), lineChartOptions);
        }
        await base.OnAfterRenderAsync(firstRender);
    }


    protected override async Task OnInitializedAsync()
    {

        await LoadRunData();
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
}