namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// What the ending's turning floor shows (#1467): each escape and explosion scene, the gunship flying away, and the
/// post-credits reward icon, each one picture of the whole floor the import composes once; or nothing, before any floor
/// has loaded or once the credits' character art has replaced the floor's characters.
/// </summary>
public enum EndingFloorShot
{
    /// <summary>No floor has loaded, or the credits' character art has replaced its characters.</summary>
    None,
    /// <summary>The first escape backdrop.</summary>
    EscapeA,
    /// <summary>The second escape backdrop.</summary>
    EscapeB,
    /// <summary>Zebes exploding.</summary>
    PlanetExplosion,
    /// <summary>The gunship flying away from the explosion.</summary>
    Flyaway,
    /// <summary>The post-credits reward icon.</summary>
    RewardIcon,
}
