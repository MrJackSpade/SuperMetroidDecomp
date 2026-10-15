using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's breakup fragment programs (#641), one per <see cref="RidleyBreakupFrame"/>: each shows its pose for one
/// update and then holds, as the native one-frame lists at $A6:CA47..CAEF do.
/// </summary>
internal static class RidleyBreakupAnimations
{
    /// <summary>Every fragment program.</summary>
    internal static AnimationProgramSet<RidleyBreakupFrame, RidleyBreakupFrame, NoAnimationCue> Programs { get; } = new(
        Enum.GetValues<RidleyBreakupFrame>().ToDictionary(
            frame => frame,
            frame => new[]
            {
                AnimationProgramStep<RidleyBreakupFrame, RidleyBreakupFrame, NoAnimationCue>.Show(frame, 1),
                AnimationProgramStep<RidleyBreakupFrame, RidleyBreakupFrame, NoAnimationCue>.Hold,
            }));
}
