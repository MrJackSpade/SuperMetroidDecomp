using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// The installed post-credits reward icon as the import pipeline reads it (#1409): the native $8000-byte reward-jump
/// stream, one tilemap byte followed by one Mode 7 character byte per word, which the reward icon sheet composes from.
/// </summary>
public sealed class EndingRewardIconCharacters : EndingRewardIconArtwork
{
    /// <summary>Retains the interleaved full-map and character transfer for reward landing.</summary>
    /// <param name="map">The row-ordered tile indices of the full 128x128 map.</param>
    /// <param name="transfer">The interleaved map and character stream.</param>
    private EndingRewardIconCharacters(byte[] map, byte[] transfer)
        : base(map, (transfer.Length - map.Length) / EndingFloorArtworkFormat.CharacterBytes) => Bytes = transfer;

    /// <summary>The $8000-byte low-map/high-character word stream consumed by the sixteen $0800-byte uploads at native $8B:F682.</summary>
    public ReadOnlyMemory<byte> Bytes { get; }

    /// <summary>Compiles the full 128x128 tilemap and the 256-tile character atlas into the native interleaved post-credits beam/icon stream.</summary>
    /// <param name="mapJson">Version-1 JSON containing 16384 row-ordered tile indices, each in the range 0..255.</param>
    /// <param name="charactersPng">A 128x128, 256-color indexed PNG; its palette is validated but its colors are not installed.</param>
    /// <returns>A transfer with each map byte followed by the corresponding byte of tile-ordered chunky character data.</returns>
    /// <exception cref="ArgumentNullException">Either input stream is null.</exception>
    /// <exception cref="InvalidDataException">The map schema or indexed character atlas is invalid.</exception>
    public static EndingRewardIconCharacters Read(Stream mapJson, Stream charactersPng)
    {
        Ensure.NotNull(mapJson);
        Ensure.NotNull(charactersPng);
        byte[] map = ReadMapCells(mapJson);
        IndexedPngImage image = IndexedPng.Read(charactersPng,
            new(EndingFloorArtworkFormat.CharacterWidth, EndingFloorArtworkFormat.CharacterHeight));
        if (image.Colours.Length != 256)
            throw new InvalidDataException("Reward icon characters require a 256-color indexed PNG.");
        byte[] characters = SnesMode7TileEncoder.Encode(image.Pixels,
            image.Size);
        var transfer = new byte[EndingRewardIconArtworkFormat.ByteCount];
        for (int index = 0; index < EndingRewardIconArtworkFormat.MapBytes; index++)
        {
            transfer[index * 2] = map[index];
            transfer[index * 2 + 1] = characters[index];
        }
        return new EndingRewardIconCharacters(map, transfer);
    }

    /// <summary>The characters <paramref name="icon"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The icon was not read from installed artwork.</exception>
    public static EndingRewardIconCharacters Of(EndingRewardIconArtwork icon) => icon as EndingRewardIconCharacters
        ?? throw new InvalidDataException("The reward icon was not read from installed artwork.");
}
