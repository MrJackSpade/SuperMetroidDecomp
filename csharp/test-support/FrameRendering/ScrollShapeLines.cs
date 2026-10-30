using SuperMetroid.Core.Rendering;

namespace SuperMetroid.TestSupport.FrameRendering;

/// <summary>
/// The scroll a described wave or stretch gives a screen line (#1468), as Tiles.hlsl's <c>LineDisplacement</c> draws it,
/// evaluated on the CPU so a check can hold a described shape against the cartridge's per-line scrolls. The game keeps
/// no table of lines; only checks evaluate one.
/// </summary>
internal static class ScrollShapeLines
{
    /// <summary>The byte angle's steps in one whole turn.</summary>
    private const int AngleStepsPerTurn = 256;

    /// <summary>The horizontal scroll <paramref name="wave"/> gives screen line <paramref name="line"/>: its base scroll and the wave's displacement there.</summary>
    internal static ushort WaveScroll(ScrollWave wave, int line) =>
        unchecked((ushort)(wave.BaseScroll + WaveDisplacement(wave, line)));

    /// <summary>
    /// The sideways displacement of screen line <paramref name="line"/>: the game's sine at the line's angle times the
    /// amplitude, rounded toward zero, inside the wave's band, and none outside it.
    /// </summary>
    internal static int WaveDisplacement(ScrollWave wave, int line)
    {
        if (line < wave.FirstLine || line >= wave.EndLine) return 0;
        int sine = Trigonometry.SignedSine(new ByteAngle(unchecked((byte)(wave.Phase.RawValue + line * AngleStepsPerTurn / wave.Period))));
        int magnitude = SixteenSixteen.Multiply(Math.Abs(sine), wave.Amplitude);
        return sine < 0 ? -magnitude : magnitude;
    }

    /// <summary>
    /// The vertical scroll <paramref name="stretch"/> gives screen line <paramref name="line"/>: the scroll above the band,
    /// the base less the rows the band has fallen behind by that line, or the base below the band.
    /// </summary>
    internal static ushort StretchScroll(ScrollStretch stretch, int line) =>
        line < stretch.FirstLine ? checked((ushort)stretch.ScrollAbove)
        : line >= stretch.EndLine ? checked((ushort)stretch.BaseScroll)
        : unchecked((ushort)(stretch.BaseScroll - SixteenSixteen.Multiply(line - stretch.FirstLine, stretch.Slope)));
}
