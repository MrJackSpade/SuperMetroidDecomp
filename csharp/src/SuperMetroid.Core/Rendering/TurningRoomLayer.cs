using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A gameplay room shown as a turning floor: the floor viewport below the lines the HUD covers, and an optional bottom band
/// of the background and sprites, with the pictures fixed in the room behind the floor (#1545). The HUD is its own layer (#1466).
/// </summary>
/// <remarks>Band boundaries are screen lines; the floor's turning is measured from the top of the screen, never from a band's start.</remarks>
public sealed partial record TurningRoomLayer : DisplayLayer
{
    /// <summary>How the floor appears on the screen, measured from the top of the screen even below the HUD.</summary>
    public FloorView Transform { get; }
    /// <summary>The turning floor the room shows (#1349); its pictures are the room tileset's.</summary>
    public AffineFloor Floor { get; }
    /// <summary>The screen lines at the top the floor leaves to the HUD, from zero through the screen's height; zero leaves none.</summary>
    public int HudLines { get; }
    /// <summary>The optional band of background and sprites at the bottom of the screen; null lets the floor and sprites continue to the bottom.</summary>
    public TurningRoomLowerBand? LowerBand { get; }
    /// <summary>The pictures fixed in the room behind the floor (#1545), such as the getaway chamber's walls; null for none.</summary>
    public TurningRoomScenery? Scenery { get; init; }

    /// <summary>Describes a room shown as a turning floor below the HUD's lines, with an optional bottom band.</summary>
    /// <param name="transform">How the floor appears on the screen.</param>
    /// <param name="floor">The turning floor.</param>
    /// <param name="hudLines">The lines the floor leaves to the HUD, from zero through the screen's height.</param>
    /// <param name="lowerBand">The optional bottom band, copied; its first line must follow the HUD and be above the screen's bottom.</param>
    /// <exception cref="ArgumentOutOfRangeException">The HUD's lines, or the band's first line, are invalid.</exception>
    public TurningRoomLayer(FloorView transform, AffineFloor floor,
        int hudLines, TurningRoomLowerBand? lowerBand = null)
    {
        HudLines = Ensure.BetweenInclusive(0, ScreenGeometry.ScreenHeightPixels, hudLines);
        if (lowerBand is { } band && (band.FirstLine < HudLines || band.FirstLine >= ScreenGeometry.ScreenHeightPixels))
            throw new ArgumentOutOfRangeException(nameof(lowerBand));
        Transform = transform;
        Floor = Ensure.NotNull(floor);
        LowerBand = lowerBand;
    }
}
