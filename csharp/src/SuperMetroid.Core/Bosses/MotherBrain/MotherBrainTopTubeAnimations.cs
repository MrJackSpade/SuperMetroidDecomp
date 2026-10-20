using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTopTubeProgram;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's falling ceiling-tube programs (#641), in place of their bank-$86 lists: one pose, then hold.</summary>
internal static class MotherBrainTopTubeAnimations
{
    /// <summary>Every program; each shows its own tube.</summary>
    internal static AnimationProgramSet<MotherBrainTopTubeProgram, MotherBrainTopTubeProgram, NoAnimationCue> Programs { get; } = new(
        new Dictionary<MotherBrainTopTubeProgram, AnimationProgramStep<MotherBrainTopTubeProgram, MotherBrainTopTubeProgram, NoAnimationCue>[]>
    {
        [P.TopRight] = [AnimationProgramStep<P, P, NoAnimationCue>.Show(P.TopRight, 1), AnimationProgramStep<P, P, NoAnimationCue>.Hold],
        [P.TopLeft] = [AnimationProgramStep<P, P, NoAnimationCue>.Show(P.TopLeft, 1), AnimationProgramStep<P, P, NoAnimationCue>.Hold],
        [P.TopMiddleLeft] = [AnimationProgramStep<P, P, NoAnimationCue>.Show(P.TopMiddleLeft, 1), AnimationProgramStep<P, P, NoAnimationCue>.Hold],
        [P.TopMiddleRight] = [AnimationProgramStep<P, P, NoAnimationCue>.Show(P.TopMiddleRight, 1), AnimationProgramStep<P, P, NoAnimationCue>.Hold],
    });
}
