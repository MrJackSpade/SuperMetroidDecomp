namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What an eye door spawns from the shared enemy-projectile pool. The values are arbitrary (#1414); the enemy side
/// owns each effect's placement, launch and animation.
/// </summary>
public enum EyeDoorProjectileDefinition : ushort
{
    /// <summary>The eye-door attack actor, whose initial animation later installs aimed movement and whose shot/impact lists terminate it.</summary>
    Projectile = 1,
    /// <summary>The falling sweat actor, launched toward the side the eye faces, with a floor-impact animation.</summary>
    Sweat = 2,
    /// <summary>One of the randomly placed smoke puffs of the shared PLM dust actor, scattered in a box around the eye.</summary>
    ScatteredSmoke = 3,
    /// <summary>The smoke puff of the shared PLM dust actor centred on the eye.</summary>
    CenteredSmoke = 4,
}
