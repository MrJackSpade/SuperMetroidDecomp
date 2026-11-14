using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// One ending colour resource as the import decodes it from the cartridge (#1464, #1544): a whole screen's colours, a run
/// of colours a scene loads, such as the final gunship's colours, or the logo crossfade's colours by step and layer. The
/// import pre-colours the ending's sheets in them (#1512) and installs each scene's backdrop as a colour; the game loads
/// none of them.
/// </summary>
public sealed class EndingPalette
{
    /// <summary>The colours of the logo crossfade's layers at one step.</summary>
    internal const int CrossfadeLayerColours = 16;

    /// <summary>The logo crossfade's colours: every step's background colours, then its sprite colours.</summary>
    private const int CrossfadeColours = EndingLogoDefinitions.CrossfadeSteps * 2 * CrossfadeLayerColours;

    /// <summary>The colours in resource order.</summary>
    internal readonly ImmutableArray<RgbColor> colours;
    /// <summary>What the colours are: a whole screen's, a run's or the logo crossfade's.</summary>
    internal readonly Kind kind;

    /// <summary>Keeps a whole screen's, a run's or the logo crossfade's decoded colours.</summary>
    /// <param name="colours">
    /// The colours in resource order: a whole screen's background colours, then its sprite colours; the crossfade's steps
    /// in order, each step's background colours before its sprite colours.
    /// </param>
    /// <param name="kind">What the colours are.</param>
    /// <exception cref="ArgumentException">A whole screen's or the crossfade's colours are not as many as it holds.</exception>
    internal EndingPalette(ImmutableArray<RgbColor> colours, Kind kind)
    {
        if (colours.IsDefault || (kind == Kind.Screen && colours.Length != ScreenColourImage.ColorCount)
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

    /// <summary>The solid colour a whole screen shows behind every layer once it is loaded: its first background colour.</summary>
    /// <exception cref="InvalidOperationException">This resource is not a whole screen's.</exception>
    internal RgbColor Backdrop => RequireScreen()[0];

    /// <summary>The whole screen's colours.</summary>
    /// <exception cref="InvalidOperationException">This resource is not a whole screen's.</exception>
    internal ImmutableArray<RgbColor> RequireScreen() => kind == Kind.Screen
        ? colours
        : throw new InvalidOperationException("This ending palette is not a whole screen's colours.");

    /// <summary>What an ending colour resource holds.</summary>
    internal enum Kind
    {
        /// <summary>A whole screen's colours.</summary>
        Screen,

        /// <summary>A run of colours a scene loads.</summary>
        Run,

        /// <summary>The logo crossfade's colours by step and layer.</summary>
        LogoCrossfade,
    }
}
