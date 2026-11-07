namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What the eye door's PLM programs ask of the eye-door family.</summary>
internal enum EyeDoorObjectCue
{
    /// <summary>An opened door's eye continues at <see cref="EyeDoorObjectProgram.EyeOpened"/>.</summary>
    ConvertIfOpened,
    /// <summary>An opened door's component continues at <see cref="EyeDoorObjectProgram.ComponentDelete"/>.</summary>
    DeleteIfOpened,
    /// <summary>Samus near the closed eye opens it.</summary>
    ActivateIfSamusNear,
    /// <summary>Samus right in front of the open eye closes it for a moment.</summary>
    CloseIfSamusInFront,
    /// <summary>Samus still near keeps the eye watching.</summary>
    WatchIfSamusNear,
    /// <summary>Samus near a still component makes it pulse.</summary>
    PulseIfSamusNear,
    /// <summary>Samus still near keeps the component pulsing.</summary>
    KeepPulsingIfSamusNear,
    /// <summary>Links the eye's hit for its missile wake.</summary>
    LinkHit,
    /// <summary>Links the component's deletion for its door-bit wake.</summary>
    LinkDelete,
    /// <summary>Installs the missile-hit frame behaviour: a missile or Super Missile wakes the eye.</summary>
    ArmMissileWake,
    /// <summary>Installs the door-bit frame behaviour: the eye's door bit wakes the component.</summary>
    ArmDoorBitWake,
    /// <summary>Fires an eye-door projectile.</summary>
    Shoot,
    /// <summary>The hit sound.</summary>
    QueueHitSound,
    /// <summary>Two randomly placed smoke puffs.</summary>
    SpawnTwoSmoke,
    /// <summary>Counts a hit; the third opens the door.</summary>
    CountHit,
    /// <summary>The eye sweats.</summary>
    SpawnSweat,
    /// <summary>The destroyed eye takes no more hits.</summary>
    ClearFrameBehavior,
    /// <summary>One centred smoke puff.</summary>
    SpawnSmoke,
    /// <summary>The eye moves up a block and lays the blue cap's collision, facing its own side.</summary>
    MakeBlue,
    /// <summary>Continues at the closed blue cap's script: the eye becomes the closed blue cap.</summary>
    BecomeBlueDoor,
}
