using SuperMetroid.Core.Rendering;

namespace SuperMetroid.TestSupport.FrameRendering;

/// <summary>
/// The scroll a described line scroll gives a screen line (#1468), as Tiles.hlsl's <c>LineScroll</c> draws it, evaluated
/// on the CPU so a check can hold a described shape against the cartridge's per-line scrolls. The game keeps no table of
/// lines; only checks evaluate one.
/// </summary>
internal static class ScrollShapeLines
{
    /// <summary>The byte angle's steps in one whole turn.</summary>
    private const int AngleStepsPerTurn = 256;

    /// <summary>The scroll <paramref name="shape"/> gives screen line <paramref name="line"/>, or <paramref name="ownScroll"/> without a shape.</summary>
    internal static ushort Scroll(LineScroll? shape, int line, int ownScroll = 0) => shape switch
    {
        null => unchecked((ushort)ownScroll),
        ScrollWave wave => WaveScroll(wave, line),
        ScrollStretch stretch => StretchScroll(stretch, line),
        SampledScrollWave sampled => SampledScroll(sampled, line),
        ScrollBands bands => BandScroll(bands, line),
        _ => throw new NotSupportedException($"Line scroll {shape.GetType().Name} has no evaluation."),
    };

    /// <summary>The scrolls <paramref name="shape"/> gives the gameplay lines below the HUD, from the first.</summary>
    internal static ushort[] GameplayScrolls(LineScroll? shape, int ownScroll = 0) =>
        [.. Enumerable.Range(ScreenGeometry.GameplayHudHeightPixels, BackgroundLineScrolls.GameplayLines).Select(line => Scroll(shape, line, ownScroll))];

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

    /// <summary>
    /// The scroll <paramref name="wave"/> gives screen line <paramref name="line"/>: inside its band, the base moved by the
    /// sample the line reaches times the amplitude, rounded down; outside it, the base.
    /// </summary>
    internal static ushort SampledScroll(SampledScrollWave wave, int line) =>
        line < wave.FirstLine || line >= wave.EndLine ? checked((ushort)wave.BaseScroll)
        : unchecked((ushort)(wave.BaseScroll + EightEight.Multiply(wave.Samples[(wave.Phase + line) % wave.Period], wave.Amplitude)));

    /// <summary>The scroll <paramref name="bands"/> give screen line <paramref name="line"/>: the last band beginning on or above it, or the base above the first.</summary>
    internal static ushort BandScroll(ScrollBands bands, int line)
    {
        int scroll = bands.BaseScroll;
        foreach (ScrollBand band in bands.Bands)
        {
            if (band.FirstLine > line) break;
            scroll = band.Scroll;
        }
        return checked((ushort)scroll);
    }
}
