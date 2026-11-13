using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// The installed approach to Ceres (#1409, #1616, #1636): its Mode 7 floor maps, beside what the game binds of it
/// (<see cref="Flight"/>), read from the installed documents. Its characters and its compositions, whose parts are the
/// original's packed sprite placements (#1638), are import data the installation never holds
/// (<see cref="CeresFlightCharacters"/>); its colours are a palette document only the import reads
/// (<see cref="ImportIntroCinematicArtwork.FlightPalette"/>).
/// </summary>
public class CeresFlightSourceArtwork
{
    /// <summary>Retains the selected maps, backdrop and actor placements.</summary>
    /// <param name="floorMaps">Front 768 map bytes followed by rear 768 map bytes.</param>
    /// <param name="backdrop">The colour the Ceres scenes show behind every layer.</param>
    /// <param name="actors">The rear-view actors' placements.</param>
    private protected CeresFlightSourceArtwork(byte[] floorMaps, RgbColor backdrop, CeresFlightActorPlacements actors)
    {
        Flight = new CeresFlightArtwork(backdrop, actors);
        FloorMaps = Ensure.NotNull(floorMaps);
    }

    /// <summary>What the game binds of the approach: its backdrop and the rear-view actors' placements.</summary>
    public CeresFlightArtwork Flight { get; }

    /// <summary>Front 768 map bytes followed by rear 768 map bytes, which the import pictures into the flight's floors (#1467).</summary>
    public ReadOnlyMemory<byte> FloorMaps { get; }

    /// <summary>
    /// Identity of all selected approach maps, backdrop and placements; the characters (#1636) and compositions (#1638) are
    /// import data, which the import's read adds (<see cref="CeresFlightCharacters"/>).
    /// </summary>
    public virtual string ContentIdentity => SelectedPresentationHash.Create("CeresFlightArtworkCatalog-v3", content =>
    {
        content.Append("mode7-maps", FloorMaps.Span);
        content.AppendColors("backdrop", [Flight.Backdrop]);
        content.Append("actors", Convert.FromHexString(Flight.Actors.ContentIdentity));
    });

    /// <summary>Compiles the installed Ceres approach documents into map and actor presentation data.</summary>
    /// <param name="mapJson">Version-one document containing both ordered 768-byte front/rear map slices.</param>
    /// <param name="backdrop">The colour the Ceres scenes show behind every layer, read from the opening's backdrop document.</param>
    /// <param name="actorsJson">Document containing the five required ordered rear-view actor identities and initial coordinates.</param>
    /// <remarks>Reads from the streams' current positions without disposing them.</remarks>
    /// <exception cref="ArgumentNullException">Any document stream is null.</exception>
    /// <exception cref="InvalidDataException">A document has an invalid schema, map index or actor identity.</exception>
    public static CeresFlightSourceArtwork Read(Stream mapJson, RgbColor backdrop, Stream actorsJson)
    {
        Ensure.NotNull(mapJson);
        Ensure.NotNull(actorsJson);
        return new CeresFlightSourceArtwork(CeresFlightCharacters.ReadMaps(mapJson), backdrop, CeresFlightActorLayoutFile.Read(actorsJson));
    }
}
