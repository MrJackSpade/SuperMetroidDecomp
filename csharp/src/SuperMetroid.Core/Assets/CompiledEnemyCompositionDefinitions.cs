using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Builds named ordinary artwork declarations from an explicitly owned finite program.</summary>
internal static class CompiledEnemyCompositionDefinitions
{
    internal static EnemySpritemapDefinition[] Frames(byte bank, ReadOnlySpan<ushort> operands,
        int expectedCount, string name)
    {
        var frames = new List<EnemySpritemapDefinition>(expectedCount);
        var seen = new HashSet<ushort>();
        foreach (ushort operand in operands)
        {
            if (!CompiledEnemyVisualSelectors.TryGet(bank, operand, out ushort pointer))
                throw new InvalidDataException($"{name} selector ${bank:X2}:{operand:X4} is not compiled.");
            // The renderer owns the shared zero-part frame. Everything visible
            // selected by this supplied program must be an actual installed export.
            if (CommonEnemyEmptyExtendedFrameDefinitions.HasEmptySpritemap(bank, pointer) ||
                !seen.Add(pointer)) continue;
            frames.Add(new(bank, pointer, $"{name}_{frames.Count:D2}"));
        }
        if (frames.Count != expectedCount)
            throw new InvalidDataException($"{name} selects {frames.Count} compositions, expected {expectedCount}.");
        return [.. frames];
    }
}
