using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// An elevator platform (#1554): from its first update it plays its loop
/// (<see cref="ElevatorPlatformObjectDefinitions.Cycle"/>) for as long as the room holds it.
/// </summary>
internal sealed partial class ElevatorPlatformObject()
    : SilentSequencedRoomObject<RoomObjectFrameRun, ElevatorPlatformFrame>(CycleSegments)
{
    /// <summary>The platform's loop, each layout for its own updates (#2480).</summary>
    private static readonly AnimationSegments<RoomObjectFrameRun, ElevatorPlatformFrame, NoAnimationCue> CycleSegments =
        RoomObjectFrameRuns.Of(ElevatorPlatformObjectDefinitions.Cycle, frame => frame.Updates);

    /// <summary>The platform's loop, played again and again.</summary>
    private static readonly Animation<RoomObjectFrameRun> Cycling =
        Animation<RoomObjectFrameRun>.Looping([CycleSegments.Whole(RoomObjectFrameRun.Frames)]);

    /// <inheritdoc/>
    private protected override void Show(ElevatorPlatformFrame frame, RoomObjectPlace place, RoomObjectStepContext context) =>
        context.Draw(place, frame.Draw);

    /// <inheritdoc/>
    /// <remarks>The platform's empty first animation ends on its first update; its loop never ends.</remarks>
    private protected override void SequenceEnded(RoomObjectPlace place, RoomObjectStepContext context) =>
        Play(Cycling, place, context);
}
