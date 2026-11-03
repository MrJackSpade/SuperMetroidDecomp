namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The colour groups the ending fades by level (#1512): each shows its installed colours scaled by its level and rounded
/// down. Which colours each group holds, and which installed colours it scales, the import alone knows.
/// </summary>
public enum EndingFade
{
    /// <summary>The escape's far background, which the explosion's crossfade fades out.</summary>
    ExplosionBackground,

    /// <summary>The exploding planet's lava, which the explosion's crossfade fades in.</summary>
    ExplosionLava,

    /// <summary>The exploding planet, which the explosion's crossfade fades in.</summary>
    ExplodingPlanet,

    /// <summary>The post-credits waiting backdrop, which fades in and then out under the reward.</summary>
    WaitingBackdrop,

    /// <summary>The suitless Samus, whom the reward fades in.</summary>
    SuitlessSamus,

    /// <summary>The suited Samus, whom the helmetless reward fades in.</summary>
    SuitedSamus,

    /// <summary>The post-credits shot and shooting stars, which the post-shot fades out.</summary>
    Shot,

    /// <summary>Samus shooting, whom the post-shot fades out after its delay.</summary>
    ShootingSamus,
}
