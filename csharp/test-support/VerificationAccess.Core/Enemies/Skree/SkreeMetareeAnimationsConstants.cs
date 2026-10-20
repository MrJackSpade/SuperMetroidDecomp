using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Skree.Artwork;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>Cartridge values and helpers of <see cref="SkreeMetareeAnimations"/> that only verification reads.</summary>
internal static class SkreeMetareeAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params SkreeMetareeProgram[] programs) =>
        programs.SelectMany(program => SkreeMetareeAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => SkreeMetareeFrameToolingDefinitions.Spritemap(step.Frame));
}
