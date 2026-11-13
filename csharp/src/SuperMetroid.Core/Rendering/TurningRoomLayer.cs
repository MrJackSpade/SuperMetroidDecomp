using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A gameplay room shown as a turning floor: the floor viewport below the lines the HUD covers, and an optional bottom band
/// of the background and sprites, with the pictures fixed in the room behind the floor (#1545). The HUD is its own layer (#1466).
/// </summary>
/// <remarks>Band boundaries are screen lines; the floor's turning is measured from the top of the screen, never from a band's start.</remarks>
public sealed record TurningRoomLayer : DisplayLayer
{
    /// <summary>How the floor appears on the screen, measured from the top of the screen even below the HUD.</summary>
    public FloorView Transform { get; }
    /// <summary>The turning floor's map the room shows (#1349); its characters are the room tileset's.</summary>
    public AffineFloor Map { get; }
    /// <summary>The screen lines at the top the floor leaves to the HUD, 0..224 in multiples of eight; zero leaves none.</summary>
    public int HudLines { get; }
    /// <summary>The optional band of background and sprites at the bottom of the screen; null lets the floor and sprites continue to the bottom.</summary>
    public TurningRoomLowerBand? Floor { get; }
    /// <summary>The pictures fixed in the room behind the floor (#1545), such as the getaway chamber's walls; null for none.</summary>
    public TurningRoomScenery? Scenery { get; init; }

    /// <summary>Describes a room shown as a turning floor below the HUD's lines, with an optional bottom band.</summary>
    /// <param name="transform">How the floor appears on the screen.</param>
    /// <param name="map">The turning floor's map.</param>
    /// <param name="hudLines">The lines the floor leaves to the HUD, from zero through 224 screen lines and a multiple of eight.</param>
    /// <param name="floor">The optional bottom band, copied; its first line must follow the HUD and be above the screen's bottom, with each map dimension 32 or 64 tiles.</param>
    /// <exception cref="ArgumentOutOfRangeException">The HUD's lines, or the band's first line or map size, are invalid.</exception>
    public TurningRoomLayer(FloorView transform, AffineFloor map,
        int hudLines, TurningRoomLowerBand? floor = null)
    {
        if (hudLines < 0 || hudLines > ScreenGeometry.ScreenHeightPixels ||
            hudLines % ScreenGeometry.BackgroundTileSizePixels != 0)
            throw new ArgumentOutOfRangeException(nameof(hudLines));
        if (floor is { } f && (f.FirstLine < hudLines || f.FirstLine >= ScreenGeometry.ScreenHeightPixels))
            throw new ArgumentOutOfRangeException(nameof(floor));
        Transform = transform;
        Map = Ensure.NotNull(map);
        HudLines = hudLines;
        Floor = floor;
    }
}
