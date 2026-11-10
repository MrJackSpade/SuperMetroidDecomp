using P = SuperMetroid.Core.Enemies.FaceBlocks.BlueBrinstarFaceBlockProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>
/// The Blue Brinstar face block's neutral and turning animations as typed programs (#641). Each
/// frame shows a <see cref="BlueBrinstarFaceBlockFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class BlueBrinstarFaceBlockAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<BlueBrinstarFaceBlockProgram, BlueBrinstarFaceBlockFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<BlueBrinstarFaceBlockProgram, AnimationBeat<BlueBrinstarFaceBlockFrame, NoAnimationCue>[]>
    {
        [P.SamusLeft] =
        [
            Show(0, 48),
            Show(1, 16),
            Show(2, 16),
        ],
        [P.SamusRight] =
        [
            Show(0, 48),
            Show(3, 16),
            Show(4, 16),
        ],
        [P.Initial] =
        [
            Show(0, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<BlueBrinstarFaceBlockProgram, Animation<BlueBrinstarFaceBlockProgram>> StartingAt { get; } = new Dictionary<BlueBrinstarFaceBlockProgram, Animation<BlueBrinstarFaceBlockProgram>>
    {
        [P.SamusLeft] = new([new(P.SamusLeft, 0, 3, 1)], AnimationEnd.Holds),
        [P.SamusRight] = new([new(P.SamusRight, 0, 3, 1)], AnimationEnd.Holds),
        [P.Initial] = new([new(P.Initial, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BlueBrinstarFaceBlockProgram> StartingWith(BlueBrinstarFaceBlockProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<BlueBrinstarFaceBlockFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<BlueBrinstarFaceBlockFrame, NoAnimationCue>.Frame(new(pose), duration);
}
