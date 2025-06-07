
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
}