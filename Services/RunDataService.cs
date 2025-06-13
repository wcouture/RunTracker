using System.Text;
using System.Text.Json;
using RunTracker.Models;
using RunTracker.Services;

public class RunDataService : IRunDataService
{
    private readonly HttpClient _httpClient;

    public RunDataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<RunDataResult> CreateRun(Run run, string authToken)
    {
        var jsonData = new StringContent(JsonSerializer.Serialize(run), Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await _httpClient.PostAsync("/run/create", jsonData, CancellationToken.None);
        if (response.IsSuccessStatusCode)
        {
            return new RunDataResult {
                Success = true,
            };
        }
        else
        {
            return new RunDataResult {
                Success = false,
                ErrorMessage = "Failed to create run: " + response.StatusCode
            };
        }
    }

    public Task<RunDataResult> DeleteRun(int runId, string authToken)
    {
        throw new NotImplementedException();
    }

    public Task<RunDataResult> GetRunById(int runId)
    {
        throw new NotImplementedException();
    }

    public Task<RunDataResult> GetRunsByUserId(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<RunDataResult> UpdateRun(Run run, string authToken)
    {
        throw new NotImplementedException();
    }
}