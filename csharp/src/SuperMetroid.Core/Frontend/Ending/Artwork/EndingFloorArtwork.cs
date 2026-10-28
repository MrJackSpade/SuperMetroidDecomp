namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// Host-owned visual streams for the escape and planet-explosion scenes (#1409): their maps and character counts. The
/// import pipeline that reads the installed artwork names the content's identity.
/// </summary>
public abstract class EndingFloorArtwork
{
    /// <summary>Zebes zoom-out backdrop used by the first escape scene.</summary>
    private readonly EndingFloorSceneArtwork escapeA;
    /// <summary>Grey-cloud backdrop used by the second escape scene.</summary>
    private readonly EndingFloorSceneArtwork escapeB;
    /// <summary>Large Zebes backdrop used during the planet explosion.</summary>
    private readonly EndingFloorSceneArtwork planetExplosion;

    /// <summary>Installs the three decoded backdrop pairs and the complete post-credits beam/icon transfer.</summary>
    /// <param name="escapeA">Zebes zoom-out map and character lanes for the first escape scene.</param>
    /// <param name="escapeB">Grey-cloud map and character lanes for the second escape scene.</param>
    /// <param name="planetExplosion">Large-Zebes map and character lanes for the explosion scene.</param>
    /// <param name="rewardIcon">Interleaved map and characters progressively uploaded during the post-credits reward jump.</param>
    /// <exception cref="ArgumentNullException">Any artwork argument is null.</exception>
    internal EndingFloorArtwork(EndingFloorSceneArtwork escapeA,
        EndingFloorSceneArtwork escapeB, EndingFloorSceneArtwork planetExplosion,
        EndingRewardIconArtwork rewardIcon)
    {
        this.escapeA = Ensure.NotNull(escapeA);
        this.escapeB = Ensure.NotNull(escapeB);
        this.planetExplosion = Ensure.NotNull(planetExplosion);
        RewardIcon = Ensure.NotNull(rewardIcon);
    }

    /// <summary>Interleaved map/character sheet transferred during the reward landing.</summary>
    public EndingRewardIconArtwork RewardIcon { get; }

    /// <summary>Identity of each decoded scene's two transfer lanes and the full reward-icon transfer.</summary>
    public abstract string ContentIdentity { get; }

    /// <summary>Gets the map and character lanes for the selected native ending backdrop.</summary>
    /// <param name="id">The escape or planet-explosion scene whose artwork is required.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value does not identify one of the three supported scenes.</exception>
    public EndingFloorSceneArtwork this[EndingFloorSceneId id] => id switch
    {
        EndingFloorSceneId.EscapeA => escapeA,
        EndingFloorSceneId.EscapeB => escapeB,
        EndingFloorSceneId.PlanetExplosion => planetExplosion,
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };
}
