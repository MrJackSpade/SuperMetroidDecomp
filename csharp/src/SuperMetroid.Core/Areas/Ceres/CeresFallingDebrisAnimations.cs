using P = SuperMetroid.Core.Areas.Ceres.CeresFallingDebrisSegment;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres falling tiles' typed animations (#641): one pose, then a sleep while the
/// per-frame movement moves the tile. Each animation's frame is the animation itself.
/// </summary>
internal static class CeresFallingDebrisAnimations
{
    /// <summary>Both tile animations.</summary>
    internal static AnimationSegments<CeresFallingDebrisSegment, CeresFallingDebrisSegment, NoAnimationCue> Segments { get; } = new(
        new Dictionary<CeresFallingDebrisSegment, AnimationBeat<CeresFallingDebrisSegment, NoAnimationCue>[]>
    {
        [P.Light] =
        [
            Show(P.Light, 1),
        ],
        [P.Dark] =
        [
            Show(P.Dark, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<CeresFallingDebrisSegment, Animation<CeresFallingDebrisSegment>> StartingAt { get; } = new Dictionary<CeresFallingDebrisSegment, Animation<CeresFallingDebrisSegment>>
    {
        [P.Light] = new([new(P.Light, 0, 1, 1)], AnimationEnd.Holds),
        [P.Dark] = new([new(P.Dark, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CeresFallingDebrisSegment> StartingWith(CeresFallingDebrisSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<CeresFallingDebrisSegment, NoAnimationCue> Show(CeresFallingDebrisSegment frame, int duration) => new AnimationBeat<CeresFallingDebrisSegment, NoAnimationCue>.Frame(frame, duration);
}
