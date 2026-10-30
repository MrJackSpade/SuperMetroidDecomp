namespace SuperMetroid.Core.Rendering;

/// <summary>
/// How the background's scroll changes line by line on each axis, where a room effect moves it (#1403, #1468): water's and
/// the heat haze's waves as sampled waveforms, the scrolling sky as bands, Phantoon's wave as a sine wave and Crocomire's
/// melt as a stretch. Each axis is described independently; an axis without a shape keeps the background's own scroll on
/// every line. The backend's shader evaluates the shapes line by line; nothing keeps a table of lines.
/// </summary>
public readonly record struct BackgroundLineScrolls
{
    /// <summary>The background's horizontal scroll line by line, or null for its own scroll on every line.</summary>
    public LineScroll? Horizontal { get; init; }

    /// <summary>The background's vertical scroll line by line, or null for its own scroll on every line.</summary>
    public LineScroll? Vertical { get; init; }

    /// <summary>The number of gameplay lines below the HUD a room effect moves.</summary>
    public static int GameplayLines => ScreenGeometry.ScreenHeightPixels - ScreenGeometry.GameplayHudHeightPixels;
}
