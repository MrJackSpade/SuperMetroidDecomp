using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Magdollite.MagdolliteCueKind;
using P = SuperMetroid.Core.Enemies.Magdollite.MagdolliteProgram;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Cartridge values and helpers of <see cref="MagdolliteAnimations"/> that only verification reads.</summary>
internal static class MagdolliteAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MagdolliteProgram[] programs) =>
        programs.SelectMany(program => MagdolliteAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => MagdolliteFrameToolingDefinitions.Spritemap(step.Frame));
}
