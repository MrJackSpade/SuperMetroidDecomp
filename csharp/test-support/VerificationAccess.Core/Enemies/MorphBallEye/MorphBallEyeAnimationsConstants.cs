using P = SuperMetroid.Core.Enemies.MorphBallEye.MorphBallEyeProgram;
using SuperMetroid.Core.Enemies.MorphBallEye.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>Cartridge values and helpers of <see cref="MorphBallEyeAnimations"/> that only verification reads.</summary>
internal static class MorphBallEyeAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MorphBallEyeProgram[] programs) =>
        programs.SelectMany(program => MorphBallEyeAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => MorphBallEyeFrameDefinitions.Spritemap(step.Frame));
}
