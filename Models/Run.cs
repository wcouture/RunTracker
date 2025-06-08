namespace RunTracker.Models;

public class Run
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public double Mileage { get; set; }
    public Duration? Duration { get; set; }
    public DateTime Date { get; set; }
    public string? Label { get; set; }

    public static Duration Pace(Run run)
    {

        double minutesValue = Duration.MinuteValue(run.Duration ?? new Duration());
        minutesValue /= run.Mileage;

        int minutes = (int)Math.Floor(minutesValue);
        int seconds = (int)((minutesValue - minutes) * 60);
        
        return new Duration() { Hours = 0, Minutes = minutes, Seconds = seconds };
    }

    public override string ToString()
    {
        return Id + " | " + Mileage + " | " + Duration + "\n" + Label + " | " + Date;
    }
}