namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What the eye door's PLM programs ask of the eye-door family.</summary>
internal enum EyeDoorPlmCue
{
    /// <summary><c>$84:8A72</c>: an opened door's eye continues at <see cref="EyeDoorPlmProgram.EyeOpened"/>.</summary>
    ConvertIfOpened,
    /// <summary><c>$84:8A72</c>: an opened door's component continues at <see cref="EyeDoorPlmProgram.ComponentDelete"/>.</summary>
    DeleteIfOpened,
    /// <summary><c>$84:8D41</c>: Samus near the closed eye opens it.</summary>
    ActivateIfSamusNear,
    /// <summary><c>$84:8D41</c>: Samus right in front of the open eye closes it for a moment.</summary>
    CloseIfSamusInFront,
    /// <summary><c>$84:8D41</c>: Samus still near keeps the eye watching.</summary>
    WatchIfSamusNear,
    /// <summary><c>$84:8D41</c>: Samus near a still component makes it pulse.</summary>
    PulseIfSamusNear,
    /// <summary><c>$84:8D41</c>: Samus still near keeps the component pulsing.</summary>
    KeepPulsingIfSamusNear,
    /// <summary><c>$84:8A24</c>: links the eye's hit for its missile wake.</summary>
    LinkHit,
    /// <summary><c>$84:8A24</c>: links the component's deletion for its door-bit wake.</summary>
    LinkDelete,
    /// <summary><c>$84:86C1</c> with <c>$84:BD50</c>: a missile or Super Missile wakes the eye.</summary>
    ArmMissileWake,
    /// <summary><c>$84:86C1</c> with <c>$84:D753</c>: the eye's door bit wakes the component.</summary>
    ArmDoorBitWake,
    /// <summary><c>$84:D77A</c>: fires an eye-door projectile.</summary>
    Shoot,
    /// <summary><c>$84:8C10</c>: the hit sound.</summary>
    QueueHitSound,
    /// <summary><c>$84:D79F</c>: two randomly placed smoke puffs.</summary>
    SpawnTwoSmoke,
    /// <summary><c>$84:8A91</c>: counts a hit; the third opens the door.</summary>
    CountHit,
    /// <summary><c>$84:D790</c>: the eye sweats.</summary>
    SpawnSweat,
    /// <summary><c>$84:86CA</c>: the destroyed eye takes no more hits.</summary>
    ClearPreInstruction,
    /// <summary><c>$84:D7B6</c>: one centred smoke puff.</summary>
    SpawnSmoke,
    /// <summary><c>$84:D7C3</c> (right) / <c>$84:D7DA</c> (left): the eye moves up a block and lays the blue cap's collision.</summary>
    MakeBlue,
    /// <summary><c>$84:8724</c> into <c>InstList_PLM_ClosedBlueDoorFacing*</c>: the eye becomes the closed blue cap.</summary>
    BecomeBlueDoor,
}
