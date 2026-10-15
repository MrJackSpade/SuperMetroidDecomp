using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Metroids.MetroidCueKind;
using P = SuperMetroid.Core.Enemies.Metroids.MetroidProgram;
using SuperMetroid.Core.Enemies.Metroids.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>Cartridge values and helpers of <see cref="MetroidAnimations"/> that only verification reads.</summary>
internal static class MetroidAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MetroidProgram[] programs) =>
        programs.SelectMany(program => MetroidAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => MetroidFrameDefinitions.Spritemap(step.Frame));
}
