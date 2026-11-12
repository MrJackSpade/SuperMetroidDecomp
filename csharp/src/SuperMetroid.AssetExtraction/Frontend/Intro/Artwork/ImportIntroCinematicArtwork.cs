using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The opening cinematic as the import reads it (#1544, #1636): its installed documents with its characters, from the
/// component's import data, and the colours only the import pre-colours the opening's, the Ceres scenes' and the ending
/// flyaway's sheets in. The game reads neither; its own read of the opening (<see cref="IntroCinematicArtworkFiles.Load"/>)
/// reads no character page and leaves the colour documents unread.
/// </summary>
public sealed class ImportIntroCinematicArtwork : IntroCinematicSourceArtwork
{
    /// <summary>Takes the opening's installed documents, its characters and the colours only the import reads.</summary>
    /// <param name="parts">The opening's scene artwork, tilemaps and sprite compositions.</param>
    /// <param name="characters">The opening's characters; their Ceres approach takes the documents' place.</param>
    /// <param name="palette">The opening narration's colours.</param>
    /// <param name="flightPalette">The Ceres approach's colours.</param>
    /// <exception cref="ArgumentNullException">A part, the characters or a palette is null.</exception>
    /// <exception cref="InvalidDataException">The scene pages, portrait, narration or Zebes tilemaps, or the reveal's characters, are malformed.</exception>
    public ImportIntroCinematicArtwork(IntroCinematicArtworkParts parts, IntroCinematicCharacters characters, IntroCinematicPalette palette,
        CeresFlightPalette flightPalette)
        : base(Ensure.NotNull(parts) with { CeresFlight = Ensure.NotNull(characters).CeresFlight })
    {
        BackgroundCharacters = Ensure.NotNull(characters.BackgroundCharacters);
        IntroObjectCharacters = Ensure.NotNull(characters.IntroObjectCharacters);
        CinematicObjectCharacters = Ensure.NotNull(characters.CinematicObjectCharacters);
        ZebesReveal = ZebesRevealArtwork.Of(ZebesRevealMap, characters.ZebesRevealCharacters);
        Palette = Ensure.NotNull(palette);
        FlightPalette = Ensure.NotNull(flightPalette);
    }

    /// <summary>BG characters of the illustrated scenes.</summary>
    public RoomCharacterAtlas BackgroundCharacters { get; }

    /// <summary>The intro's fixed-bank OBJ characters.</summary>
    public RoomCharacterAtlas IntroObjectCharacters { get; }

    /// <summary>The intro's cinematic OBJ characters.</summary>
    public RoomCharacterAtlas CinematicObjectCharacters { get; }

    /// <summary>The Zebes reveal's tilemap and characters, which the planet's picture is composed from.</summary>
    public ZebesRevealArtwork ZebesReveal { get; }

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
