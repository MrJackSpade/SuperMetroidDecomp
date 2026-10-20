using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Cartridge values and helpers of <see cref="FakeKraidAnimations"/> that only verification reads.</summary>
internal static class FakeKraidAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params FakeKraidProgram[] programs) =>
        programs.SelectMany(program => FakeKraidAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => FakeKraidFrameToolingDefinitions.Spritemap(step.Frame));
}
