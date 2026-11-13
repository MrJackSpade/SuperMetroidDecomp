using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Cartridge values and helpers of <see cref="FakeKraidAnimations"/> that only verification reads.</summary>
internal static class FakeKraidAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params FakeKraidProgram[] programs) =>
        programs.SelectMany(program => FakeKraidAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, FakeKraidCue>.Frame>()
            .Select(frame => FakeKraidFrameToolingDefinitions.Spritemap(frame.Shown));
}
