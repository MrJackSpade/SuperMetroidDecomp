using SuperMetroid.Core.Enemies.Drawing;


namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Builds named ordinary artwork declarations from an explicitly owned finite program.</summary>
internal static class CompiledEnemyCompositionDefinitions
{
    /// <summary>
    /// Resolves a finite program's visual operands into distinct, visible frame exports.
    /// The shared zero-part frame stays renderer-owned; unknown selectors and an unexpected
    /// number of distinct compositions fail installation.
    /// </summary>
    internal static EnemySpritemapDefinition[] Frames(byte bank, ReadOnlySpan<ushort> operands,
        int expectedCount, string name)
    {
        var spritemaps = new List<ushort>(operands.Length);
        foreach (ushort operand in operands)
        {
            if (!CompiledEnemyVisualSelectors.TryGet(bank, operand, out ushort pointer))
                throw new InvalidDataException($"{name} selector ${bank:X2}:{operand:X4} is not compiled.");
            spritemaps.Add(pointer);
        }
        return FramesFromSpritemaps(bank, spritemaps, expectedCount, name);
    }

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
