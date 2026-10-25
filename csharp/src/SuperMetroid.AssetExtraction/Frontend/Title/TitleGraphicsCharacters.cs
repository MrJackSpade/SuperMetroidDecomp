using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>
/// The installed title artwork as the import pipeline reads it (#1409): the Mode 7 floor and Baby Metroid characters as
/// chunky 8-bit pixels and the OBJ characters as SNES 4-bpp planar bytes, which the title sheets compose from.
/// </summary>
public sealed class TitleGraphicsCharacters : TitleGraphicsPresentation
{
    /// <summary>The pixels of one 8-by-8 Mode 7 character.</summary>
    private const int Mode7CharacterBytes = 64;

    /// <summary>The bytes of one 4-bpp OBJ character.</summary>
    private const int ObjectCharacterBytes = 32;

    /// <summary>Owned Mode 7 floor characters, OBJ characters, and Baby Metroid characters.</summary>
    private readonly byte[] floorCharacters, objectCharacters, babyCharacters;

    /// <summary>Retains the compiled characters beside the validated map and compositions.</summary>
    /// <param name="floorMapJson">JSON document containing the Mode 7 map and OBJ compositions.</param>
    /// <param name="floorCharacters">The chunky Mode 7 floor characters.</param>
    /// <param name="objectCharacters">The planar 4-bpp OBJ characters.</param>
    /// <param name="babyCharacters">The chunky Baby Metroid Mode 7 characters.</param>
    private TitleGraphicsCharacters(Stream floorMapJson, byte[] floorCharacters, byte[] objectCharacters, byte[] babyCharacters)
        : base(floorMapJson, floorCharacters.Length / Mode7CharacterBytes, objectCharacters.Length / ObjectCharacterBytes,
            babyCharacters.Length / Mode7CharacterBytes)
    {
        this.floorCharacters = floorCharacters;
        this.objectCharacters = objectCharacters;
        this.babyCharacters = babyCharacters;
    }

    /// <summary>Eight-bit Mode-7 character lane uploaded for the title backdrop.</summary>
    public ReadOnlySpan<byte> FloorCharacters => floorCharacters;

    /// <summary>Separate eight-bit Mode-7 character page for the baby Metroid scene.</summary>
    public ReadOnlySpan<byte> BabyCharacters => babyCharacters;

    /// <summary>The title OBJ page the compositions draw from: the OBJ sheet as the title uploads it (#1358).</summary>
    public SpriteArtwork ObjectPage => field ??= new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.TitleObjects),
        objectCharacters.AsSpan(0, Math.Min(TitleSequenceDefinitions.Graphics.ObjectCharacterByteCount, objectCharacters.Length)));

    /// <summary>Loads and validates the editable title graphics and map assets.</summary>
    /// <param name="floorTilesPng">Indexed 256-color PNG containing the title Mode 7 characters.</param>
    /// <param name="floorMapJson">JSON document containing the Mode 7 map and OBJ compositions.</param>
    /// <param name="objectTilesPng">Indexed 16-color PNG containing title OBJ characters.</param>
    /// <param name="babyTilesPng">Indexed 256-color PNG containing baby Metroid Mode 7 characters.</param>
    /// <returns>The compiled title graphics presentation.</returns>
    /// <exception cref="InvalidDataException">A PNG, the map or a composition is invalid.</exception>
    public static TitleGraphicsCharacters Read(
        Stream floorTilesPng,
        Stream floorMapJson,
        Stream objectTilesPng,
        Stream babyTilesPng)
    {
        IndexedPngImage floorImage = IndexedPng.Read(
            floorTilesPng,
            new(TitleGraphicsFormat.FloorWidth, TitleGraphicsFormat.FloorHeight));
        IndexedPngImage objects = IndexedPng.Read(
            objectTilesPng,
            new(TitleGraphicsFormat.ObjectWidth, TitleGraphicsFormat.ObjectHeight));
        IndexedPngImage baby = IndexedPng.Read(
            babyTilesPng,
            new(TitleGraphicsFormat.BabyWidth, TitleGraphicsFormat.BabyHeight));
        if (floorImage.Colours.Length != 256 || baby.Colours.Length != 256 || objects.Colours.Length != 16)
            throw new InvalidDataException("Title graphics require 256-color Mode 7/Baby PNGs and a 16-color OBJ PNG.");
        return new TitleGraphicsCharacters(floorMapJson,
            SnesMode7TileEncoder.Encode(floorImage.Pixels, floorImage.Size),
            SnesPlanarTileEncoder.Encode(objects.Pixels, objects.Size, 4),
            SnesMode7TileEncoder.Encode(baby.Pixels, baby.Size));
    }

    /// <summary>The characters <paramref name="graphics"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The presentation was not read from installed artwork.</exception>
    public static TitleGraphicsCharacters Of(TitleGraphicsPresentation graphics) => graphics as TitleGraphicsCharacters
        ?? throw new InvalidDataException("The title graphics were not read from installed artwork.");
}
