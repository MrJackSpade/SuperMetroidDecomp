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

    /// <summary>
    /// The first of the room's tile characters <paramref name="band"/>'s characters show over: the first character of the
    /// band's row of the melt image.
    /// </summary>
    internal static int BandFirstCharacter(int band) => band * BandRowCharacters;

    /// <summary>The bytes of the characters <paramref name="band"/> shows in an assembled melt <paramref name="image"/>.</summary>
    internal static ReadOnlyMemory<byte> BandCharacters(byte[] image, int band) =>
        image.AsMemory(band * BandRowCharacters * RoomCharacterAtlasFormat.BytesPerTile,
            BandCharacterCount * RoomCharacterAtlasFormat.BytesPerTile);
}
