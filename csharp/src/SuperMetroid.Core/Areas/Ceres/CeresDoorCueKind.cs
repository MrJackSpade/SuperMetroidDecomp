namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// What a cue of the Ceres door's animations signals (#2189): a door appearing, standing open or standing closed while it
/// waits on Samus, the door of Ridley's room waiting on Ceres Ridley, the elevator room's wall waiting on his escape, or the
/// door being shown or hidden, handed to Ridley's scene, switched in its collision with Samus or opening with its sound.
/// Each owner, <see cref="CeresDoorBehavior"/>, starts the animation a waiting door's or wall's event means.
/// </summary>
internal enum CeresDoorCueKind
{
    /// <summary>A right-facing door appears: unless Samus is near, its owner starts it standing closed.</summary>
    RightDoorAppears,
    /// <summary>A left-facing door appears: unless Samus is near, its owner starts it standing closed.</summary>
    LeftDoorAppears,
    /// <summary>A right-facing door stands open: once Samus is away, its owner starts it closing.</summary>
    RightDoorStandsOpen,
    /// <summary>A left-facing door stands open: once Samus is away, its owner starts it closing.</summary>
    LeftDoorStandsOpen,
    /// <summary>A right-facing door stands closed: while Samus is away, its owner starts it standing closed again.</summary>
    RightDoorStandsClosed,
    /// <summary>A left-facing door stands closed: while Samus is away, its owner starts it standing closed again.</summary>
    LeftDoorStandsClosed,
    /// <summary>The door of Ridley's room waits on him: while the area boss is alive, its owner starts the wait again.</summary>
    RidleyRoomDoorWaits,
    /// <summary>The elevator room's wall appears: until Ceres Ridley has escaped, its owner starts it as a left-facing door.</summary>
    ElevatorRoomWallAppears,
    /// <summary>The door of Ridley's room shows its fixed overlay picture instead of its actor frames (#2573).</summary>
    ShowOverlay,
    /// <summary>Stops colliding with Samus.</summary>
    MakeIntangible,
    /// <summary>Hides the door.</summary>
    MakeInvisible,
    /// <summary>Collides with Samus again.</summary>
    MakeTangible,
    /// <summary>Shows the door.</summary>
    MakeVisible,
    /// <summary>Shows the door, its actor frames again in place of its overlay picture.</summary>
    MakeVisibleAndShowActor,
    /// <summary>Queues the door's opening sound.</summary>
    PlayOpeningSound,
}
