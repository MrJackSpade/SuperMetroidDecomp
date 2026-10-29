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
    private readonly ScreenColours? screen;
    /// <summary>Exact selected BGR555 bytes for a run of colours.</summary>
    private readonly byte[]? colorBytes;
    /// <summary>An independently edited logo crossfade's supplied colours.</summary>
    private readonly EndingLogoCrossfadeColours? crossfade;
    /// <summary>Endpoint-backed logo fade used only when it reproduces every supplied crossfade color.</summary>
    private readonly EndingLogoPaletteFade? logoFade;

    /// <summary>Keeps a whole screen's colours.</summary>
    private EndingPalette(ScreenColours screen) => this.screen = screen;
    /// <summary>Retains a validated compiled color run without recalculating its supplied inks.</summary>
    private EndingPalette(byte[] colorBytes) => this.colorBytes = colorBytes;
    /// <summary>Retains an edited logo crossfade's supplied colours.</summary>
    private EndingPalette(EndingLogoCrossfadeColours crossfade) => this.crossfade = crossfade;
    /// <summary>Retains the verified logo interpolation instead of a duplicate 512-color table.</summary>
    private EndingPalette(EndingLogoPaletteFade logoFade) => this.logoFade = logoFade;

    /// <summary>Keeps a whole ending screen's selected colours, which the import pipeline decoded by owner.</summary>
    /// <param name="colours">The screen's colours by owner.</param>
    internal static EndingPalette FromScreen(ScreenColours colours) => new(Ensure.NotNull(colours));

    /// <summary>Keeps one ending colour run's selected colours.</summary>
    /// <param name="colours">The selected colours.</param>
    internal static EndingPalette FromRun(ReadOnlySpan<RgbColor> colours) => new(Words(colours));

    /// <summary>
    /// Keeps the logo crossfade's selected colours, which the import pipeline decoded by step and layer, recognising the
    /// calculated fade when its colours follow it.
    /// </summary>
    /// <param name="colours">The crossfade's colours.</param>
    internal static EndingPalette FromCrossfade(EndingLogoCrossfadeColours colours) =>
        EndingLogoPaletteFade.TryCreate(colours) is { } fade ? new EndingPalette(fade) : new EndingPalette(colours);

    /// <summary>The whole screen's colours by owner.</summary>
    /// <exception cref="InvalidOperationException">This resource is a run of colours, not a whole screen's.</exception>
    public ScreenColours Screen => screen ?? throw new InvalidOperationException("This ending palette is a run of colours, not a whole screen's.");

    /// <summary>Byte/export view, for the content identity: a whole screen's background sets' colour words then its sprite
    /// sets', a run's words in order, or the crossfade's steps in order, each step's background colours before its sprite
    /// colours. A computed fade is materialized only for this request, never stored as a generated lookup cache; the logo
    /// reads its colours by step and layer.</summary>
    public ReadOnlyMemory<byte> Bytes
    {
        get
        {
            if (screen is not null) return Words([.. screen.Backgrounds, .. screen.Sprites]);
            if (colorBytes is not null) return colorBytes;
            var colours = new List<RgbColor>(EndingLogoPaletteFade.ColorCount);
            for (int step = 0; step < EndingLogoDefinitions.PaletteSteps; step++)
            foreach (EndingLogoLayer layer in Enum.GetValues<EndingLogoLayer>())
            for (int colour = 0; colour < EndingLogoPaletteFade.ColoursPerLayer; colour++)
                colours.Add(LogoCrossfadeColour(step, layer, colour));
            return Words([.. colours]);
        }
    }
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

    /// <summary><paramref name="colours"/> as little-endian colour words.</summary>
    private static byte[] Words(ReadOnlySpan<RgbColor> colours)
    {
        var words = new byte[colours.Length * RgbColor.ByteCount];
        for (int index = 0; index < colours.Length; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(words.AsSpan(index * RgbColor.ByteCount), colours[index].ToWord());
        return words;
    }
}
