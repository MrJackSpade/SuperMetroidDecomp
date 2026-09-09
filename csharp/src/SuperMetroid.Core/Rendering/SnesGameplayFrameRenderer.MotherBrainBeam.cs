using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rendering;

public static partial class SnesGameplayFrameRenderer
{
    /// <summary>
    /// Captures bank-$88 layer configuration $24's fixed-color addition through window 1.
    /// This shared layer is consumed by both the software and Windows Direct3D backends.
    /// </summary>
    public static ScanlineColorAddRenderLayer? CaptureMotherBrainRainbowBeam(MotherBrainRainbowBeamHdmaState beam)
    {
        ArgumentNullException.ThrowIfNull(beam);
        if (!beam.Active) return null;
        var windows = new ColorAddWindow[Height];
        Array.Fill(windows, ColorAddWindow.Empty);
        byte red = ExpandFiveBit((byte)(beam.Color & 31));
        byte green = ExpandFiveBit((byte)((beam.Color >> 5) & 31));
        byte blue = ExpandFiveBit((byte)((beam.Color >> 10) & 31));
        for (int y = HudHeight; y < Height; y++)
        {
            ushort word = beam.Windows[y];
            byte left = (byte)word, right = (byte)(word >> 8);
            if (left <= right) windows[y] = new(left, right, red, green, blue);
        }
        return new(windows);
    }
}
