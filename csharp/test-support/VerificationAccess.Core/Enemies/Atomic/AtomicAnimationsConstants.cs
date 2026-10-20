using SuperMetroid.Core.Enemies.Atomic.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>Cartridge values and helpers of <see cref="AtomicAnimations"/> that only verification reads.</summary>
internal static class AtomicAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params AtomicProgram[] programs) =>
        programs.SelectMany(program => AtomicAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => AtomicFrameToolingDefinitions.Spritemap(step.Frame));
}
