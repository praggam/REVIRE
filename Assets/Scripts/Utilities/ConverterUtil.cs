using UnityEngine;

// Converters which can be used to display values in UI in more descriptive manner.
public static class ConverterUtil
{
    // converts time to minutes and seconds format like so: "11:59"
    public static string MinutesSecondsToString(int time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        return string.Format("{0:0}:{1:00}", minutes, seconds);
    }

    public static string MinutesSecondsToStringDescriptive(int time)
    {
        if (time >=  60f)
        {
            return System.TimeSpan.FromSeconds((double)time).ToString("m' min 's' sec'");
        }
        else
        {
            return System.TimeSpan.FromSeconds((double)time).ToString("s' sec'");
        }
    }

    // converts time to hours and minutes format skipping seconds like so: "12 h 11 min"
    public static string HoursMinutesToString(float time)
    {
        if (time > 60 * 60f)
        {
            return System.TimeSpan.FromSeconds((double)time).ToString("h' h 'm' min'");
        }
        else
        {
            return System.TimeSpan.FromSeconds((double)time).ToString("m' min'");
        }

    }

    public static string PointsToString(int points)
    {
        if (points == 1)
        {
            return string.Format("{0} point", points);
        }
        else
        {
            return string.Format("{0} points", points);
        }
    }
}
