using P = SuperMetroid.Core.Areas.Ceres.CeresFallingDebrisProgram;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres falling tiles' typed programs (#641), in place of their bank-$86 lists: one pose, then a sleep while the
/// pre-instruction moves the tile. Each program's frame is the program itself.
/// </summary>
internal static class CeresFallingDebrisAnimations
{
    /// <summary>Both tile programs.</summary>
    internal static AnimationProgramSet<CeresFallingDebrisProgram, CeresFallingDebrisProgram, NoAnimationCue> Programs { get; } = new(
        new Dictionary<CeresFallingDebrisProgram, AnimationProgramStep<CeresFallingDebrisProgram, CeresFallingDebrisProgram, NoAnimationCue>[]>
    {
        [P.Light] = [Show(P.Light, 1), Hold],
        [P.Dark] = [Show(P.Dark, 1), Hold],
    });

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<CeresFallingDebrisProgram, CeresFallingDebrisProgram, NoAnimationCue> Show(CeresFallingDebrisProgram frame, int duration) => AnimationProgramStep<CeresFallingDebrisProgram, CeresFallingDebrisProgram, NoAnimationCue>.Show(frame, duration);

    /// <summary>Holds the last frame.</summary>
    private static AnimationProgramStep<CeresFallingDebrisProgram, CeresFallingDebrisProgram, NoAnimationCue> Hold => AnimationProgramStep<CeresFallingDebrisProgram, CeresFallingDebrisProgram, NoAnimationCue>.Hold;
}
