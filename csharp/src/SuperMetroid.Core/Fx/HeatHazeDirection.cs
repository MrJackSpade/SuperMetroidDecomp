namespace SuperMetroid.Core.Fx;

/// <summary>Which way lava's or acid's heat haze ripples the room's background (#1695).</summary>
/// <remarks>The values are arbitrary.</remarks>
public enum HeatHazeDirection
{
    /// <summary>The background stays still.</summary>
    None,

    /// <summary>Each line of the background sways sideways with the horizontal heat wave.</summary>
    Horizontal,

    /// <summary>The background's lines sway up and down with the vertical heat wave.</summary>
    Vertical,
}
