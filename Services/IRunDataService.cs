using RunTracker.Models;

namespace RunTracker.Services;

public class RunDataResult {
    public bool Success { get; set; }
    public List<Run>? Runs { get; set; }
    public string? ErrorMessage { get; set; }
}

// <summary>
// The IRunDataService is used to get and create, modify and delete runs
// </summary>
public interface IRunDataService
{
    Task<RunDataResult> GetRunById(int runId);
    Task<RunDataResult> GetRunsByUserId(int userId);
    Task<RunDataResult> CreateRun(Run run, string authToken);
    Task<RunDataResult> UpdateRun(Run run, string authToken);
    Task<RunDataResult> DeleteRun(int runId, string authToken);
}