using SuperMetroid.Core.Enemies.Elevator;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The elevator journey state the runtime reads and the operations it performs on it at fixed phase points. Reads are
/// live: input, movement and the door transition see the state after their own updates within the frame.
/// </summary>
internal interface IElevatorJourney
{
    /// <summary>The elevator's journey status.</summary>
    ElevatorActorStatus ElevatorStatus { get; }

    /// <summary>What has armed the elevator's pseudo-door.</summary>
    ElevatorJourneyFlag ElevatorFlags { get; }

    /// <summary>Whether the elevator travels upward.</summary>
    bool ElevatorTravelsUpward { get; }

    /// <summary>Whether the elevator, not Samus's handlers, moves Samus this frame.</summary>
    bool ElevatorRunsSamusMovement { get; }

    /// <summary>The door-transition gate that suppresses actor movement.</summary>
    bool ElevatorDoorTransitionActive { get; set; }

    /// <summary>Returns and clears the slot of the arriving elevator that placed Samus during the room load.</summary>
    int? ConsumeElevatorArrivalPlacementSlot();

    /// <summary>Publishes the elevator's pseudo-door collision.</summary>
    void PublishElevatorDoorContact();

    /// <summary>Clears the flags when gameplay resumes.</summary>
    void ClearElevatorFlagsOnGameplayResume();

    /// <summary>Prepares the destination elevator before the room load.</summary>
    void PrepareElevatorArrival(ElevatorJourneyFlag doorFlags = ElevatorJourneyFlag.DoorLoad);

    /// <summary>Returns the journey to rest: status inactive and flags clear, as gameplay does at fixed points.</summary>
    void ResetStatus();

}
