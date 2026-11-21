using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.World;

/// <summary>
/// The elevator under a forward-facing Samus on one update (#2553): whether it carries her and what the door blocks it
/// carries her through do to her.
/// </summary>
/// <param name="Moving">Whether an elevator carries her this update.</param>
/// <param name="DoorBlocks">What the door blocks the elevator carries her through do to her (#2545).</param>
internal readonly record struct SamusElevatorFooting(bool Moving, DoorBlockMeeting DoorBlocks)
{
    /// <summary>The footing an elevator of <paramref name="status"/> gives her.</summary>
    internal static SamusElevatorFooting Of(ElevatorActorStatus status) =>
        new(status != ElevatorActorStatus.Inactive, ElevatorDoorBlocks.FacingForward(status));
}
