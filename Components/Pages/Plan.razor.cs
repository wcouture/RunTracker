using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using RunTracker.Models;

namespace RunTracker.Components.Pages;

public enum DayOfWeek
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday
}

public partial class Plan : ComponentBase
{
    [SupplyParameterFromForm]
    private double _goalMiles { get; set; }
    [SupplyParameterFromForm]
    private DateTime _goalDate { get; set; } = DateTime.Today;

    [SupplyParameterFromForm]
    private double _currentMiles { get; set; }
    [SupplyParameterFromForm]
    private Duration _currentDuration { get; set; } = new();

    [SupplyParameterFromForm]
    private int _weeklyRuns { get; set; }

    [SupplyParameterFromForm]
    private DayOfWeek _longRunDay { get; set; }

    public void Submit()
    {
        Console.WriteLine("Goal Miles: " + _goalMiles);
        Console.WriteLine("Goal Date: " + _goalDate);

        Console.WriteLine("Curr Miles: " + _currentMiles);
        Console.WriteLine("Curr Durration: " + _currentDuration);

        Console.WriteLine("Weekly Runs: " + _weeklyRuns);
        Console.WriteLine("Long Run Day: " + _longRunDay);
    }
}