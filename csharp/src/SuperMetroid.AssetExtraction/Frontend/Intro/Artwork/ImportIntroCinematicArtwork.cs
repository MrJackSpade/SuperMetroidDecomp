using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The installed opening cinematic as the import reads it (#1544): its source artwork with the colours only the import
/// pre-colours the opening's, the Ceres scenes' and the ending flyaway's sheets in. The game never reads these colours;
/// its own read of the opening (<see cref="IntroCinematicArtworkFiles.Load"/>) leaves their documents unread.
/// </summary>
public sealed class ImportIntroCinematicArtwork : IntroCinematicSourceArtwork
{
    /// <summary>Takes the opening's source artwork parts and the colours only the import reads.</summary>
    /// <param name="parts">The opening's scene artwork, character atlases and sprite compositions.</param>
    /// <param name="palette">The opening narration's colours.</param>
    /// <param name="flightPalette">The Ceres approach's colours.</param>
    /// <exception cref="ArgumentNullException">A part or a palette is null.</exception>
    /// <exception cref="InvalidDataException">The scene pages, portrait or narration tilemaps are malformed.</exception>
    public ImportIntroCinematicArtwork(IntroCinematicArtworkParts parts, IntroCinematicPalette palette, CeresFlightPalette flightPalette)
        : base(parts)
    {
        Palette = Ensure.NotNull(palette);
        FlightPalette = Ensure.NotNull(flightPalette);
    }

    /// <summary>The opening narration's selected colours, which the import pre-colours the opening's sheets in (#1512).</summary>
    public IntroCinematicPalette Palette { get; }

    /// <summary>
    /// The Ceres approach's selected 256 colours, separate from the PNGs' transport palettes, which the import pre-colours
    /// the Ceres scenes' pictures and actors in (#1512).
    /// </summary>
    public CeresFlightPalette FlightPalette { get; }

    /// <summary>The import's read of <paramref name="intro"/>.</summary>
    /// <exception cref="InvalidDataException">The opening artwork was not read for the import.</exception>
    public static ImportIntroCinematicArtwork Imported(IntroCinematicArtwork intro) => intro as ImportIntroCinematicArtwork
        ?? throw new InvalidDataException("The opening cinematic artwork was not read for the import, which alone reads its colours.");
}
