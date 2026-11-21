namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// Provides one map arrow's screen anchor, as installed (#1596). The arrow is a still picture (#2359): it shows the same
/// picture for as long as the map can scroll its way.
/// </summary>
public sealed partial class MapArrowVisual
{
    /// <summary>Keeps the installed anchor.</summary>
    /// <param name="position">The arrow's screen-pixel anchor.</param>
    internal MapArrowVisual(Offset16<ScreenPixels> position) => Position = position;

    /// <summary>Screen-pixel anchor used when composing this direction's sprite.</summary>
    public Offset16<ScreenPixels> Position { get; }
}
