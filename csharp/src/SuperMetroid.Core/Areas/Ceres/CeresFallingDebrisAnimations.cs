using P = SuperMetroid.Core.Areas.Ceres.CeresFallingDebrisSegment;
using F = SuperMetroid.Core.Areas.Ceres.CeresFallingDebrisLook;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres falling debris's typed animations (#641, #2424): each piece shows its one picture for good while the
/// per-update movement moves it.
/// </summary>
internal static partial class CeresFallingDebrisAnimations
{
    /// <summary>Both pieces' animations.</summary>
    internal static UnpausedAnimationSegments<P, F, NoAnimationCue> Segments { get; } = new(
        new Dictionary<P, AnimationBeat<F, NoAnimationCue>.Unpaused[]>
    {
        [P.Light] =
        [
            Hold(F.Light),
        ],
        [P.Dark] =
        [
            Hold(F.Dark),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<P, Animation<P>> StartingAt { get; } = new Dictionary<P, Animation<P>>
    {
        [P.Light] = new([Segments.Whole(P.Light)], AnimationEnd.Holds),
        [P.Dark] = new([Segments.Whole(P.Dark)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<P> StartingWith(P segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="look"/> for good.</summary>
    private static AnimationBeat<F, NoAnimationCue>.Unpaused Hold(F look) => new AnimationBeat<F, NoAnimationCue>.HeldFrame(look);
}
