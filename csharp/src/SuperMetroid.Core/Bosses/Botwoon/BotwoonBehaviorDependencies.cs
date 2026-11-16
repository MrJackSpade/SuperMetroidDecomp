using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>The collaborators a <see cref="BotwoonBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed record BotwoonBehaviorDependencies
{
    /// <summary>The room Botwoon fights in: its random numbers and whether Botwoon is already defeated.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>The enemy projectiles Botwoon's body segments and spit are, and the explosion its death raises.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>The room's effect actors, which show the explosions that break Botwoon's wall.</summary>
    public required RoomEffectActors EffectActors { get; init; }

    /// <summary>The enemies' ordinary touch, which Botwoon's touch runs.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>The enemies' death drops, which Botwoon's death leaves.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>The boss's results for the frame, which carry the change to Botwoon's wall.</summary>
    public required BossFrameResultBuilder FrameResults { get; init; }

    /// <summary>The enemy update's sound and music requests.</summary>
    public required EnemyAudioQueue Audio { get; init; }
}
