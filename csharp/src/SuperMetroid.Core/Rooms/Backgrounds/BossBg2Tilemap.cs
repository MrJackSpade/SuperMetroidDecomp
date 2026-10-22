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
        : this(words, size, default)
    {
    }

    /// <summary>Creates a tilemap whose words are drawn from <paramref name="pictures"/>, or from characters without them.</summary>
    private BossBg2Tilemap(ImmutableArray<ushort> words, Extent16<TilemapCells> size, ImmutableArray<BgTilePicture?> pictures)
    {
        Ensure.LengthEqual(words.AsSpan(), size.X * size.Y);
        Words = words;
        Size = size;
        Pictures = pictures;
        geometry = new BgTilemapGeometry(size);
    }

    /// <summary>
    /// Each word's picture in the boss's body sheet, at least as many as there are words, null where a word draws from
    /// its character; default before the body's pictures are resolved (#1392).
    /// </summary>
    public ImmutableArray<BgTilePicture?> Pictures { get; }

    /// <summary>The tilemap drawn from <paramref name="pictures"/>, the first of which picture its words.</summary>
    /// <exception cref="ArgumentException">There are fewer pictures than words.</exception>
    public BossBg2Tilemap WithPictures(ImmutableArray<BgTilePicture?> pictures)
    {
        if (pictures.IsDefault || pictures.Length < Words.Length)
            throw new ArgumentException($"A boss body of {Words.Length} words needs a picture for each.", nameof(pictures));
        return new BossBg2Tilemap(Words, Size, pictures);
    }

    /// <summary>The body as BG2 shows it: from its pictures once resolved, otherwise from its words' characters.</summary>
    public IBgSource Shown => Pictures.IsDefault ? new BgTileLayer(this) : new Pictured(this);

    /// <summary>The words in the hardware's screen order: each 32x32 screen's rows, screens left to right then down.</summary>
    public ImmutableArray<ushort> Words { get; }

    /// <summary>The tilemap's size in tiles.</summary>
    public Extent16<TilemapCells> Size { get; }

    /// <summary>The body drawn from its pictures, a tile without one from its character.</summary>
    private sealed class Pictured(BossBg2Tilemap map) : IBgSource
    {
        /// <inheritdoc />
        public bool TrySample(Position16<BgPlanePixels> scrolled, out BgSample sample)
        {
            int x = map.geometry.WrapX(scrolled.X), y = map.geometry.WrapY(scrolled.Y);
            int entry = map.geometry.EntryWordAddress(0,
                new Position16<TilemapCells>((ushort)BgCharacterGrid.CharacterOf(x), (ushort)BgCharacterGrid.CharacterOf(y)));
            if (map.Pictures[entry] is { } picture)
            {
                sample = BgSample.OfPicture(picture.Picture, new Position16<ImagePixels>(
                    (ushort)(picture.Corner.X + BgCharacterGrid.PixelWithinCharacter(x)), (ushort)(picture.Corner.Y + BgCharacterGrid.PixelWithinCharacter(y))));
                return true;
            }
            map.TrySample(scrolled, out BgTileSample tile);
            sample = BgSample.OfTile(tile);
            return true;
        }
    }

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
