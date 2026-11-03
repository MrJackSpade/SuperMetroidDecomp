namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The installed colours the ending's scenes show as they load them (#1512), each over the colours it replaces. Which
/// colours each load replaces, the import alone knows.
/// </summary>
public enum EndingColourLoad
{
    /// <summary>The escape's whole screen of colours, which replaces every colour the ending showed before.</summary>
    Escape,

    /// <summary>The explosion's sprite colours, loaded as the planet explosion begins.</summary>
    ExplosionSprites,

    /// <summary>The explosion's background colours, restored before the flyaway's floor loads.</summary>
    ExplosionBackgrounds,

    /// <summary>The final gunship's colours, shown under the operation text.</summary>
    FinalGunship,

    /// <summary>The credits' background colours, loaded at the staff handoff.</summary>
    CreditsBackgrounds,

    /// <summary>The post-credits colours, restored after the credits except the first four background colours.</summary>
    PostCredits,

    /// <summary>The post-credits shot's colours, restored as the white flash begins.</summary>
    RestoredShot,

    /// <summary>The assembling logo's first colours.</summary>
    LogoInitial,
}
