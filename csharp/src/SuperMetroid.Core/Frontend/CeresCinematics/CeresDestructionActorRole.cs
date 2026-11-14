namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// What each persistent actor behind Ceres's explosion shows (#1768): its definition and installed placement key.
/// </summary>
public enum CeresDestructionActorRole
{
    /// <summary>The large asteroids drifting behind the exploding station.</summary>
    LargeAsteroids,

    /// <summary>The small asteroids drifting behind the exploding station.</summary>
    SmallAsteroids,

    /// <summary>The purple space vortex, standing still behind the exploding station.</summary>
    Vortex,
}
