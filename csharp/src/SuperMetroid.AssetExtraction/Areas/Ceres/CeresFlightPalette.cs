using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// The Ceres approach's installed colours as the import reads them (#1464, #1512): its background colours then its
/// sprite colours in the cartridge's colour order, every colour kept as installed. The import pre-colours the Ceres
/// scenes' pictures from these colours; the game keeps only their backdrop.
/// </summary>
public sealed class CeresFlightPalette
{
    /// <summary>Every colour of the approach: its background colours, then its sprite colours.</summary>
    internal const int ColourCount = 2 * OwnerColours;

    /// <summary>The colours of each owner, the backgrounds' or the sprites'.</summary>
    internal const int OwnerColours = 128;

    /// <summary>The backdrop, the first background set's transparent colour.</summary>
    private const int BackdropColour = 0;

    /// <summary>Keeps the complete installed colours.</summary>
    /// <param name="supplied">The approach's 256 colours in the cartridge's colour order.</param>
    /// <exception cref="InvalidDataException">The approach does not supply exactly 256 colours.</exception>
    internal CeresFlightPalette(ReadOnlySpan<RgbColor> supplied)
    {
        if (supplied.Length != ColourCount)
            throw new InvalidDataException($"The Ceres flight palette requires {ColourCount} colours.");
        Colours = [.. supplied];
    }

    /// <summary>The installed colours in the cartridge's colour order.</summary>
    public ImmutableArray<RgbColor> Colours { get; }

    /// <summary>The installed sprite colours, which the Ceres scenes' actors are pictured in.</summary>
    public ImmutableArray<RgbColor> SpriteColours => Colours[OwnerColours..];

    /// <summary>The colour the Ceres scenes show behind every layer.</summary>
    public RgbColor Backdrop => Colours[BackdropColour];
}
