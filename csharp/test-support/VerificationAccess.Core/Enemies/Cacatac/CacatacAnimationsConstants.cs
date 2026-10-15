using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Cacatac.CacatacCueKind;
using P = SuperMetroid.Core.Enemies.Cacatac.CacatacProgram;
using SuperMetroid.Core.Enemies.Cacatac.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Cartridge values and helpers of <see cref="CacatacAnimations"/> that only verification reads.</summary>
internal static class CacatacAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params CacatacProgram[] programs) =>
        programs.SelectMany(program => CacatacAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => CacatacFrameDefinitions.Spritemap(step.Frame));
}
