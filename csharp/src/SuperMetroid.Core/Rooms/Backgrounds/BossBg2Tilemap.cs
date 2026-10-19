using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A boss body's BG2 tilemap (#1352): the words a boss shows on BG2 in the hardware's 32x32-screen order, sampled with
/// the tilemap's wrap. The boss publishes it from its body frame; nothing is written into VRAM.
/// </summary>
[PpuLayoutBoundary("backend: samples a boss's BG2 words in the hardware's screen order")]
public sealed class BossBg2Tilemap : IBgTileSource
{
    /// <summary>The tilemap's screen arrangement.</summary>
    private readonly BgTilemapGeometry geometry;

    /// <summary>Creates a tilemap of <paramref name="size"/> from <paramref name="words"/> in screen order.</summary>
    /// <exception cref="ArgumentException">The words do not fill the tilemap.</exception>
    public BossBg2Tilemap(ImmutableArray<ushort> words, Extent16<TilemapCells> size)
    {
        Ensure.LengthEqual(words.AsSpan(), size.X * size.Y);
        Words = words;
        Size = size;
        geometry = new BgTilemapGeometry(size);
    }

    /// <summary>The words in the hardware's screen order: each 32x32 screen's rows, screens left to right then down.</summary>
    public ImmutableArray<ushort> Words { get; }

    /// <summary>The tilemap's size in tiles.</summary>
    public Extent16<TilemapCells> Size { get; }

    /// <inheritdoc />
    public bool TrySample(Position16<BgPlanePixels> scrolled, out BgTileSample sample)
    {
        int x = geometry.WrapX(scrolled.X), y = geometry.WrapY(scrolled.Y);
        var tile = new SnesBgTilemapWord(Words[geometry.EntryWordAddress(0,
            new Position16<TilemapCells>((ushort)BgCharacterGrid.CharacterOf(x), (ushort)BgCharacterGrid.CharacterOf(y)))]);
        sample = new BgTileSample(tile, new Position16<CharacterPixels>(
            (ushort)BgCharacterGrid.PixelWithinCharacter(x), (ushort)BgCharacterGrid.PixelWithinCharacter(y)));
        return true;
    }
}
