using SuperMetroid.Core.Animation;
using S = SuperMetroid.Core.Rooms.Doors.DoorCapSegment;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The coloured caps' authored data (#1554): how many hits each colour takes and each colour and facing's animations
/// (<see cref="DoorCapSequenceDefinitions"/>). A Super Missile opens any cap it may hit at once, whatever hits the cap has
/// taken (#1907). Every cap plays the same shape; the yellow downward cap waits on its closed image longer, the yellow left
/// cap opens fast and lingers open, and the red cap sounds each hit.
/// </summary>
internal static class ColoredDoorDefinitions
{
    /// <summary>The hits each colour's cap takes before it opens.</summary>
    internal static IReadOnlyDictionary<ColoredDoorColor, int> HitThresholds { get; } = new Dictionary<ColoredDoorColor, int>
    {
        [ColoredDoorColor.Yellow] = 1,
        [ColoredDoorColor.Green] = 1,
        [ColoredDoorColor.Red] = 5,
    };

    /// <summary>The yellow downward cap's closed image as it starts to wait, for three updates.</summary>
    private static readonly Animation<DoorCapSegment> YellowDownWaiting = DoorCapSequenceDefinitions.Once(S.YellowDownClosed);

    /// <summary>A hit on a red cap short of opening: its sound, then the shared flash three times.</summary>
    private static readonly Animation<DoorCapSegment> RedFlash = new(
        [
            DoorCapSequenceDefinitions.Segments.Whole(S.RedHitSounding),
            DoorCapSequenceDefinitions.Segments.Repeated(S.Flash, DoorCapSequenceDefinitions.FlashesPerHit),
        ],
        AnimationEnd.Stops);

    /// <summary>A cap opening, after which it leaves the room.</summary>
    private static readonly Animation<DoorCapSegment> Opening = DoorCapSequenceDefinitions.Once(S.Opening);

    /// <summary>The yellow left cap opening, after which it leaves the room.</summary>
    private static readonly Animation<DoorCapSegment> YellowLeftOpening = DoorCapSequenceDefinitions.Once(S.YellowLeftOpening);

    /// <summary>Each colour and facing's animations.</summary>
    internal static IReadOnlyDictionary<ColoredDoorVariant, ColoredDoorSequences> Sequences { get; } =
        new Dictionary<ColoredDoorVariant, ColoredDoorSequences>
    {
        [new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left)] = new(DoorCapSequenceDefinitions.Closed, DoorCapSequenceDefinitions.Flash, YellowLeftOpening),
        [new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right)] = new(DoorCapSequenceDefinitions.Closed, DoorCapSequenceDefinitions.Flash, Opening),
        [new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up)] = new(DoorCapSequenceDefinitions.Closed, DoorCapSequenceDefinitions.Flash, Opening),
        [new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down)] = new(YellowDownWaiting, DoorCapSequenceDefinitions.Flash, Opening),
        [new(ColoredDoorColor.Green, ColoredDoorOrientation.Left)] = new(DoorCapSequenceDefinitions.Closed, DoorCapSequenceDefinitions.Flash, Opening),
        [new(ColoredDoorColor.Green, ColoredDoorOrientation.Right)] = new(DoorCapSequenceDefinitions.Closed, DoorCapSequenceDefinitions.Flash, Opening),
        [new(ColoredDoorColor.Green, ColoredDoorOrientation.Up)] = new(DoorCapSequenceDefinitions.Closed, DoorCapSequenceDefinitions.Flash, Opening),
        [new(ColoredDoorColor.Green, ColoredDoorOrientation.Down)] = new(DoorCapSequenceDefinitions.Closed, DoorCapSequenceDefinitions.Flash, Opening),
        [new(ColoredDoorColor.Red, ColoredDoorOrientation.Left)] = new(DoorCapSequenceDefinitions.Closed, RedFlash, Opening),
        [new(ColoredDoorColor.Red, ColoredDoorOrientation.Right)] = new(DoorCapSequenceDefinitions.Closed, RedFlash, Opening),
        [new(ColoredDoorColor.Red, ColoredDoorOrientation.Up)] = new(DoorCapSequenceDefinitions.Closed, RedFlash, Opening),
        [new(ColoredDoorColor.Red, ColoredDoorOrientation.Down)] = new(DoorCapSequenceDefinitions.Closed, RedFlash, Opening),
    };
}
