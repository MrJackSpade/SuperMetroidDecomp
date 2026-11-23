using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The collaborators a <see cref="SporeSpawnBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed partial record SporeSpawnBehaviorDependencies
{
    /// <summary>The room Spore Spawn's fight is in: its area, its enemy definitions and the game services and random numbers its enemies use.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>The room's presentation, which shows his colours (#2194).</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>The enemy projectiles Spore Spawn's fight fires and spawns.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>The room's effect actors, which Spore Spawn's fight shows.</summary>
    public required RoomEffectActors EffectActors { get; init; }

    /// <summary>The enemy update's random number, frame counter and flags.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>The enemies' touches, shots and bombs with Samus, which Spore Spawn's fight reacts to.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>The enemies' death explosions and drops, which Spore Spawn's fight spawns.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>The enemy pass's results, which Spore Spawn's fight writes.</summary>
    public required EnemyPassEvents PassEvents { get; init; }

    /// <summary>The enemy update's sound and music requests.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>The room camera's floor, which Spore Spawn's live fight holds at his arena's top (#2243).</summary>
    public required RoomCameraFloor CameraFloor { get; init; }
}
