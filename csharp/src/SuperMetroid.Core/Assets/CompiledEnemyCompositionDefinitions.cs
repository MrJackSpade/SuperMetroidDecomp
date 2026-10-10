using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Builds named ordinary artwork declarations from an explicitly owned finite program.</summary>
internal static class CompiledEnemyCompositionDefinitions
{
    /// <summary>Resolves a finite program's presentation operands into unique exported ordinary-enemy spritemap definitions.</summary>
    /// <param name="bank">ROM bank containing the instruction operands and selected spritemaps.</param>
    /// <param name="operands">Presentation operand addresses whose compiled selectors identify the visible frames.</param>
    /// <param name="expectedCount">Required number of distinct exported compositions after shared empty frames are omitted.</param>
    /// <param name="name">Name prefix used to identify the generated frame definitions in diagnostics and exports.</param>
    /// <returns>Definitions for the distinct visible spritemaps selected by the supplied program.</returns>
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
