using SuperMetroid.Core.Animation;
using S = SuperMetroid.Core.Rooms.RoomObjects.NoobTubeSegment;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Maridia's glass tube's animations (#2480): its authored images (<see cref="NoobTubeObjectDefinitions"/>), each shown for
/// its own updates, played once through on the shared animation player.
/// </summary>
internal static partial class NoobTubeAnimations
{
    /// <summary>Each run's images.</summary>
    internal static AnimationSegments<NoobTubeSegment, TerrainChangeFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<NoobTubeSegment, AnimationBeat<TerrainChangeFrame, NoAnimationCue>[]>
        {
            [S.Intact] = [Show(new(NoobTubeObjectDraws.Family.Of(NoobTubeObjectDefinitions.IntactImage), 1))],
            [S.Cracking] = [.. NoobTubeObjectDefinitions.Cracking.Select(Show)],
            [S.Shattered] = [Show(NoobTubeObjectDefinitions.Shattered)],
        });

    /// <summary>The intact tube.</summary>
    internal static Animation<NoobTubeSegment> Intact { get; } = Once(S.Intact);

    /// <summary>The tube cracking.</summary>
    internal static Animation<NoobTubeSegment> Cracking { get; } = Once(S.Cracking);

    /// <summary>The shattered, open tube.</summary>
    internal static Animation<NoobTubeSegment> Shattered { get; } = Once(S.Shattered);

    /// <summary>The whole of <paramref name="segment"/>, once.</summary>
    private static Animation<NoobTubeSegment> Once(NoobTubeSegment segment) => new([Segments.Whole(segment)], AnimationEnd.Stops);

    /// <summary>Shows <paramref name="frame"/> for its own updates.</summary>
    private static AnimationBeat<TerrainChangeFrame, NoAnimationCue> Show(TerrainChangeFrame frame) =>
        new AnimationBeat<TerrainChangeFrame, NoAnimationCue>.Frame(frame, frame.Updates);
}
