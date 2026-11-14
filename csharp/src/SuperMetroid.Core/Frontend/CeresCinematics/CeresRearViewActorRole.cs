namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>What each actor of the Ceres approach's rear view shows (#1768): its definition and installed placement key.</summary>
public enum CeresRearViewActorRole
{
    /// <summary>The large asteroids drifting past the station.</summary>
    LargeAsteroids,

    /// <summary>The space colony under attack.</summary>
    StationUnderAttack,

    /// <summary>The small asteroids drifting past the station.</summary>
    SmallAsteroids,

    /// <summary>The purple space vortex sliding behind the station.</summary>
    Vortex,

    /// <summary>The star field behind the rear view.</summary>
    RearStars,
}
