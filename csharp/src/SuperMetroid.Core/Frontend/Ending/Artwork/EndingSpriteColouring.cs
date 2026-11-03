namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The colours an ending actor's palette-inheriting parts draw in (#1512), named for what the ending shows in them.
/// Which of the ending's colours each one is, the import alone knows.
/// </summary>
public enum EndingSpriteColouring
{
    /// <summary>The clear time's digits and colon.</summary>
    ClearTimeDigits,

    /// <summary>The clear time's label.</summary>
    ClearTimeLabel,

    /// <summary>THE OPERATION WAS and COMPLETED SUCCESSFULLY.</summary>
    CompletionText,

    /// <summary>The escape's clouds.</summary>
    Clouds,

    /// <summary>The exploding planet's lava and silhouette, which the explosion's crossfade brings in.</summary>
    ExplosionLava,

    /// <summary>The exploding planet, its glow and its stars, which the explosion's crossfade brings in.</summary>
    ExplodingPlanet,

    /// <summary>The planet's afterglow.</summary>
    ExplosionAfterglow,

    /// <summary>The animals' escape pod.</summary>
    AnimalEscape,

    /// <summary>The post-credits logo, which its crossfade recolours.</summary>
    Logo,

    /// <summary>The suitless Samus and the helmetless head, which the reward brings in.</summary>
    SuitlessSamus,

    /// <summary>The suited Samus and the helmeted head, which the helmetless reward brings in.</summary>
    SuitedSamus,

    /// <summary>Samus falling toward the camera.</summary>
    FallingSamus,

    /// <summary>Samus shooting, whom the post-shot fades out.</summary>
    ShootingSamus,

    /// <summary>The post-credits shooting stars, which the post-shot fades out.</summary>
    ShootingStars,
}
