using System.Runtime.Serialization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Net.Http.Headers;

namespace RunTracker.Models;

public class Duration
{
    public int Hours { get; set; }
    public int Minutes { get; set; }
    public int Seconds { get; set; }

    public override string ToString()
    {
        string hoursLabel = Hours.ToString();
        if (Hours < 10)
        {
            hoursLabel = "0" + hoursLabel;
        }

        string minutesLabel = Minutes.ToString();
        if (Minutes < 10)
        {
            minutesLabel = "0" + minutesLabel;
        }

        string secondsLabel = Seconds.ToString();
        if (Seconds < 10)
        {
            secondsLabel = "0" + secondsLabel;
        }
        return hoursLabel + ":" + minutesLabel + ":" + secondsLabel;
    }

    public Duration()
    {
        Hours = 0;
        Minutes = 0;
        Seconds = 0;
    }

    private void AggregateValues()
    {
        while (Seconds > 60)
        {
            Seconds -= 60;
            Minutes += 1;
        }

        while (Minutes > 60)
        {
            Minutes -= 60;
            Hours += 1;
        }
    }

    public void Add(Duration duration)
    {
        Seconds += duration.Seconds;
        Minutes += duration.Minutes;
        Hours += duration.Hours;

        AggregateValues();
    }

    public bool GreaterThan(Duration duration)
    {
        double thisPace = MinuteValue(this);
        double otherPace = MinuteValue(duration);

        return thisPace > otherPace;
    }

    public static double MinuteValue(Duration duration)
    {
        double minutes = (duration.Hours * 60.0) + duration.Minutes;
        double fraction = (double)duration.Seconds / 60;
        double total = minutes + fraction;
        return total;
    }
}