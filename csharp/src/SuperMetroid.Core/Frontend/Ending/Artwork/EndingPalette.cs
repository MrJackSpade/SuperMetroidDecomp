using System.Buffers.Binary;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// One independently editable ending colour resource (#1464): a whole screen's colours by owner, which the import
/// pipeline decodes, a run of colours a scene loads into one of the screen's groups, such as the final gunship's colours,
/// or the logo crossfade's colours by step and layer.
/// </summary>
public sealed class EndingPalette
{
    /// <summary>A whole screen's colours by owner, or null for a run of colours.</summary>
    internal readonly ScreenColours? screen;
    /// <summary>Exact selected BGR555 bytes for a run of colours.</summary>
    internal readonly byte[]? colorBytes;
    /// <summary>An independently edited logo crossfade's supplied colours.</summary>
    private readonly EndingLogoCrossfadeColours? crossfade;
    /// <summary>Endpoint-backed logo fade used only when it reproduces every supplied crossfade color.</summary>
    private readonly EndingLogoPaletteFade? logoFade;

    /// <summary>Keeps a whole screen's colours, which the import pipeline decoded by owner.</summary>
    /// <param name="screen">The screen's colours by owner.</param>
    internal EndingPalette(ScreenColours screen) => this.screen = screen;
    /// <summary>Retains a validated compiled color run without recalculating its supplied inks.</summary>
    /// <param name="colorBytes">The run's little-endian colour words.</param>
    internal EndingPalette(byte[] colorBytes) => this.colorBytes = colorBytes;
    /// <summary>Retains an edited logo crossfade's supplied colours.</summary>
    /// <param name="crossfade">The crossfade's colours by step and layer.</param>
    internal EndingPalette(EndingLogoCrossfadeColours crossfade) => this.crossfade = crossfade;
    /// <summary>Retains the verified logo interpolation instead of a duplicate 512-color table.</summary>
    /// <param name="logoFade">The calculated fade the import recognised in the supplied crossfade.</param>
    internal EndingPalette(EndingLogoPaletteFade logoFade) => this.logoFade = logoFade;

    /// <summary>The whole screen's colours by owner.</summary>
    /// <exception cref="InvalidOperationException">This resource is a run of colours, not a whole screen's.</exception>
    public ScreenColours Screen => screen ?? throw new InvalidOperationException("This ending palette is a run of colours, not a whole screen's.");

    /// <summary>Number of colours in the complete resource: both owners' for a whole screen, including all 512 ordered colors when this is the sixteen-step logo crossfade.</summary>
    public int ColorCount => screen is not null ? screen.Backgrounds.Length + screen.Sprites.Length
        : colorBytes is not null ? colorBytes.Length / RgbColor.ByteCount : EndingLogoPaletteFade.ColorCount;

    /// <summary>Reads one selected color of a run.</summary>
    /// <param name="index">Zero-based color position within <see cref="ColorCount"/>.</param>
    /// <returns>A compiled BGR555 word: red in bits 0..4, green in 5..9 and blue in 10..14.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside this resource.</exception>
    /// <exception cref="InvalidOperationException">
    /// This resource holds a whole screen's colours, read by owner, or the logo crossfade's, read by step and layer.
    /// </exception>
    public RgbColor Color(int index)
    {
        byte[] run = colorBytes ?? throw new InvalidOperationException(
            "A whole screen's colours are read by owner through Screen, and the logo crossfade's by step and layer.");
        if ((uint)index >= ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return RgbColor.FromWord(BinaryPrimitives.ReadUInt16LittleEndian(run.AsSpan(index * RgbColor.ByteCount)));
    }

    /// <summary>The logo crossfade's colour <paramref name="colour"/> of <paramref name="layer"/> at crossfade step <paramref name="step"/>,
    /// evaluating a recognized fade directly.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The step, layer or colour is outside the crossfade.</exception>
    /// <exception cref="InvalidOperationException">This resource is not the logo crossfade.</exception>
    internal RgbColor LogoCrossfadeColour(int step, EndingLogoLayer layer, int colour) =>
        logoFade?.Color(step, layer, colour) ?? (crossfade ?? throw new InvalidOperationException(
            "Only the logo crossfade is read by step and layer.")).Colour(step, layer, colour);

    /// <summary>Changes <paramref name="destination"/> to this run's colours from <paramref name="sourceColor"/>.</summary>
    /// <param name="destination">The screen colours to change; the others keep theirs.</param>
    /// <param name="sourceColor">Zero-based starting color in this resource.</param>
    /// <exception cref="ArgumentOutOfRangeException">The source range is outside this resource.</exception>
    public void LoadTo(ColourRun destination, int sourceColor)
    {
        Ensure.BetweenInclusive(0, ColorCount - destination.Count, sourceColor);
        for (int colour = 0; colour < destination.Count; colour++)
            destination.Write(colour, Color(sourceColor + colour));
    }
}
