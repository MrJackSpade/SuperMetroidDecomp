using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The collaborators a <see cref="BabyMetroidCutsceneBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed partial record BabyMetroidCutsceneBehaviorDependencies
{
    /// <summary>Sets up enemies in their places, spawns them and runs their AI.</summary>
    public required IEnemyLifecycle Lifecycle { get; init; }

    /// <summary>The room the Baby Metroid's cutscene is in: its area, its enemy definitions and the game services and random numbers its enemies use.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>The enemy update's sound and music requests.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>The room's enemy places.</summary>
    public required EnemyPlaces Places { get; init; }

    /// <summary>The enemy update's random number, frame counter and flags.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>The enemy projectiles the Baby Metroid's cutscene fires and spawns.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>The Mother Brain encounter state shared across Tourian.</summary>
    public required MotherBrainEncounter MotherBrainEncounter { get; init; }
}
