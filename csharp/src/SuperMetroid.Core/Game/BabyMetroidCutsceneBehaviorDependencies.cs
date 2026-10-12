using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Game;

/// <summary>The collaborators a <see cref="BabyMetroidCutsceneBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record BabyMetroidCutsceneBehaviorDependencies
{
    /// <summary>Dispatch.</summary>
    public required IEnemyDispatch Dispatch { get; init; }

    /// <summary>Room.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Audio.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>Slot table.</summary>
    public required EnemySlotTable SlotTable { get; init; }

    /// <summary>Presentation.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>Frame context.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>Projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Mother brain encounter.</summary>
    public required MotherBrainEncounter MotherBrainEncounter { get; init; }
}
