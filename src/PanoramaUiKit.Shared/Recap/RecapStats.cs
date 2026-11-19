using System.Globalization;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Recap;

/// <summary>
/// The common tiles, with their icon and an English label. Numbers are written with the invariant culture.
/// For another label or language, build a <see cref="RecapStat"/> directly.
/// </summary>
public static class RecapStats
{
    public static RecapStat Kills(int kills, bool best = false) =>
        new(RecapIcons.Kills, Number(kills), "Kills", UiStyle.Danger, best);

    public static RecapStat Knockouts(int knockouts, bool best = false) =>
        new(RecapIcons.Knockouts, Number(knockouts), "Knockouts", UiStyle.Warn, best);

    public static RecapStat Assists(int assists, bool best = false) =>
        new(RecapIcons.Assists, Number(assists), "Assists", UiStyle.Info, best);

    public static RecapStat Points(int points, bool best = false) =>
        new(RecapIcons.Points, Number(points), "Points", UiStyle.Gold, best);

    public static RecapStat Shots(int shots, bool best = false) =>
        new(RecapIcons.Shots, Number(shots), "Shots fired", UiStyle.Neutral, best);

    public static RecapStat Headshots(int headshots, bool best = false) =>
        new(RecapIcons.Headshots, Number(headshots), "Headshots", UiStyle.Danger, best);

    /// <summary><paramref name="accuracy"/> from 0 to 1, written as a percentage.</summary>
    public static RecapStat Accuracy(float accuracy, bool best = false) =>
        new(RecapIcons.Accuracy, $"{(int)Math.Round(Math.Clamp(accuracy, 0f, 1f) * 100f)}%", "Accuracy", UiStyle.Info, best);

    public static RecapStat DamageDealt(int damage, bool best = false) =>
        new(RecapIcons.DamageDealt, Number(damage), "Damage dealt", UiStyle.Success, best);

    public static RecapStat DamageTaken(int damage, bool best = false) =>
        new(RecapIcons.DamageTaken, Number(damage), "Damage taken", UiStyle.Purple, best);

    public static RecapStat Revives(int revives, bool best = false) =>
        new(RecapIcons.Revives, Number(revives), "Revives", UiStyle.Success, best);

    /// <summary>Written <c>m:ss</c>, or <c>h:mm:ss</c> past an hour.</summary>
    public static RecapStat Survival(TimeSpan time, bool best = false) =>
        new(RecapIcons.Survival, Duration(time), "Survived", UiStyle.Neutral, best);

    /// <summary><paramref name="meters"/> written in metres, or kilometres past 1000.</summary>
    public static RecapStat Distance(float meters, bool best = false) =>
        new(RecapIcons.Distance, Meters(meters), "Distance", UiStyle.Neutral, best);

    private static string Number(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static string Duration(TimeSpan time)
    {
        if (time < TimeSpan.Zero)
        {
            time = TimeSpan.Zero;
        }

        if (time.TotalHours >= 1)
        {
            return $"{(int)time.TotalHours}:{time.Minutes:00}:{time.Seconds:00}";
        }

        return $"{time.Minutes}:{time.Seconds:00}";
    }

    private static string Meters(float meters)
    {
        if (meters >= 1000f)
        {
            return string.Create(CultureInfo.InvariantCulture, $"{meters / 1000f:0.0} km");
        }

        return string.Create(CultureInfo.InvariantCulture, $"{Math.Max(0f, meters):0} m");
    }
}
