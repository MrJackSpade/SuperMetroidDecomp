using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>
/// The installed escape-timer typeface as the import pipeline reads it (#1409): the installed PNG's pixels compiled to the
/// timer's four-bit planar characters, which only the import reads (#1513).
/// </summary>
public sealed class EscapeTimerCharacters
{
    /// <summary>The 25 timer characters in four-bit planar load order.</summary>
    internal readonly byte[] characters;

    /// <summary>Retains the installed characters.</summary>
    /// <param name="characters">The 25 timer characters the installed pixels compile to, in transfer order.</param>
    private EscapeTimerCharacters(byte[] characters)
    {
        this.characters = characters;
        Artwork = new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.EscapeTimerTiles), characters);
    }

    /// <summary>The 25 timer characters as owned sprite artwork, in transfer order: character 0 is OBJ character $1E0.</summary>
    public SpriteArtwork Artwork { get; }

    /// <summary>Loads the indexed PNG and compiles its pixels to the cartridge's two native DMA pages.</summary>
    /// <exception cref="InvalidDataException">The PNG is malformed, the wrong size, or compiles to the wrong length.</exception>
    public static EscapeTimerCharacters Read(Stream png)
    {
        IndexedPngImage image = IndexedPng.Read(
            png,
            new(EscapeTimerTileAtlasFormat.Width, EscapeTimerTileAtlasFormat.Height));
        byte[] planar = SnesPlanarTileEncoder.Encode(image.Pixels, image.Size, 4);
        if (planar.Length != EscapeTimerTileAtlasFormat.TotalByteCount)
            throw new InvalidDataException(
                $"Escape timer artwork compiled to {planar.Length} bytes; expected {EscapeTimerTileAtlasFormat.TotalByteCount}.");
        return new(planar);
    }
}
