using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

public static partial class SnesGameplayFrameRenderer
{
    /// <summary>Resolves the selected Ceres gradient into RGB window rows, with no boss-state reference.</summary>
    public static ScanlineColorAddRenderLayer CaptureCeresHaze(bool ridleyIsDead,
        int intensity = CeresHazeRenderDefinitions.MaximumComponent,
        RoomFxPaletteBlendCatalog? colors = null)
    {
        var windows = new ColorAddWindow[Height];
        Array.Fill(windows, ColorAddWindow.Empty);
        for (int y = HudHeight; y < Height; y++)
        {
            (byte red, byte green, byte blue) = CeresHazeRenderDefinitions.ResolveComponents(
                y, intensity, ridleyIsDead, colors);
            windows[y] = new(0, Width - 1, ExpandFiveBit(red),
                ExpandFiveBit(green), ExpandFiveBit(blue));
        }
        return new(windows);
    }

    /// <summary>
    /// Resolves ROM curve data and the rendered explosion phase on the producer side.
    /// The renderer receives only clipped endpoints/colors, not a bus or projectile owner.
    /// </summary>
    public static ScanlineColorAddRenderLayer? CapturePowerBombColorMath(ISnesAddressSpace bus,
        SamusPowerBombExplosionState explosion, ushort layer1X, ushort layer1Y,
        int firstVisibleScanline = HudHeight)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(explosion);
        if ((uint)firstVisibleScanline > Height)
            throw new ArgumentOutOfRangeException(nameof(firstVisibleScanline));
        if (!explosion.IsActive) return null;
        var windows = new ColorAddWindow[Height];
        Array.Fill(windows, ColorAddWindow.Empty);
        int centerX = unchecked((short)(explosion.XPosition - layer1X));
        int centerY = unchecked((short)(explosion.YPosition - layer1Y));
        byte red = ExpandFiveBit(explosion.FixedColorRed);
        byte green = ExpandFiveBit(explosion.FixedColorGreen);
        byte blue = ExpandFiveBit(explosion.FixedColorBlue);
        // Gameplay protects its HUD; full-screen cinematics use the same native
        // window profile starting at scanline zero instead.
        for (int y = firstVisibleScanline; y < Height; y++)
        {
            int halfWidth = ReadPowerBombHalfWidth(explosion, y - centerY);
            if (halfWidth < 0) continue;
            int left = Math.Max(0, centerX - halfWidth);
            int right = Math.Min(Width - 1, centerX + halfWidth);
            if (left <= right) windows[y] = new((byte)left, (byte)right, red, green, blue);
        }
        return new(windows);
    }
}
