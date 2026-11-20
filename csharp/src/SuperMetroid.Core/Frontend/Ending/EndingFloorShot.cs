namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// What the ending's turning floor shows (#1467, #1515): each escape and explosion scene, the gunship flying away, and the
/// post-credits reward icon, each one picture of the whole floor the import composes once. A phase that shows no floor
/// has no shot.
/// </summary>
public enum EndingFloorShot
{
    /// <summary>The escape through the side clouds, over the exploding planet's lava and crust.</summary>
    SideCloudEscape,
    /// <summary>The escape between the top and bottom clouds, through the grey clouds.</summary>
    TopAndBottomCloudEscape,
    /// <summary>Zebes exploding.</summary>
    PlanetExplosion,
    /// <summary>The gunship flying away from the explosion.</summary>
    Flyaway,
    /// <summary>The post-credits reward icon.</summary>
    RewardIcon,
}
