using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>Resolved Mode-7 viewport, opaque HUD, and optional bottom Mode-1 BG2/OBJ band.</summary>
/// <remarks>Scanline boundaries are physical screen coordinates; the transform never restarts at the band origin.</remarks>
public sealed record Mode7GameplayRenderLayer : RenderLayer
{
    /// <summary>Gets the floor's transform; sampling uses physical screen coordinates even below the HUD band.</summary>
    public AffineTransform Transform { get; }
    /// <summary>Gets the BG3 plane whose top rows are the HUD, drawn unscrolled with opaque color-zero pixels (#1348).</summary>
    public Layer3Cells Hud { get; }
    /// <summary>Gets the Mode 7 map the room shows (#1349); its characters are the room tileset's.</summary>
    public AffineFloor Map { get; }
    /// <summary>Gets the number of top-of-screen scanlines occupied by the opaque HUD, 0..224 in multiples of eight; zero disables the HUD band.</summary>
    public int HudScanlines { get; }
    /// <summary>Gets the optional bottom Mode-1 BG2/OBJ band; null lets Mode 7 and OBJ continue to the bottom of the viewport.</summary>
    public Mode1FloorBand? Floor { get; }

    /// <summary>Creates an immutable full-screen composition description with explicit HUD, Mode 7, and optional Mode-1 floor boundaries.</summary>
    /// <param name="transform">The floor's transform.</param>
    /// <param name="hud">The BG3 plane whose top rows are the HUD.</param>
    /// <param name="map">The Mode 7 map the room shows.</param>
    /// <param name="hudScanlines">Top HUD height, from zero through 224 physical scanlines and divisible by eight.</param>
    /// <param name="floor">Optional value-copied lower band; its first scanline must follow the HUD and be below 224, with each map dimension either 32 or 64 tiles.</param>
    /// <exception cref="ArgumentOutOfRangeException">The HUD height or supplied floor boundary/map dimensions are invalid.</exception>
    public Mode7GameplayRenderLayer(AffineTransform transform, Layer3Cells hud, AffineFloor map,
        int hudScanlines, Mode1FloorBand? floor = null)
    {
        if (hudScanlines < 0 || hudScanlines > SnesPpuLayout.ScreenHeightPixels ||
            hudScanlines % SnesPpuLayout.BackgroundTileSizePixels != 0)
            throw new ArgumentOutOfRangeException(nameof(hudScanlines));
        if (floor is { } f && (f.FirstScanline < hudScanlines || f.FirstScanline >= SnesPpuLayout.ScreenHeightPixels ||
            f.MapSize.X is not (32 or 64) || f.MapSize.Y is not (32 or 64)))
            throw new ArgumentOutOfRangeException(nameof(floor));
        Transform = transform;
        Hud = Ensure.NotNull(hud);
        Map = Ensure.NotNull(map);
        HudScanlines = hudScanlines;
        Floor = floor;
    }
}
