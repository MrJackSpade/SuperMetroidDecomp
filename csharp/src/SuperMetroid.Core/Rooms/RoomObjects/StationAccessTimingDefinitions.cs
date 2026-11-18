using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The authored timing of a map, energy or missile station's access arm (#1554), and its use as an animation (#2480): the
/// arm shows retracted, extends and holds, the station activates, the arm holds, sounds as it disengages, retracts and
/// holds again before the access ends.
/// </summary>
internal static class StationAccessTimingDefinitions
{
    /// <summary>The updates the arm takes to extend or retract, and each hold around the station's activation.</summary>
    internal const int MovementUpdates = 6;

    /// <summary>The updates the fully extended arm holds before the station activates.</summary>
    internal const int ExtendedHoldUpdates = 96;

    /// <summary>The arm's use, from the access's first draw to its end.</summary>
    internal static AnimationSegments<StationAccessSegment, StationAccessPose, StationAccessCue> Segments { get; } = new(
        new Dictionary<StationAccessSegment, AnimationBeat<StationAccessPose, StationAccessCue>[]>
        {
            [StationAccessSegment.Use] =
            [
                new AnimationBeat<StationAccessPose, StationAccessCue>.Frame(StationAccessPose.Retracted, MovementUpdates),
                new AnimationBeat<StationAccessPose, StationAccessCue>.Frame(StationAccessPose.Extended, ExtendedHoldUpdates),
                new AnimationBeat<StationAccessPose, StationAccessCue>.Event(StationAccessCue.Activates),
                new AnimationBeat<StationAccessPose, StationAccessCue>.Pause(MovementUpdates),
                new AnimationBeat<StationAccessPose, StationAccessCue>.Event(StationAccessCue.Disengages),
                new AnimationBeat<StationAccessPose, StationAccessCue>.Pause(MovementUpdates),
                new AnimationBeat<StationAccessPose, StationAccessCue>.Frame(StationAccessPose.Retracted, MovementUpdates),
            ],
        });

    /// <summary>The arm's use, played once; the access ends on the update after its last hold.</summary>
    internal static Animation<StationAccessSegment> Use { get; } = new([Segments.Whole(StationAccessSegment.Use)], AnimationEnd.Stops);
}
