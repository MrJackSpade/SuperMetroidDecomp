using P = SuperMetroid.Core.Enemies.WreckedShip.WreckedShipGhostProgram;
using SuperMetroid.Core.Enemies.WreckedShip.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// The Wrecked Ship ghost's (native Coven) floating loop as a typed program (#641), in place of their bank-$A8 lists. Generated from the verified native lists: each
/// frame shows a <see cref="WreckedShipGhostFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class WreckedShipGhostAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<WreckedShipGhostProgram, WreckedShipGhostFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<WreckedShipGhostProgram, AnimationProgramStep<WreckedShipGhostProgram, WreckedShipGhostFrame, NoAnimationCue>[]>
    {
        [P.Floating] =
        [
            Show(0, 16),
            Show(1, 16),
            Show(2, 16),
            GoTo(P.Floating),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<WreckedShipGhostProgram, WreckedShipGhostFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<WreckedShipGhostProgram, WreckedShipGhostFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<WreckedShipGhostProgram, WreckedShipGhostFrame, NoAnimationCue> GoTo(WreckedShipGhostProgram program) => AnimationProgramStep<WreckedShipGhostProgram, WreckedShipGhostFrame, NoAnimationCue>.GoTo(program);

}
