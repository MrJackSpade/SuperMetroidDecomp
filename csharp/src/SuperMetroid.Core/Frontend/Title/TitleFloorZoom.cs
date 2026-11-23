namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// The title floor's zoom (#2553): no larger than the floor's own size, where the title's final zoom stops. A scene's
/// starting zoom is checked once, as the title's catalog builds, and the final zoom grows by the finest step a zoom takes
/// only while the floor is larger than its own size, so it lands on the floor's own size and no title zoom shows the floor
/// smaller.
/// </summary>
public readonly partial record struct TitleFloorZoom
{
    /// <summary>The title zoom <paramref name="zoom"/>, no larger than the floor's own size.</summary>
    private TitleFloorZoom(FloorZoom zoom) => FloorZoom = zoom;

    /// <summary>The zoom, no larger than the floor's own size.</summary>
    public FloorZoom FloorZoom { get; }

    /// <summary>The zoom that shows the floor at its own size, where the final zoom stops.</summary>
    public static TitleFloorZoom OwnSize { get; } = new(FloorZoom.OwnSize);

    /// <summary>Whether the zoom shows the floor at its own size.</summary>
    public bool IsOwnSize => FloorZoom == FloorZoom.OwnSize;

    /// <summary>A scene's starting zoom, checked once as the title's catalog builds.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The zoom shows the floor smaller than its own size.</exception>
    public static TitleFloorZoom Starting(FloorZoom zoom) =>
        zoom <= FloorZoom.OwnSize
            ? new(zoom)
            : throw new ArgumentOutOfRangeException(nameof(zoom), zoom, "A title scene starts no smaller than the floor's own size.");

    /// <summary>
    /// The zoom one final-zoom step on: widened by the finest step a zoom takes while the floor is larger than its own size,
    /// and none at its own size, where the final zoom completes.
    /// </summary>
    public TitleFloorZoom? Grown() => IsOwnSize ? null : new(FloorZoom.Widened(FloorZoomStep.Finest));
}
