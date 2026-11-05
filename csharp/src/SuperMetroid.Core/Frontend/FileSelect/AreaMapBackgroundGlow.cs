namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// Where the area map's background glows (#1514): the area's background picture is added over the screen as a glow, over
/// the whole screen or only over the area's foreground.
/// </summary>
public enum AreaMapBackgroundGlow
{
    /// <summary>Over the whole screen, the empty space around the area's foreground included.</summary>
    WholeScreen,

    /// <summary>Only over the area's foreground, as while the room map's window opens over the area map.</summary>
    ForegroundOnly,
}
