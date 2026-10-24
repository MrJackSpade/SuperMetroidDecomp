using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// One installed ending backdrop as the import pipeline reads it (#1409): its Mode 7 characters as chunky 8-bit pixels,
/// the high-byte lane the ending floor sheets compose from. The discarded odd bytes of the native packed map source never
/// reach visible VRAM because the subsequent high-byte-only transfer overwrites them.
/// </summary>
public sealed class EndingFloorSceneCharacters : EndingFloorSceneArtwork
{
    /// <summary>Retains the validated low-byte map and high-byte character lanes for one scene.</summary>
    /// <param name="map">The row-ordered tile indices.</param>
    /// <param name="characters">The chunky Mode 7 characters.</param>
    private EndingFloorSceneCharacters(byte[] map, byte[] characters)
        : base(map, characters.Length / EndingFloorArtworkFormat.CharacterBytes) => Characters = characters;

    /// <summary>The $4000 chunky pixel bytes for 256 ordered 8x8 tiles, written to the high VRAM byte lane.</summary>
    public ReadOnlyMemory<byte> Characters { get; }

    /// <summary>Validates a 128x64 map and a 128x128 indexed character atlas, then compiles their separate native Mode-7 lanes.</summary>
    /// <param name="mapJson">Version-1 JSON containing 8192 ordered tile indices in the range 0..255.</param>
    /// <param name="charactersPng">A 256-color indexed PNG whose 16x16 grid supplies the 256 character tiles; palette colors are not imported here.</param>
    /// <returns>The decoded map lane and tile-ordered character lane.</returns>
    /// <exception cref="ArgumentNullException">Either input stream is null.</exception>
    /// <exception cref="InvalidDataException">The map schema, atlas format, palette length, or native transfer length is invalid.</exception>
    public static EndingFloorSceneCharacters Read(Stream mapJson, Stream charactersPng)
    {
        Ensure.NotNull(mapJson);
        Ensure.NotNull(charactersPng);
        byte[] map = ReadMapCells(mapJson);
        IndexedPngImage image = IndexedPng.Read(charactersPng,
            new(EndingFloorArtworkFormat.CharacterWidth, EndingFloorArtworkFormat.CharacterHeight));
        if (image.Colours.Length != 256)
            throw new InvalidDataException("Ending Mode-7 characters require a 256-color indexed PNG.");
        byte[] characters = SnesMode7TileEncoder.Encode(image.Pixels,
            image.Size);
        if (characters.Length != EndingFloorArtworkFormat.CharacterByteCount)
            throw new InvalidDataException("Ending Mode-7 PNG has the wrong native transfer length.");
        return new EndingFloorSceneCharacters(map, characters);
    }

    /// <summary>The characters <paramref name="scene"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The scene was not read from installed artwork.</exception>
    public static EndingFloorSceneCharacters Of(EndingFloorSceneArtwork scene) => scene as EndingFloorSceneCharacters
        ?? throw new InvalidDataException("The ending backdrop was not read from installed artwork.");
}
