namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What an eye door spawns from the shared enemy-projectile pool. The values are arbitrary (#1414); the enemy side
/// owns each effect's placement, launch and animation.
/// </summary>
public enum EyeDoorProjectileDefinition
{
    /// <summary>The falling sweat actor, launched toward the side the eye faces, with a floor-impact animation.</summary>
    Sweat,
    /// <summary>One of the randomly placed smoke puffs of the shared room-object dust actor, scattered in a box around the eye.</summary>
    ScatteredSmoke,
    /// <summary>The smoke puff of the shared room-object dust actor centred on the eye.</summary>
    CenteredSmoke,
}
