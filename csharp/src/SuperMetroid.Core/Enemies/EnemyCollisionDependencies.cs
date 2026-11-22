using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>The collaborators a <see cref="EnemyCollision"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed partial record EnemyCollisionDependencies
{
    /// <summary>Sets up enemies in their places, spawns them and runs their AI.</summary>
    public required IEnemyLifecycle Lifecycle { get; init; }

    /// <summary>Queues the enemy sounds hits, freezes and deaths make.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>Runs the death and drop of an enemy a collision kills.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>The room's earthquake, which a hit that shakes the room starts.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>The room's enemy projectiles, which collision outcomes reach.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>The room's effect actors, which the burst at an enemy a shot or bomb hurts joins.</summary>
    public required RoomEffectActors EffectActors { get; init; }

    /// <summary>The room's enemy places.</summary>
    public required EnemyPlaces Places { get; init; }

    /// <summary>The power bomb's sweep radius after a kill, shared with the deaths that record kills.</summary>
    public required PowerBombRadiusAfterKill PowerBombRadius { get; init; }
}
