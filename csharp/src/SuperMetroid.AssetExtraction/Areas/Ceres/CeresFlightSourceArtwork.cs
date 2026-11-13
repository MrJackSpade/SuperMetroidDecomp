using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// The installed approach to Ceres (#1409, #1616, #1636): its Mode 7 floor maps and its compositions, beside what the game
/// binds of it (<see cref="Flight"/>), read from the installed documents. Its characters are import data the installation
/// never holds (<see cref="CeresFlightCharacters"/>); its colours are a palette document only the import reads
/// (<see cref="ImportIntroCinematicArtwork.FlightPalette"/>).
/// </summary>
public class CeresFlightSourceArtwork
{
    /// <summary>Retains the selected maps, backdrop, compositions and actor placements.</summary>
    /// <param name="floorMaps">Front 768 map bytes followed by rear 768 map bytes.</param>
    /// <param name="backdrop">The colour the Ceres scenes show behind every layer.</param>
    /// <param name="sprites">The approach's OAM compositions.</param>
    /// <param name="actors">The rear-view actors' placements.</param>
    private protected CeresFlightSourceArtwork(byte[] floorMaps, RgbColor backdrop, CeresFlightSpritePresentation sprites,
        CeresFlightActorPlacements actors)
    {
        Flight = new CeresFlightArtwork(backdrop, actors);
        FloorMaps = Ensure.NotNull(floorMaps);
        Sprites = Ensure.NotNull(sprites);
    }

    /// <summary>What the game binds of the approach: its backdrop and the rear-view actors' placements.</summary>
    public CeresFlightArtwork Flight { get; }

    /// <summary>Selected six-frame compositions for stars, station, asteroids, and vortex, shared with the Ceres destruction scene's sheets.</summary>
    public CeresFlightSpritePresentation Sprites { get; }

    /// <summary>Front 768 map bytes followed by rear 768 map bytes, which the import pictures into the flight's floors (#1467).</summary>
    public ReadOnlyMemory<byte> FloorMaps { get; }

    /// <summary>Identity of all selected approach maps, backdrop, compositions and placements; the characters are import data (#1636).</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("CeresFlightArtworkCatalog-v2", content =>
    {
        content.Append("mode7-maps", FloorMaps.Span);
        content.AppendColors("backdrop", [Flight.Backdrop]);
        content.Append("sprites", Convert.FromHexString(Sprites.ContentIdentity));
        content.Append("actors", Convert.FromHexString(Flight.Actors.ContentIdentity));
    });

    /// <summary>Compiles the installed Ceres approach documents into map and actor presentation data.</summary>
    /// <param name="mapJson">Version-one document containing both ordered 768-byte front/rear map slices.</param>
    /// <param name="backdrop">The colour the Ceres scenes show behind every layer, read from the opening's backdrop document.</param>
    /// <param name="spritesJson">Document containing the six required named OAM compositions.</param>
    /// <param name="actorsJson">Document containing the five required ordered rear-view actor identities and initial coordinates.</param>
    /// <remarks>Reads from the streams' current positions without disposing them.</remarks>
    /// <exception cref="ArgumentNullException">Any document stream is null.</exception>
    /// <exception cref="InvalidDataException">A document has an invalid schema, map index, actor identity or composition.</exception>
    public static CeresFlightSourceArtwork Read(Stream mapJson, RgbColor backdrop, Stream spritesJson, Stream actorsJson)
    {
        Ensure.NotNull(mapJson);
        Ensure.NotNull(spritesJson);
        Ensure.NotNull(actorsJson);
        return new CeresFlightSourceArtwork(CeresFlightCharacters.ReadMaps(mapJson), backdrop,
            CeresFlightSpritePresentation.Load(spritesJson), CeresFlightActorLayoutFile.Read(actorsJson));
    }
}
