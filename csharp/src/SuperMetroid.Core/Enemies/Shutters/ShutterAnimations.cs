using P = SuperMetroid.Core.Enemies.Shutters.ShutterProgram;
using SuperMetroid.Core.Enemies.Shutters.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The growing, vertical and horizontal shutters' poses and Kamer's platform loop as typed programs (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="ShutterFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class ShutterAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<ShutterProgram, ShutterFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<ShutterProgram, AnimationProgramStep<ShutterProgram, ShutterFrame, NoAnimationCue>[]>
    {
        // $A2:E998
        [P.GrowingTenPixels] =
        [
            Show(0, 1),
            Hold,
        ],
        // $A2:E99E
        [P.GrowingTwentyPixels] =
        [
            Show(1, 1),
            Hold,
        ],
        // $A2:E9A4
        [P.GrowingThirtyPixels] =
        [
            Show(2, 1),
            Hold,
        ],
        // $A2:E9AA
        [P.FortyPixels] =
        [
            Show(3, 1),
            Hold,
        ],
        // $A2:E9D4
        [P.Horizontal] =
        [
            Show(4, 1),
            Hold,
        ],
        // $A2:EDE7
        [P.KamerPlatform] =
        [
            Show(5, 10),
            Show(6, 10),
            Show(7, 10),
            Show(8, 10),
            GoTo(P.KamerPlatform),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<ShutterProgram, ShutterFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<ShutterProgram, ShutterFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<ShutterProgram, ShutterFrame, NoAnimationCue> GoTo(ShutterProgram program) => AnimationProgramStep<ShutterProgram, ShutterFrame, NoAnimationCue>.GoTo(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<ShutterProgram, ShutterFrame, NoAnimationCue> Hold => AnimationProgramStep<ShutterProgram, ShutterFrame, NoAnimationCue>.Hold;

}
