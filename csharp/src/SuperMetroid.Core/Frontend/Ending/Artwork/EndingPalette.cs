using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// One independently editable ending colour resource (#1464): a whole screen's colours, a run of colours a scene loads,
/// such as the final gunship's colours, or the logo crossfade's colours by step and layer. The import decodes each and
/// pre-colours the ending's sheets in them (#1512); the ending itself reads only a whole screen's backdrop.
/// </summary>
public sealed class EndingPalette
{
    /// <summary>The colours of a whole screen or a run, in resource order; empty for the logo crossfade.</summary>
    private readonly ImmutableArray<RgbColor> colours;
    /// <summary>Whether <see cref="colours"/> is a whole screen's: its background colours, then its sprite colours.</summary>
    private readonly bool screen;
    /// <summary>An independently edited logo crossfade's supplied colours.</summary>
    private readonly EndingLogoCrossfadeColours? crossfade;
    /// <summary>Endpoint-backed logo fade used only when it reproduces every supplied crossfade color.</summary>
    private readonly EndingLogoPaletteFade? logoFade;

    /// <summary>Keeps a whole screen's or a run's decoded colours.</summary>
    /// <param name="colours">The colours in resource order: a whole screen's background colours, then its sprite colours.</param>
    /// <param name="screen">Whether the colours are a whole screen's.</param>
    /// <exception cref="ArgumentException">A whole screen's colours are not one background and one sprite half.</exception>
    internal EndingPalette(ImmutableArray<RgbColor> colours, bool screen)
    {
        if (colours.IsDefault || (screen && colours.Length != 2 * SpriteImage.SpriteColourCount))
            throw new ArgumentException("A whole ending screen holds its background colours, then as many sprite colours.", nameof(colours));
        this.colours = colours;
        this.screen = screen;
    }

    /// <summary>Retains an edited logo crossfade's supplied colours.</summary>
    /// <param name="crossfade">The crossfade's colours by step and layer.</param>
    internal EndingPalette(EndingLogoCrossfadeColours crossfade)
    {
        this.crossfade = crossfade;
        colours = [];
    }

    /// <summary>Retains the verified logo interpolation instead of a duplicate 512-color table.</summary>
    /// <param name="logoFade">The calculated fade the import recognised in the supplied crossfade.</param>
    internal EndingPalette(EndingLogoPaletteFade logoFade)
    {
        this.logoFade = logoFade;
        colours = [];
    }

    /// <summary>Whether this resource is a whole screen's colours.</summary>
    public bool IsScreen => screen;

    /// <summary>Whether this resource is the logo crossfade's colours by step and layer.</summary>
    public bool IsLogoCrossfade => crossfade is not null || logoFade is not null;

    /// <summary>A whole screen's or a run's colours in resource order: a whole screen's background colours, then its sprite colours.</summary>
    /// <exception cref="InvalidOperationException">This resource is the logo crossfade, read by step and layer.</exception>
    public ImmutableArray<RgbColor> Colours => IsLogoCrossfade
        ? throw new InvalidOperationException("The logo crossfade's colours are read by step and layer.")
        : colours;

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
    public int ColorCount => IsLogoCrossfade ? EndingLogoPaletteFade.ColorCount : colours.Length;

    /// <summary>The logo crossfade's colour <paramref name="colour"/> of <paramref name="layer"/> at crossfade step <paramref name="step"/>,
    /// evaluating a recognized fade directly.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The step, layer or colour is outside the crossfade.</exception>
    /// <exception cref="InvalidOperationException">This resource is not the logo crossfade.</exception>
    public RgbColor LogoCrossfadeColour(int step, EndingLogoLayer layer, int colour) =>
        logoFade?.Color(step, layer, colour) ?? (crossfade ?? throw new InvalidOperationException(
            "Only the logo crossfade is read by step and layer.")).Colour(step, layer, colour);

    /// <summary>The whole screen's colours.</summary>
    /// <exception cref="InvalidOperationException">This resource is not a whole screen's.</exception>
    private ImmutableArray<RgbColor> RequireScreen() => screen
        ? colours
        : throw new InvalidOperationException("This ending palette is not a whole screen's colours.");
}
