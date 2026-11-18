using SuperMetroid.Core.Animation;
using S = SuperMetroid.Core.Rooms.RoomObjects.DownwardGateSegment;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The downward gate's animations (#2480): its authored images (<see cref="DownwardGateMovementDefinitions"/>), each shown
/// for its own updates, played once through on the shared animation player.
/// </summary>
internal static class DownwardGateAnimations
{
    /// <summary>Each run's images.</summary>
    internal static AnimationSegments<DownwardGateSegment, DownwardGateImage, NoAnimationCue> Segments { get; } = new(
        new Dictionary<DownwardGateSegment, AnimationBeat<DownwardGateImage, NoAnimationCue>[]>
        {
            [S.Closing] = Shown(DownwardGateMovementDefinitions.Closing),
            [S.Reopening] = Shown(DownwardGateMovementDefinitions.Reopening),
            [S.Closed] = Shown([DownwardGateMovementDefinitions.Closed]),
            [S.Open] = Shown([DownwardGateMovementDefinitions.Open]),
        });

    /// <summary>The gate closing: the open gate holding, then lowering.</summary>
    internal static Animation<DownwardGateSegment> Closing { get; } = Once(S.Closing);

    /// <summary>The gate reopening.</summary>
    internal static Animation<DownwardGateSegment> Reopening { get; } = Once(S.Reopening);

    /// <summary>The closed gate, before the gate rests closed.</summary>
    internal static Animation<DownwardGateSegment> Closed { get; } = Once(S.Closed);

    /// <summary>The open gate, before the gate rests open.</summary>
    internal static Animation<DownwardGateSegment> Open { get; } = Once(S.Open);

    /// <summary>The whole of <paramref name="segment"/>, once.</summary>
    private static Animation<DownwardGateSegment> Once(DownwardGateSegment segment) => new([Segments.Whole(segment)], AnimationEnd.Stops);

    /// <summary>Each of <paramref name="images"/>, shown for its own updates.</summary>
    private static AnimationBeat<DownwardGateImage, NoAnimationCue>[] Shown(IReadOnlyList<DownwardGateImage> images) =>
        [.. images.Select(image => new AnimationBeat<DownwardGateImage, NoAnimationCue>.Frame(image, image.Updates))];
}
