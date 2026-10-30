using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A gameplay room shown as a turning floor: the floor viewport below the lines the HUD covers, and an optional bottom band
/// of the background and sprites. The HUD is its own layer (#1466).
/// </summary>
/// <remarks>Band boundaries are screen lines; the floor's turning is measured from the top of the screen, never from a band's start.</remarks>
public sealed record TurningRoomLayer : DisplayLayer
{
    /// <summary>The floor's transform, measured in screen coordinates even below the HUD.</summary>
    public AffineTransform Transform { get; }
    /// <summary>The turning floor's map the room shows (#1349); its characters are the room tileset's.</summary>
    public AffineFloor Map { get; }
    /// <summary>The screen lines at the top the floor leaves to the HUD, 0..224 in multiples of eight; zero leaves none.</summary>
    public int HudLines { get; }
    /// <summary>The optional band of background and sprites at the bottom of the screen; null lets the floor and sprites continue to the bottom.</summary>
    public TurningRoomLowerBand? Floor { get; }

    /// <summary>Describes a room shown as a turning floor below the HUD's lines, with an optional bottom band.</summary>
    /// <param name="transform">The floor's transform.</param>
    /// <param name="map">The turning floor's map.</param>
    /// <param name="hudScanlines">The lines the floor leaves to the HUD, from zero through 224 screen lines and a multiple of eight.</param>
    /// <param name="floor">The optional bottom band, copied; its first line must follow the HUD and be above the screen's bottom, with each map dimension 32 or 64 tiles.</param>
    /// <exception cref="ArgumentOutOfRangeException">The HUD's lines, or the band's first line or map size, are invalid.</exception>
    public TurningRoomLayer(AffineTransform transform, AffineFloor map,
        int hudScanlines, TurningRoomLowerBand? floor = null)
    {
        if (hudScanlines < 0 || hudScanlines > ScreenGeometry.ScreenHeightPixels ||
            hudScanlines % ScreenGeometry.BackgroundTileSizePixels != 0)
            throw new ArgumentOutOfRangeException(nameof(hudScanlines));
        if (floor is { } f && (f.FirstLine < hudScanlines || f.FirstLine >= ScreenGeometry.ScreenHeightPixels ||
            f.MapSize.X is not (32 or 64) || f.MapSize.Y is not (32 or 64)))
            throw new ArgumentOutOfRangeException(nameof(floor));
        Transform = transform;
        Map = Ensure.NotNull(map);
        HudLines = hudScanlines;
        Floor = floor;
    }
}
