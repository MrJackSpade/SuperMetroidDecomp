using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>One frame a boss draws on its body layer as the cartridge writes it (#1466): the tile each layer cell receives.</summary>
/// <param name="Name">The frame's name in the boss's body sheet.</param>
/// <param name="Cells">The tile written at each layer cell, by cell in reading order.</param>
internal sealed record BossBodyFrameSource(string Name, SortedDictionary<int, BackgroundTile> Cells)
{
    /// <summary>The frame writing <paramref name="tiles"/> from layer cell <paramref name="firstCell"/> onward.</summary>
    internal static BossBodyFrameSource Written(string name, int firstCell, ReadOnlySpan<BackgroundTile> tiles)
    {
        var cells = new SortedDictionary<int, BackgroundTile>();
        for (int index = 0; index < tiles.Length; index++)
            cells[firstCell + index] = tiles[index];
        return new BossBodyFrameSource(name, cells);
    }
}
