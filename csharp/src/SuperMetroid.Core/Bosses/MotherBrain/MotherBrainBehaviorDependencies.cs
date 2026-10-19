using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The collaborators a <see cref="MotherBrainBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record MotherBrainBehaviorDependencies
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    public required EnemyFrameResultBuilder FrameResults { get; init; }

    /// <summary>Dispatch.</summary>
    public required IEnemyDispatch Dispatch { get; init; }

    /// <summary>Room.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Earthquake.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>Audio.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>Slot table.</summary>
    public required EnemySlotTable SlotTable { get; init; }

    /// <summary>Presentation.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>Frame context.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>Collision.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>Deaths and drops.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>Mother brain encounter.</summary>
    public required MotherBrainEncounter MotherBrainEncounter { get; init; }

    /// <summary>Baby metroid cutscene behavior.</summary>
    public required BabyMetroidCutsceneBehavior BabyMetroidCutsceneBehavior { get; init; }

    /// <summary>Rinka behavior.</summary>
    public required RinkaBehavior RinkaBehavior { get; init; }

    /// <summary>Walking space pirate behavior.</summary>
    public required WalkingSpacePirateBehavior WalkingSpacePirateBehavior { get; init; }

    /// <summary>Ridley projectiles.</summary>
    public required RidleyProjectiles RidleyProjectiles { get; init; }

    /// <summary>Room status.</summary>
    public required EnemyRoomStatus RoomStatus { get; init; }
}
