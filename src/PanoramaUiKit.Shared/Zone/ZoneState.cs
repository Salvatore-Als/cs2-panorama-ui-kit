using System.Text.Json.Serialization;

namespace PanoramaUiKit.Shared.Zone;

/// <summary>Where the zone is in its phase: <c>state-waiting</c>, <c>state-closing</c> or <c>state-final</c> on <c>zn_status</c>.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ZoneState>))]
public enum ZoneState
{
    /// <summary>The zone holds; the countdown is the time before it starts closing.</summary>
    Waiting,

    /// <summary>The zone is closing; the countdown is the time until it stops.</summary>
    Closing,

    /// <summary>The last zone.</summary>
    Final,
}

public static class ZoneStateExtensions
{
    /// <summary>Class on <c>zn_status</c>: <c>state-waiting</c>, <c>state-closing</c>, <c>state-final</c>.</summary>
    public static string Class(this ZoneState state) => state switch
    {
        ZoneState.Closing => "state-closing",
        ZoneState.Final => "state-final",
        _ => "state-waiting",
    };

    /// <summary>Default text of <c>{s:zn_state}</c>.</summary>
    public static string DefaultText(this ZoneState state) => state switch
    {
        ZoneState.Closing => "Closing",
        ZoneState.Final => "Final zone",
        _ => "Closing in",
    };
}
