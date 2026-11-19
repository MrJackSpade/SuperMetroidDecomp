using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.World;

/// <summary>What the door blocks of an elevator's shaft do to the Samus it carries (#2545).</summary>
internal static class ElevatorDoorBlocks
{
    /// <summary>
    /// What the door blocks a front-facing Samus meets do to her while the elevator in <paramref name="status"/> carries
    /// her. Gameplay resumes as soon as the destination loads, so an arriving elevator carries her past its shaft's door
    /// blocks while main gameplay runs. Her movement keeps scanning them, but only a departing elevator's ride contacts
    /// them, entering a door or standing at the elevator's door; otherwise the arrival would depart again at once.
    /// </summary>
    /// <param name="status">The room's elevator's status.</param>
    internal static DoorBlockMeeting FacingForward(ElevatorActorStatus status) =>
        status == ElevatorActorStatus.Departing ? DoorBlockMeeting.Contacts : DoorBlockMeeting.CarriedPastByElevator;
}
