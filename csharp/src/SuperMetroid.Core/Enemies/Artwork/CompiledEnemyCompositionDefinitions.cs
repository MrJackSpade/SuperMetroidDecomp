using SuperMetroid.Core.Enemies.Drawing;


namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Builds named ordinary artwork declarations from an explicitly owned finite program.</summary>
internal static class CompiledEnemyCompositionDefinitions
{

    /// <summary>
    /// Names the distinct, visible spritemaps a typed program shows, in first-shown order (#641). The shared zero-part
    /// frame stays renderer-owned; an unexpected number of distinct compositions fails installation.
    /// </summary>
    internal static EnemySpritemapDefinition[] FramesFromSpritemaps(byte bank, IEnumerable<ushort> spritemaps,
        int expectedCount, string name)
    {
        var frames = new List<EnemySpritemapDefinition>(expectedCount);
        var seen = new HashSet<ushort>();
        foreach (ushort pointer in spritemaps)
        {
            if (CommonEnemyEmptyExtendedFrameDefinitions.HasEmptySpritemap(bank, pointer) ||
                !seen.Add(pointer)) continue;
            frames.Add(new(bank, pointer, $"{name}_{frames.Count:D2}"));
        }
        if (frames.Count != expectedCount)
            throw new InvalidDataException($"{name} selects {frames.Count} compositions, expected {expectedCount}.");
        return [.. frames];
    }
}
