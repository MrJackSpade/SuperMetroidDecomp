using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A boss body's BG2 tilemap (#1352): the words a boss shows on BG2 in the hardware's 32x32-screen order, which the room's
/// body sheet pictures (#1392). The boss publishes it from its body frame; nothing is written into VRAM.
/// </summary>
public sealed class BossBg2Tilemap
{
    /// <summary>Creates a tilemap of <paramref name="size"/> from <paramref name="words"/> in screen order.</summary>
    /// <exception cref="ArgumentException">The words do not fill the tilemap.</exception>
    public BossBg2Tilemap(ImmutableArray<ushort> words, Extent16<TilemapCells> size)
    {
        Ensure.LengthEqual(words.AsSpan(), size.X * size.Y);
        Words = words;
        Size = size;
    }

    /// <summary>The words in the hardware's screen order: each 32x32 screen's rows, screens left to right then down.</summary>
    public ImmutableArray<ushort> Words { get; }

    /// <summary>The tilemap's size in tiles.</summary>
    public Extent16<TilemapCells> Size { get; }
}
