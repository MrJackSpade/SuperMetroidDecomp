using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy BG2 image a boss draws its body into (#1353): the 32x64 tilemap of <c>$7E:2000</c> that the cartridge
/// uploads over the room's BG2 tilemap. A boss that owns it publishes it as its body layer; nothing reaches VRAM.
/// </summary>
internal sealed class EnemyBg2Image
{
    /// <summary>The image's words, row by row, 32 per row.</summary>
    private readonly ushort[] words;

    /// <summary>The last published image, until a write changes it.</summary>
    [NonSerialized]
    private EnemyBg2ImageSnapshot? published;

    /// <summary>Creates an image with every word <paramref name="fill"/>, as a boss's initialization clears it.</summary>
    public EnemyBg2Image(int fill)
    {
        words = new ushort[EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight];
        words.AsSpan().Fill(checked((ushort)fill));
    }

    /// <summary>Writes <paramref name="tiles"/> from word <paramref name="firstWord"/> onward.</summary>
    /// <exception cref="InvalidDataException">The run leaves the image.</exception>
    public void Write(int firstWord, ReadOnlySpan<ushort> tiles)
    {
        if (firstWord < 0 || firstWord + tiles.Length > words.Length)
            throw new InvalidDataException($"An enemy BG2 write of {tiles.Length} words at {firstWord} leaves the {words.Length}-word image.");
        tiles.CopyTo(words.AsSpan(firstWord));
        published = null;
    }

    /// <summary>The image as it stands, shared until the next write.</summary>
    public EnemyBg2ImageSnapshot Publish() => published ??= new EnemyBg2ImageSnapshot([.. words]);
}
