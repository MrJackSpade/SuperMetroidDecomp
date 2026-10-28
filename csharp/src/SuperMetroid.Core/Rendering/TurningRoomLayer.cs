using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>A gameplay room shown as a turning floor: the floor viewport, the opaque HUD, and an optional bottom band of the background and sprites.</summary>
/// <remarks>Band boundaries are screen lines; the floor's turning is measured from the top of the screen, never from a band's start.</remarks>
public sealed record TurningRoomLayer : DisplayLayer
{
    /// <summary>The floor's transform, measured in screen coordinates even below the HUD.</summary>
    public AffineTransform Transform { get; }
    /// <summary>The overlay whose top rows are the HUD, drawn unscrolled and opaque throughout (#1348).</summary>
    public Layer3Cells Hud { get; }
    /// <summary>The turning floor's map the room shows (#1349); its characters are the room tileset's.</summary>
    public AffineFloor Map { get; }
    /// <summary>The screen lines at the top the opaque HUD covers, 0..224 in multiples of eight; zero shows no HUD.</summary>
    public int HudLines { get; }
    /// <summary>The optional band of background and sprites at the bottom of the screen; null lets the floor and sprites continue to the bottom.</summary>
    public TurningRoomLowerBand? Floor { get; }

    /// <summary>Describes a room shown as a turning floor, with its HUD and an optional bottom band.</summary>
    /// <param name="transform">The floor's transform.</param>
    /// <param name="hud">The overlay whose top rows are the HUD.</param>
    /// <param name="map">The turning floor's map.</param>
    /// <param name="hudScanlines">The HUD's height, from zero through 224 screen lines and a multiple of eight.</param>
    /// <param name="floor">The optional bottom band, copied; its first line must follow the HUD and be above the screen's bottom, with each map dimension 32 or 64 tiles.</param>
    /// <exception cref="ArgumentOutOfRangeException">The HUD's height, or the band's first line or map size, is invalid.</exception>
    public TurningRoomLayer(AffineTransform transform, Layer3Cells hud, AffineFloor map,
        int hudScanlines, TurningRoomLowerBand? floor = null)
    {
        if (hudScanlines < 0 || hudScanlines > ScreenGeometry.ScreenHeightPixels ||
            hudScanlines % ScreenGeometry.BackgroundTileSizePixels != 0)
            throw new ArgumentOutOfRangeException(nameof(hudScanlines));
        if (floor is { } f && (f.FirstLine < hudScanlines || f.FirstLine >= ScreenGeometry.ScreenHeightPixels ||
            f.MapSize.X is not (32 or 64) || f.MapSize.Y is not (32 or 64)))
            throw new ArgumentOutOfRangeException(nameof(floor));
        Transform = transform;
        Hud = Ensure.NotNull(hud);
        Map = Ensure.NotNull(map);
        HudLines = hudScanlines;
        Floor = floor;
    }
}
