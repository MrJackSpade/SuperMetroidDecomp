using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Crawlers.CrawlerProgram;
using SuperMetroid.Core.Enemies.Crawlers.Artwork;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Cartridge values and helpers of <see cref="CrawlerAnimations"/> that only verification reads.</summary>
internal static class CrawlerAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params CrawlerProgram[] programs) =>
        programs.SelectMany(program => CrawlerAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, CrawlerFrame, CrawlerCue>.ShowStep>()
            .Select(step => CrawlerFrameToolingDefinitions.Spritemap(step.Frame));
}
