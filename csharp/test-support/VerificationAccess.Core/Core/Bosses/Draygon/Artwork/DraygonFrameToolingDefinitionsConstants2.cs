using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>Cartridge values and helpers of <see cref="DraygonFrameToolingDefinitions"/> that only verification reads.</summary>
internal static class DraygonFrameToolingDefinitionsConstants2
{
    /// <summary>Every extended spritemap that shows <paramref name="frame"/>.</summary>
    internal static IEnumerable<ushort> Spritemaps(EnemyFrameId frame) =>
        Enumerable.Range(0, DraygonFrameToolingDefinitions.CartridgeOrder.Length).Where(position => DraygonFrameToolingDefinitions.CartridgeOrder[position] == frame).Select(DraygonFrameToolingDefinitions.RootAt);
}
