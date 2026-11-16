using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The collaborators a <see cref="MotherBrainBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
[ArchitectureReview("wide-dependencies", "The composition seam for Mother Brain's parts (body, head, tubes, projectiles, fake death, ascent, rainbow and death, attacks, combat), each built from a different subset of these collaborators; #2218 added the room's places, among which her head finds her body.")]
[RoomLifetime]
internal sealed record MotherBrainBehaviorDependencies
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    public required BossFrameResultBuilder FrameResults { get; init; }

    /// <summary>Sets up enemies in their places, spawns them and runs their AI.</summary>
    public required IEnemyLifecycle Lifecycle { get; init; }
    /// <summary>The room's enemy places, in which her head finds her body listed right before it (#2218).</summary>
    public required EnemyPlaces Places { get; init; }

    /// <summary>Room.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Earthquake.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>Audio.</summary>
    public required EnemyAudioQueue Audio { get; init; }


    /// <summary>What the boss draws with: the room\'s presentation surfaces and the installed boss artwork.</summary>
    public required BossPresentation Presentation { get; init; }

    /// <summary>Frame context.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>Collision.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>Deaths and drops.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>Mother brain encounter.</summary>
    public required MotherBrainEncounter MotherBrainEncounter { get; init; }

    /// <summary>The other actors in the room that Mother Brain asks to spawn, fire or stop.</summary>
    public required IMotherBrainRoomActors RoomActors { get; init; }


}
