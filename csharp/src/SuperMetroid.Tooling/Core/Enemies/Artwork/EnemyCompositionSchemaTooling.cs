using SuperMetroid.Core.World.Artwork;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Development-tool views of the installed composition schemas and BG2 catalogs by cartridge address, for oracles that
/// compare them with cartridge data; never linked by player hosts.
/// </summary>
internal static class EnemyCompositionSchemaTooling
{
    /// <summary>Every composition of both schemas, by the frame it shows.</summary>
    private static readonly Lazy<Dictionary<EnemyFrameId, EnemySpritemapDefinition>> Compositions = new(() =>
        EnemySpritemapDefinitions.Frames.Concat(EnemyExtendedFrameDefinitions.Frames).ToDictionary(definition => definition.Frame));

    /// <summary>One family's compositions, in schema order: every composition whose name starts with <paramref name="prefix"/>.</summary>
    internal static EnemySpritemapDefinition[] Family(string prefix) =>
        [.. EnemySpritemapDefinitions.Frames.Concat(EnemyExtendedFrameDefinitions.Frames)
            .Where(definition => definition.Name.StartsWith(prefix, StringComparison.Ordinal))];

    /// <summary>The composition that shows <paramref name="frame"/>.</summary>
    internal static EnemySpritemapDefinition CompositionOf(EnemyFrameId frame) => Compositions.Value[frame];

}
