using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// The installed approach to Ceres (#1409, #1616, #1636): what the game binds of it (<see cref="Flight"/>), read from the
/// installed documents. Its characters, its Mode 7 floor maps (#1693) and its compositions, whose parts are the original's
/// packed sprite placements (#1638), are import data the installation never holds (<see cref="CeresFlightCharacters"/>);
/// its colours are a palette document only the import reads (<see cref="ImportIntroCinematicArtwork.FlightPalette"/>).
/// </summary>
public class CeresFlightSourceArtwork
{
    /// <summary>Retains the selected backdrop and actor placements.</summary>
    /// <param name="backdrop">The colour the Ceres scenes show behind every layer.</param>
    /// <param name="actors">The rear-view actors' placements.</param>
    private protected CeresFlightSourceArtwork(RgbColor backdrop, CeresFlightActorPlacements actors) =>
        Flight = new CeresFlightArtwork(backdrop, actors);

    /// <summary>What the game binds of the approach: its backdrop and the rear-view actors' placements.</summary>
    public CeresFlightArtwork Flight { get; }

    /// <summary>
    /// Identity of the selected approach backdrop and placements, which is the identity of what the game binds of them
    /// (<see cref="Flight"/>); the characters (#1636), floor maps (#1693) and compositions (#1638) are import data, which the
    /// import's read adds (<see cref="CeresFlightCharacters"/>).
    /// </summary>
    public virtual string ContentIdentity => Flight.ContentIdentity;

    /// <summary>Compiles the installed Ceres approach documents into actor presentation data.</summary>
    /// <param name="backdrop">The colour the Ceres scenes show behind every layer, read from the opening's backdrop document.</param>
    /// <param name="actorsJson">Document containing the five required ordered rear-view actor identities and initial coordinates.</param>
    /// <remarks>Reads from the stream's current position without disposing it.</remarks>
    /// <exception cref="ArgumentNullException">The document stream is null.</exception>
    /// <exception cref="InvalidDataException">The document has an invalid schema or actor identity.</exception>
    public static CeresFlightSourceArtwork Read(RgbColor backdrop, Stream actorsJson) =>
        new(backdrop, CeresFlightActorLayoutFile.Read(Ensure.NotNull(actorsJson)));
}
