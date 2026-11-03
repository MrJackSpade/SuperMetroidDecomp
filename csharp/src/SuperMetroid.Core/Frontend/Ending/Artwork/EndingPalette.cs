using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// One independently editable ending colour resource (#1464): a whole screen's colours, a run of colours a scene loads,
/// such as the final gunship's colours, or the logo crossfade's colours by step and layer. The import decodes each,
/// recognising the logo's calculated fade, and pre-colours the ending's sheets in them (#1512); the ending itself reads
/// only a whole screen's backdrop.
/// </summary>
public sealed class EndingPalette
{
    /// <summary>The colours of the logo crossfade's layers at one step.</summary>
    private const int CrossfadeLayerColours = 16;

    /// <summary>The logo crossfade's colours: every step's background colours, then its sprite colours.</summary>
    private const int CrossfadeColours = EndingLogoDefinitions.PaletteSteps * 2 * CrossfadeLayerColours;

    /// <summary>The colours in resource order.</summary>
    private readonly ImmutableArray<RgbColor> colours;
    /// <summary>What the colours are: a whole screen's, a run's or the logo crossfade's.</summary>
    private readonly Kind kind;

    /// <summary>Keeps a whole screen's, a run's or the logo crossfade's decoded colours.</summary>
    /// <param name="colours">
    /// The colours in resource order: a whole screen's background colours, then its sprite colours; the crossfade's steps
    /// in order, each step's background colours before its sprite colours.
    /// </param>
    /// <param name="kind">What the colours are.</param>
    /// <exception cref="ArgumentException">A whole screen's or the crossfade's colours are not as many as it holds.</exception>
    private EndingPalette(ImmutableArray<RgbColor> colours, Kind kind)
    {
        if (colours.IsDefault || (kind == Kind.Screen && colours.Length != 2 * SpriteImage.SpriteColourCount)
            || (kind == Kind.LogoCrossfade && colours.Length != CrossfadeColours))
            throw new ArgumentException($"An ending {kind} palette does not hold its colours.", nameof(colours));
        this.colours = colours;
        this.kind = kind;
    }

    /// <summary>Keeps a whole screen's or a run's decoded colours.</summary>
    /// <param name="colours">The colours in resource order: a whole screen's background colours, then its sprite colours.</param>
    /// <param name="screen">Whether the colours are a whole screen's.</param>
    /// <exception cref="ArgumentException">A whole screen's colours are not one background and one sprite half.</exception>
    internal EndingPalette(ImmutableArray<RgbColor> colours, bool screen)
        : this(colours, screen ? Kind.Screen : Kind.Run)
    {
    }

    /// <summary>The logo crossfade's decoded colours: its steps in order, each step's background colours before its sprite colours.</summary>
    /// <exception cref="ArgumentException">The colours are not every step's.</exception>
    internal static EndingPalette LogoCrossfade(ImmutableArray<RgbColor> colours) => new(colours, Kind.LogoCrossfade);

    /// <summary>Whether this resource is a whole screen's colours.</summary>
    public bool IsScreen => kind == Kind.Screen;

    /// <summary>Whether this resource is the logo crossfade's colours by step and layer.</summary>
    public bool IsLogoCrossfade => kind == Kind.LogoCrossfade;

    /// <summary>
    /// The colours in resource order: a whole screen's background colours, then its sprite colours; a run's in order; the
    /// crossfade's steps in order, each step's background colours before its sprite colours.
    /// </summary>
    public ImmutableArray<RgbColor> Colours => colours;

    /// <summary>A whole screen's background colours.</summary>
    /// <exception cref="InvalidOperationException">This resource is not a whole screen's.</exception>
    public ReadOnlySpan<RgbColor> Backgrounds => RequireScreen().AsSpan(0, SpriteImage.SpriteColourCount);

    /// <summary>A whole screen's sprite colours.</summary>
    /// <exception cref="InvalidOperationException">This resource is not a whole screen's.</exception>
    public ReadOnlySpan<RgbColor> Sprites => RequireScreen().AsSpan(SpriteImage.SpriteColourCount, SpriteImage.SpriteColourCount);

    /// <summary>The solid colour a whole screen shows behind every layer once it is loaded: its first background colour.</summary>
    /// <exception cref="InvalidOperationException">This resource is not a whole screen's.</exception>
    public RgbColor Backdrop => RequireScreen()[0];

    /// <summary>Number of colours in the complete resource, including all 512 ordered colours of the sixteen-step logo crossfade.</summary>
    public int ColorCount => colours.Length;

    /// <summary>The logo crossfade's colour <paramref name="colour"/> of <paramref name="layer"/> at crossfade step <paramref name="step"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The step, layer or colour is outside the crossfade.</exception>
    /// <exception cref="InvalidOperationException">This resource is not the logo crossfade.</exception>
    internal RgbColor LogoCrossfadeColour(int step, EndingLogoLayer layer, int colour)
    {
        if (kind != Kind.LogoCrossfade)
            throw new InvalidOperationException("Only the logo crossfade is read by step and layer.");
        Ensure.BetweenInclusive(0, EndingLogoDefinitions.PaletteSteps - 1, step);
        Ensure.BetweenInclusive(0, CrossfadeLayerColours - 1, colour);
        int place = layer switch
        {
            EndingLogoLayer.Background => 0,
            EndingLogoLayer.Sprites => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Undefined logo layer."),
        };
        return colours[(step * 2 + place) * CrossfadeLayerColours + colour];
    }

    /// <summary>The whole screen's colours.</summary>
    /// <exception cref="InvalidOperationException">This resource is not a whole screen's.</exception>
    private ImmutableArray<RgbColor> RequireScreen() => kind == Kind.Screen
        ? colours
        : throw new InvalidOperationException("This ending palette is not a whole screen's colours.");

    /// <summary>What an ending colour resource holds.</summary>
    private enum Kind
    {
        /// <summary>A whole screen's colours.</summary>
        Screen,

        /// <summary>A run of colours a scene loads.</summary>
        Run,

        /// <summary>The logo crossfade's colours by step and layer.</summary>
        LogoCrossfade,
    }
}
