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

    /// <summary>The way the elevator travels.</summary>
    ElevatorTravelDirection ElevatorTravel { get; }

    /// <summary>Whether the elevator, not Samus's own movement, moves Samus this frame.</summary>
    bool ElevatorRunsSamusMovement { get; }

    /// <summary>The elevator that placed Samus as the room loaded for an arriving ride, while that arrival is beginning.</summary>
    RoomEnemyPlace? ArrivalElevator { get; }

    /// <summary>Publishes the elevator's pseudo-door collision.</summary>
    void PublishElevatorDoorContact();

    /// <summary>Gameplay resumes from the pause, which disarms the pseudo-door (<see cref="Compatibility.UnpauseDisarmsElevator"/>).</summary>
    void ResumeFromPause();

    /// <summary>Prepares the destination elevator before the room load.</summary>
    void PrepareElevatorArrival();

    /// <summary>Returns the journey to rest: status inactive and flags clear, as gameplay does at fixed points.</summary>
    void ResetStatus();

}
