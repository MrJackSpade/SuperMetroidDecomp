using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The installed approach to Ceres (#1409): its backdrop and actor placements.
/// The installed flight sheets picture the floors, characters and actor compositions in the flight's colours (#1512);
/// their maps, pixels, parts and palette belong to the import pipeline that reads the installed artwork, which also names
/// the content's identity (#1463, #1467).
/// Flight motion, artwork load order and the SPACE COLONY letter script remain compiled logic.
/// </summary>
public abstract class CeresFlightArtwork
{
    /// <summary>Retains the selected backdrop and actor resources.</summary>
    /// <param name="backdrop">The colour the Ceres scenes show behind every layer once they load the flight's colours.</param>
    /// <param name="actors">The rear-view actors' placements.</param>
    internal CeresFlightArtwork(RgbColor backdrop, CeresFlightActorPlacements actors)
    {
        Backdrop = backdrop;
        Actors = actors;
    }

    /// <summary>
    /// The solid colour the Ceres flight and the Ceres destruction show behind every layer once they load the flight's
    /// colours (#1512); every picture they draw is pre-coloured in the flight's colours by the import.
    /// </summary>
    public RgbColor Backdrop { get; }
    /// <summary>Selected initial screen-pixel positions for the five named rear-view actors; their movement and instruction timing remain compiled logic.</summary>
    public CeresFlightActorPlacements Actors { get; }
}
