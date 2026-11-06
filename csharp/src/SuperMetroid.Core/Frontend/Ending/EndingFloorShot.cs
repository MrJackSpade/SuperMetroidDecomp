namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// What the ending's turning floor shows (#1467, #1515): each escape and explosion scene, the gunship flying away, and the
/// post-credits reward icon, each one picture of the whole floor the import composes once; or nothing, in a phase that
/// shows no floor.
/// </summary>
public enum EndingFloorShot
{
    /// <summary>The phase shows no floor.</summary>
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
