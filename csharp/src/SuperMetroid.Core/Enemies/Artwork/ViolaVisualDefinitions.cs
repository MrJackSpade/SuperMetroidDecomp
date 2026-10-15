using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The eight distinct bank-$A3 OAM compositions shown by Viola's fourteen-frame normal loop. Its typed program owns
/// durations and branches.
/// </summary>
internal static class ViolaVisualDefinitions
{
    /// <summary>Native Viola spritemap bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>Fourteen loop frames reuse eight distinct Viola spin spritemaps.</summary>
    internal const int FrameCount = 8;

    /// <summary>Collects each distinct loop frame once, in loop order, and rejects changes to the expected eight-frame artwork set.</summary>
    internal static EnemySpritemapDefinition[] Frames()
    {
        EnemySpritemapDefinition[] frames = [.. CrawlerAnimations.Spritemaps(CrawlerProgram.ViolaNormal).Distinct()
            .Select(pointer => new EnemySpritemapDefinition(Bank, pointer, $"viola_spin_{pointer:x4}"))];
        if (frames.Length != FrameCount)
            throw new InvalidDataException($"Viola shows {frames.Length} distinct frames, expected {FrameCount}.");
        return frames;
    }
}
