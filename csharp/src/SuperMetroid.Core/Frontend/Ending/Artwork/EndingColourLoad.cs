namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The installed colours the ending's scenes show as they load them (#1512, #1544). A scene's colours are its own: each
/// scene starts in its whole screen of colours, and nothing an earlier scene showed shows in it. The other loads change
/// some of the running scene's own colours. Which colours each load writes, the import alone knows.
/// </summary>
public enum EndingColourLoad
{
    /// <summary>The escape scenes' colours.</summary>
    Escape,

    /// <summary>The planet explosion's colours, which the flyaway keeps.</summary>
    Explosion,

    /// <summary>The explosion's background colours, restored over its palette effects as the explosion's hold begins.</summary>
    ExplosionBackgrounds,

    /// <summary>The final gunship's colours, shown under the operation text.</summary>
    FinalGunship,

    /// <summary>The staff credits' colours, shown at the staff handoff.</summary>
    Credits,

    /// <summary>The post-credits scenes' colours, shown after the credits.</summary>
    PostCredits,

    /// <summary>The post-credits shot's colours, restored as the white flash begins.</summary>
    RestoredShot,

    /// <summary>The assembling logo's first colours.</summary>
    LogoInitial,
}
