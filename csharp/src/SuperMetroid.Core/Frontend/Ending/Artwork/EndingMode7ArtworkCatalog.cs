using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Host-owned visual streams for the escape and planet-explosion scenes.</summary>
public sealed class EndingMode7ArtworkCatalog
{
    /// <summary>Zebes zoom-out backdrop used by the first escape scene.</summary>
    private readonly EndingMode7SceneArtwork escapeA;
    /// <summary>Grey-cloud backdrop used by the second escape scene.</summary>
    private readonly EndingMode7SceneArtwork escapeB;
    /// <summary>Large Zebes backdrop used during the planet explosion.</summary>
    private readonly EndingMode7SceneArtwork planetExplosion;

    /// <summary>Installs the three decoded backdrop pairs and the complete post-credits beam/icon transfer.</summary>
    /// <param name="escapeA">Zebes zoom-out map and character lanes for the first escape scene.</param>
    /// <param name="escapeB">Grey-cloud map and character lanes for the second escape scene.</param>
    /// <param name="planetExplosion">Large-Zebes map and character lanes for the explosion scene.</param>
    /// <param name="rewardIcon">Interleaved map and characters progressively uploaded during the post-credits reward jump.</param>
    /// <exception cref="ArgumentNullException">Any artwork argument is null.</exception>
    public EndingMode7ArtworkCatalog(EndingMode7SceneArtwork escapeA,
        EndingMode7SceneArtwork escapeB, EndingMode7SceneArtwork planetExplosion,
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
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(EndingMode7ArtworkCatalog), content =>
    {
        content.Append("scene-count", 3);
        content.Append("map", escapeA.Map.Span);
        content.Append("characters", escapeA.Characters.Span);
        content.Append("map", escapeB.Map.Span);
        content.Append("characters", escapeB.Characters.Span);
        content.Append("map", planetExplosion.Map.Span);
        content.Append("characters", planetExplosion.Characters.Span);
        content.Append("reward-icon", RewardIcon.Transfer.Span);
    });

    /// <summary>Gets the map and character lanes for the selected native ending backdrop.</summary>
    /// <param name="id">The escape or planet-explosion scene whose artwork is required.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value does not identify one of the three supported scenes.</exception>
    public EndingMode7SceneArtwork this[EndingMode7SceneId id] => id switch
    {
        EndingMode7SceneId.EscapeA => escapeA,
        EndingMode7SceneId.EscapeB => escapeB,
        EndingMode7SceneId.PlanetExplosion => planetExplosion,
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };
}
