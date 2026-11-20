using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// The installed escape, planet-explosion and reward-icon artwork as the import pipeline reads it (#1409), which names its
/// content identity from each scene's map and characters and the reward icon's transfer. The import pictures the turning
/// floors' sheets from it; the game draws those sheets and never loads this artwork (#1555).
/// </summary>
public sealed class EndingFloorCharacters
{
    /// <summary>Zebes zoom-out backdrop used by the side-cloud escape.</summary>
    private readonly EndingFloorSceneArtwork sideCloudEscape;
    /// <summary>Grey-cloud backdrop used by the top-and-bottom-cloud escape.</summary>
    private readonly EndingFloorSceneArtwork topAndBottomCloudEscape;
    /// <summary>Large Zebes backdrop used during the planet explosion.</summary>
    private readonly EndingFloorSceneArtwork planetExplosion;

    /// <summary>Installs the three decoded backdrop pairs and the complete post-credits beam/icon transfer.</summary>
    /// <param name="sideCloudEscape">Zebes zoom-out map and character lanes for the side-cloud escape.</param>
    /// <param name="topAndBottomCloudEscape">Grey-cloud map and character lanes for the top-and-bottom-cloud escape.</param>
    /// <param name="planetExplosion">Large-Zebes map and character lanes for the explosion scene.</param>
    /// <param name="rewardIcon">Interleaved map and characters progressively uploaded during the post-credits reward jump.</param>
    /// <exception cref="ArgumentNullException">Any artwork argument is null.</exception>
    public EndingFloorCharacters(EndingFloorSceneArtwork sideCloudEscape, EndingFloorSceneArtwork topAndBottomCloudEscape,
        EndingFloorSceneArtwork planetExplosion, EndingRewardIconArtwork rewardIcon)
    {
        this.sideCloudEscape = Ensure.NotNull(sideCloudEscape);
        this.topAndBottomCloudEscape = Ensure.NotNull(topAndBottomCloudEscape);
        this.planetExplosion = Ensure.NotNull(planetExplosion);
        RewardIcon = Ensure.NotNull(rewardIcon);
    }

    /// <summary>Gets the map and character lanes for the selected ending backdrop.</summary>
    /// <param name="id">The escape or planet-explosion scene whose artwork is required.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value does not identify one of the three supported scenes.</exception>
    public EndingFloorSceneArtwork this[EndingFloorSceneId id] => id switch
    {
        EndingFloorSceneId.SideCloudEscape => sideCloudEscape,
        EndingFloorSceneId.TopAndBottomCloudEscape => topAndBottomCloudEscape,
        EndingFloorSceneId.PlanetExplosion => planetExplosion,
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    /// <summary>Interleaved map/character sheet transferred during the reward landing.</summary>
    public EndingRewardIconArtwork RewardIcon { get; }
}
