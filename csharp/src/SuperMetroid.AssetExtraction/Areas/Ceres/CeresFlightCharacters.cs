using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// The installed approach to Ceres as the import pipeline reads it (#1409): its Mode 7 floor characters as chunky 8-bit
/// pixels and its OBJ characters as SNES 4-bpp planar bytes, which the flight sheets compose from, beside the maps, palette
/// and compositions the game binds.
/// </summary>
public sealed class CeresFlightCharacters : CeresFlightArtworkCatalog
{
    /// <summary>Retains compiled characters and selected palette, OAM, and actor resources.</summary>
    /// <param name="floorCharacters">The chunky Mode 7 floor characters.</param>
    /// <param name="floorMaps">Front 768 map bytes followed by rear 768 map bytes.</param>
    /// <param name="objectCharacters">The planar 4-bpp OBJ characters.</param>
    /// <param name="palette">The approach palette.</param>
    /// <param name="sprites">The approach's OAM compositions.</param>
    /// <param name="actors">The rear-view actors' placements.</param>
    private CeresFlightCharacters(byte[] floorCharacters, byte[] floorMaps,
        byte[] objectCharacters, CeresFlightPalette palette,
        CeresFlightSpritePresentation sprites, CeresFlightActorLayout actors)
        : base(floorMaps, palette, sprites, actors, floorCharacters.Length / CeresFlightArtworkFormat.FloorCharacterBytes,
            objectCharacters.Length / CeresFlightArtworkFormat.ObjectCharacterBytes)
    {
        FloorCharacters = floorCharacters;
        ObjectCharacters = objectCharacters;
    }

    /// <summary>Owned $4000-byte stream of 256 chunky 8-bpp Mode-7 characters, imported from <c>Tiles_Gunship_Ceres_Mode7</c> at <c>$95:A82F</c>; also reused by the ending flyaway.</summary>
    public ReadOnlyMemory<byte> FloorCharacters { get; }

    /// <summary>Owned $4000-byte stream of 512 planar 4-bpp characters, imported from <c>Tiles_Space_Ceres</c> at <c>$96:D10A</c> and uploaded to VRAM bytes $C000-$FFFF.</summary>
    public ReadOnlyMemory<byte> ObjectCharacters { get; }

    /// <summary>The flight OBJ page the space, gunship and station compositions draw from: the OBJ sheet as $BCCD uploads it (#1358).</summary>
    public SpriteArtwork ObjectPage => field ??= new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.CeresFlightObjects),
        ObjectCharacters.Span[..CeresFlightRomData.Transfers.ObjectCharacterByteCount]);

    /// <inheritdoc/>
    public override string ContentIdentity => SelectedPresentationHash.Create(nameof(CeresFlightArtworkCatalog), content =>
    {
        content.Append("mode7-characters", FloorCharacters.Span);
        content.Append("mode7-maps", FloorMaps.Span);
        content.Append("object-characters", ObjectCharacters.Span);
        Span<byte> paletteBytes = stackalloc byte[CeresFlightPalette.ColorWordBytes];
        Palette.CopyColorWordsTo(paletteBytes);
        content.Append("palette", paletteBytes);
        content.Append("sprites", Convert.FromHexString(Sprites.ContentIdentity));
        content.Append("actors", Convert.FromHexString(Actors.ContentIdentity));
    });

    /// <summary>Compiles the six selected Ceres approach resources into native character, map, palette, and actor presentation data.</summary>
    /// <param name="floorPng">128-by-128 indexed PNG with 256 palette entries, supplying 256 chunky 8-bpp characters.</param>
    /// <param name="mapJson">Version-one document containing both ordered 768-byte front/rear map slices.</param>
    /// <param name="objectPng">256-by-128 indexed PNG with sixteen palette entries, supplying 512 planar 4-bpp characters.</param>
    /// <param name="palette">The approach's 256 selected colours, read from the installed palette document.</param>
    /// <param name="spritesJson">Document containing the six required named OAM compositions.</param>
    /// <param name="actorsJson">Document containing the five required ordered rear-view actor identities and initial coordinates.</param>
    /// <returns>A catalog owning newly compiled transfer bytes and loaded presentation resources.</returns>
    /// <remarks>Reads from the streams' current positions without disposing them. PNG pixel indices define characters; the separate palette resource defines rendered colors. File provenance and stock hashes are validated by installation loading, not by this method.</remarks>
    /// <exception cref="ArgumentNullException">Any resource stream is null.</exception>
    /// <exception cref="InvalidDataException">A resource has an invalid format, dimensions, palette size, schema, map index, actor identity, composition, or compiled transfer length.</exception>
    public static CeresFlightCharacters Read(Stream floorPng, Stream mapJson,
        Stream objectPng, CeresFlightPalette palette, Stream spritesJson, Stream actorsJson)
    {
        Ensure.NotNull(floorPng);
        Ensure.NotNull(mapJson);
        Ensure.NotNull(objectPng);
        Ensure.NotNull(palette);
        Ensure.NotNull(spritesJson);
        Ensure.NotNull(actorsJson);
        IndexedPngImage floorImage = IndexedPng.Read(floorPng,
            new(CeresFlightArtworkFormat.FloorWidth, CeresFlightArtworkFormat.FloorHeight));
        IndexedPngImage objects = IndexedPng.Read(objectPng,
            new(CeresFlightArtworkFormat.ObjectWidth, CeresFlightArtworkFormat.ObjectHeight));
        if (floorImage.Colours.Length != 256 || objects.Colours.Length != 16)
            throw new InvalidDataException("Ceres flight requires 256-color Mode-7 and 16-color OBJ PNGs.");

        byte[] map = ReadMaps(mapJson);
        byte[] characters = SnesMode7TileEncoder.Encode(floorImage.Pixels, floorImage.Size);
        byte[] objectCharacters = SnesPlanarTileEncoder.Encode(objects.Pixels,
            objects.Size, 4);
        if (characters.Length != CeresFlightArtworkFormat.FloorByteCount ||
            objectCharacters.Length != CeresFlightArtworkFormat.ObjectByteCount)
            throw new InvalidDataException("Ceres flight PNGs compile to unexpected DMA lengths.");
        return new CeresFlightCharacters(characters, map, objectCharacters,
            palette,
            CeresFlightSpritePresentation.Load(spritesJson),
            CeresFlightActorLayout.Load(actorsJson));
    }

    /// <summary>The characters <paramref name="flight"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The flight artwork was not read from installed artwork.</exception>
    public static CeresFlightCharacters Of(CeresFlightArtworkCatalog flight) => flight as CeresFlightCharacters
        ?? throw new InvalidDataException("The Ceres flight artwork was not read from installed artwork.");
}
