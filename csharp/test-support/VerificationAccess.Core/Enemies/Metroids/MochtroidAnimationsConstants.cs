using P = SuperMetroid.Core.Enemies.Metroids.MochtroidProgram;
using SuperMetroid.Core.Enemies.Metroids.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>Cartridge values and helpers of <see cref="MochtroidAnimations"/> that only verification reads.</summary>
internal static class MochtroidAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MochtroidProgram[] programs) =>
        programs.SelectMany(program => MochtroidAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => MochtroidFrameDefinitions.Spritemap(step.Frame));
}
