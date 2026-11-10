using P = SuperMetroid.Core.Areas.Ceres.CeresFallingDebrisProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres falling tiles' typed programs (#641): one pose, then a sleep while the
/// per-frame movement moves the tile. Each program's frame is the program itself.
/// </summary>
internal static class CeresFallingDebrisAnimations
{
    /// <summary>Both tile programs.</summary>
    internal static AnimationSegments<CeresFallingDebrisProgram, CeresFallingDebrisProgram, NoAnimationCue> Segments { get; } = new(
        new Dictionary<CeresFallingDebrisProgram, AnimationBeat<CeresFallingDebrisProgram, NoAnimationCue>[]>
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
    internal static IReadOnlyDictionary<CeresFallingDebrisProgram, Animation<CeresFallingDebrisProgram>> StartingAt { get; } = new Dictionary<CeresFallingDebrisProgram, Animation<CeresFallingDebrisProgram>>
    {
        [P.Light] = new([new(P.Light, 0, 1, 1)], AnimationEnd.Holds),
        [P.Dark] = new([new(P.Dark, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CeresFallingDebrisProgram> StartingWith(CeresFallingDebrisProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<CeresFallingDebrisProgram, NoAnimationCue> Show(CeresFallingDebrisProgram frame, int duration) => new AnimationBeat<CeresFallingDebrisProgram, NoAnimationCue>.Frame(frame, duration);
}
