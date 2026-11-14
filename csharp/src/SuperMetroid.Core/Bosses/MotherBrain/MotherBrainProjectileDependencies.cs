using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The collaborators Mother Brain's projectile behaviour is constructed with, built once by
/// <see cref="MotherBrainProjectiles"/>, which creates it (#1801).
/// </summary>
[RoomLifetime]
internal sealed record MotherBrainProjectileDependencies
{
    /// <summary>The loaded room, whose random numbers scatter the glass and the hand beams and time the turrets.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>The enemy projectiles, which hold what Mother Brain's projectiles spawn and raise their dust.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>The room's earthquake, which an exploding ring or a hand beam leaving the arena starts.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>The drops a bomb a Samus bomb destroys leaves.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>Mother Brain's fight state: her head and body, the Baby, her bombs and her sounds.</summary>
    public required MotherBrainEncounter Encounter { get; init; }

    /// <summary>The room's other actors: the afterburn an expiring bomb leaves.</summary>
    public required IMotherBrainRoomActors RoomActors { get; init; }
}
