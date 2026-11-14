namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How fast a Metaree dives (#1846): four pixels per update, plus one for every 24 pixels Samus stands below it. A Metaree
/// that springs at a Samus above it measures her distance downward past the room's last row and round from its top, so
/// it dives up to 2734 pixels per update and leaves the room's height in one update.
/// </summary>
/// <remarks>
/// Consulted once, as a Metaree within 72 pixels of Samus horizontally starts its dive, from its own and Samus's whole-pixel
/// heights; the dive then moves by this speed every update until it lands. Evidence: the original's dive divides the
/// sixteen-bit difference of the two heights, never checking which is lower, by 24 and adds four; the Metaree dive check
/// compares this speed with that arithmetic for a Samus below and above the Metaree.
/// </remarks>
[CompatibilityService("a Metaree springing at a Samus above it measures her distance round the room's height and dives enormously fast")]
public static class MetareeDiveSpeed
{
    /// <summary>The pixels of Samus's distance below that add one pixel to the dive's speed.</summary>
    private const int PixelsPerSpeedStep = 24;

    /// <summary>The speed of a dive at a Samus level with the Metaree, in pixels per update.</summary>
    private const int SlowestSpeed = 4;

    /// <summary>
    /// The dive's speed in whole pixels per update for a Metaree at <paramref name="metareeY"/> springing at Samus at
    /// <paramref name="samusY"/>, both whole-pixel room heights.
    /// </summary>
    public static int Of(int metareeY, int samusY) =>
        CoordinateAxis.Wrap(samusY - metareeY) / PixelsPerSpeedStep + SlowestSpeed;
}
