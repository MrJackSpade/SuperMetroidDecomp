using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy BG2 image a boss draws its body into (#1353): the 32x64 tilemap of <c>$7E:2000</c> that the cartridge
/// uploads over the room's BG2 tilemap. A boss that owns it publishes it as its body layer; nothing reaches VRAM.
/// </summary>
internal sealed class EnemyBg2Image
{
    /// <summary>The image's tiles, row by row, 32 per row.</summary>
    private readonly BackgroundTile[] tiles;

    /// <summary>The last published image, until a write changes it.</summary>
    [NonSerialized]
    private EnemyBg2ImageSnapshot? published;

    /// <summary>Creates an image with every cell showing <paramref name="fill"/>, as a boss's initialization clears it.</summary>
    public EnemyBg2Image(BackgroundTile fill)
    {
        tiles = new BackgroundTile[EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight];
        tiles.AsSpan().Fill(fill);
    }

    /// <summary>Places <paramref name="written"/> from cell <paramref name="firstCell"/> onward.</summary>
    /// <exception cref="InvalidDataException">The run leaves the image.</exception>
    public void Write(int firstCell, ReadOnlySpan<BackgroundTile> written)
    {
        if (firstCell < 0 || firstCell + written.Length > tiles.Length)
            throw new InvalidDataException($"An enemy BG2 write of {written.Length} tiles at {firstCell} leaves the {tiles.Length}-cell image.");
        written.CopyTo(tiles.AsSpan(firstCell));
        published = null;
    }

    /// <summary>The image as it stands, shared until the next write.</summary>
    public EnemyBg2ImageSnapshot Publish() => published ??= new EnemyBg2ImageSnapshot([.. tiles]);
}
