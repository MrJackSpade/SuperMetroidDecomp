using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// How Crocomire's melt dissolves his image, played at extraction so the room's tile word sheet pictures each band at every
/// step (#1392, #1406): the cartridge erases one column of the four-bit characters per step, in a fixed order, through a
/// mask chosen by the step. The simulation only counts the steps and selects each band's picture.
/// </summary>
internal static class CrocomireMeltDissolve
{
    /// <summary>Characters in one row of a melt image, and so between the first characters of consecutive bands.</summary>
    public const int BandRowCharacters = 16;

    /// <summary>Characters a band shows from the start of its row.</summary>
    public const int BandCharacterCount = 11;

    /// <summary>Pixel rows each step erases down its column: the whole height of a melt image's bands.</summary>
    private const int ColumnPixels = 48;

    /// <summary>The pixel column step <paramref name="step"/> erases.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="step"/> is not a step of the dissolve.</exception>
    internal static int SelectColumn(int step)
    {
        if ((uint)step >= CrocomireMeltingDefinitions.ColumnCount)
            throw new ArgumentOutOfRangeException(nameof(step));
        return CrocomireMeltDissolveDefinitions.ColumnOrder[step];
    }

    /// <summary>
    /// The mask step <paramref name="step"/> keeps each erased character row's bits through. The cartridge chooses it by the
    /// step's place modulo eight rather than by the erased column, so a step usually clears a different pixel than its
    /// column; this silhouette is what the melt shows.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="step"/> is not a step of the dissolve.</exception>
    internal static byte SelectMask(int step)
    {
        if ((uint)step >= CrocomireMeltingDefinitions.ColumnCount)
            throw new ArgumentOutOfRangeException(nameof(step));
        return (byte)~(128 >> (step & 7));
    }

    /// <summary>
    /// The four-bit characters of <paramref name="illustration"/>'s image after each step of its dissolve, from none erased
    /// through every column, each as the bytes the cartridge's assembled image holds.
    /// </summary>
    internal static IEnumerable<byte[]> Steps(CrocomireMeltingArtwork artwork, CrocomireMeltingIllustration illustration)
    {
        byte[] image = Ensure.NotNull(artwork).Characters(illustration).ToArray();
        yield return [.. image];
        for (int step = 0; step < CrocomireMeltingDefinitions.ColumnCount; step++)
        {
            EraseColumn(image, step);
            yield return [.. image];
        }
    }

    /// <summary>
    /// The first of the room's tile characters <paramref name="band"/>'s characters show over: the first character of the
    /// band's row of the melt image.
    /// </summary>
    internal static int BandFirstCharacter(int band) => band * BandRowCharacters;

    /// <summary>The bytes of the characters <paramref name="band"/> shows in an assembled melt <paramref name="image"/>.</summary>
    internal static ReadOnlyMemory<byte> BandCharacters(byte[] image, int band) =>
        image.AsMemory(band * BandRowCharacters * RoomCharacterAtlasFormat.BytesPerTile,
            BandCharacterCount * RoomCharacterAtlasFormat.BytesPerTile);

    /// <summary>
    /// Erases the column step <paramref name="step"/> selects, every pixel row of it, through the step's mask in all four
    /// bit planes of each character row it crosses.
    /// </summary>
    /// <exception cref="InvalidDataException">The column lies outside the image.</exception>
    internal static void EraseColumn(Span<byte> image, int step)
    {
        int column = SelectColumn(step);
        int mask = SelectMask(step);
        for (int y = 0; y < ColumnPixels; y++)
        {
            int byteIndex = 2 * (y & 7) + ((y & ~7) << 6) + 4 * (column & ~7);
            if (byteIndex + 17 >= image.Length)
                throw new InvalidDataException("Crocomire melting column addressed outside its graphics image.");
            image[byteIndex] = (byte)(image[byteIndex] & mask);
            image[byteIndex + 1] = (byte)(image[byteIndex + 1] & mask);
            image[byteIndex + 16] = (byte)(image[byteIndex + 16] & mask);
            image[byteIndex + 17] = (byte)(image[byteIndex + 17] & mask);
        }
    }
}
