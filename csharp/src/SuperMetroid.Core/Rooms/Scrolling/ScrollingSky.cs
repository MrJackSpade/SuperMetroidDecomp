namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The two scrolling skies, each showing its own run of sky pages down its span (<see cref="ScrollingSkyChunks"/>). Its
/// values are arbitrary.
/// </summary>
public enum ScrollingSky
{
    /// <summary>The land sky, which the scrolling-sky room main uses.</summary>
    Land,
    /// <summary>The ocean sky, which the ocean scrolling-sky room main uses.</summary>
    Ocean,
}
