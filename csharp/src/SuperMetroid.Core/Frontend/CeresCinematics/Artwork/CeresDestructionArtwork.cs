using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Editable map and character transfers unique to the destruction of Ceres and
/// the following Zebes reveal. The Ceres character sheets and the first two map
/// slices are shared with <see cref="CeresFlightArtwork"/>.
/// </summary>
public sealed class CeresDestructionArtwork
{
    /// <summary>Retains validated Ceres map slices, Zebes reveal art, and actor placements.</summary>
    private CeresDestructionArtwork(byte[] ceresMaps,
        RoomBackgroundTilemapAtlas zebesMap, RoomCharacterAtlas zebesCharacters,
        CeresRevealActorPlacements revealActors, CeresDestructionActorPlacements destructionActors)
    {
        CeresMaps = ceresMaps;
        ZebesMap = zebesMap;
        ZebesCharacters = zebesCharacters;
        RevealActors = revealActors;
        DestructionActors = destructionActors;
    }

    /// <summary>Two destruction views followed by the native clear-map slice.</summary>
    public ReadOnlyMemory<byte> CeresMaps { get; }
    /// <summary>One compiled 32-by-32 ordinary BG tilemap page for the Zebes reveal, replacing the $0800-byte map imported from $97:8ADB and transferred by $8B:C6F1–$C70E.</summary>
    public RoomBackgroundTilemapAtlas ZebesMap { get; }
    /// <summary>Compiled $4000-byte 4bpp character sheet for the Zebes reveal, replacing artwork imported from $96:EC76 and loaded at VRAM byte $C000.</summary>
    public RoomCharacterAtlas ZebesCharacters { get; }
    /// <summary>Initial screen-pixel placements of the six Zebes-reveal actors; instruction lists, slide acceleration, deletion, and phase transitions remain compiled cinematic behavior.</summary>
    public CeresRevealActorPlacements RevealActors { get; }
    /// <summary>Initial screen-pixel placements of the three persistent destruction-background actors; movement and wrap behavior are not editable through this layout.</summary>
    public CeresDestructionActorPlacements DestructionActors { get; }

    /// <summary>Identity of all selected destruction/reveal maps, characters and placements.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("CeresDestructionArtworkCatalog", content =>
    {
        content.Append("ceres-maps", CeresMaps.Span);
        content.AppendTiles("zebes-map", ZebesMap.Tiles.Span);
        content.Append("zebes-characters", ZebesCharacters.Characters.Span);
        content.Append("reveal-actors", Convert.FromHexString(RevealActors.ContentIdentity));
        content.Append("destruction-actors", Convert.FromHexString(DestructionActors.ContentIdentity));
    });

    /// <summary>Loads the bounded destruction and Zebes-reveal presentation resources, validating each map, character sheet, and actor layout without reading cartridge data.</summary>
    /// <param name="ceresMapJson">Version-1 three-view Mode 7 map JSON from <c>ceres-destruction-mode7-maps.json</c>.</param>
    /// <param name="zebesMapJson">One-page BG tilemap JSON from <c>zebes-reveal-tilemap.json</c>.</param>
    /// <param name="zebesCharacters">The installed 4bpp characters of <c>zebes-reveal-characters.png</c>, $4000 bytes.</param>
    /// <param name="revealActorsJson">Six ordered initial Zebes-reveal actor placements.</param>
    /// <param name="destructionActorsJson">Three ordered initial destruction-background actor placements.</param>
    /// <returns>The compiled presentation; all input streams are consumed from their current positions and remain caller-owned.</returns>
    public static CeresDestructionArtwork Load(Stream ceresMapJson,
        Stream zebesMapJson, RoomCharacterAtlas zebesCharacters,
        Stream revealActorsJson, Stream destructionActorsJson)
    {
        Ensure.NotNull(ceresMapJson);
        Ensure.NotNull(zebesMapJson);
        Ensure.NotNull(zebesCharacters);
        Ensure.NotNull(revealActorsJson);
        Ensure.NotNull(destructionActorsJson);
        CeresDestructionMapDocument document = ReadMap(ceresMapJson);
        var maps = new byte[CeresDestructionArtworkFormat.MapByteCount];
        for (int view = 0; view < document.Views.Length; view++)
            for (int cell = 0; cell < CeresDestructionArtworkFormat.CellsPerView; cell++)
                maps[view * CeresDestructionArtworkFormat.CellsPerView + cell] =
                    checked((byte)document.Views[view][cell]);
        return new CeresDestructionArtwork(maps,
            RoomBackgroundTilemapAtlas.Load(zebesMapJson,
                CeresDestructionArtworkFormat.ZebesMapByteCount),
            zebesCharacters.Characters.Length == CeresDestructionArtworkFormat.ZebesCharacterByteCount ? zebesCharacters
                : throw new InvalidDataException($"Zebes reveal characters hold {zebesCharacters.Characters.Length} bytes; expected {CeresDestructionArtworkFormat.ZebesCharacterByteCount}."),
            CeresRevealActorPlacements.Load(revealActorsJson),
            CeresDestructionActorPlacements.Load(destructionActorsJson));
    }

    /// <summary>Serializes the three-view Mode 7 map as UTF-8 JSON, validating version, dimensions, cell counts, and eight-bit tile indexes before writing any bytes.</summary>
    /// <param name="json">Destination written at its current position and left open.</param>
    /// <param name="document">Ordered destruction-view and clear-view cells to serialize.</param>
    public static void WriteMap(Stream json, CeresDestructionMapDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = ReadMap(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    /// <summary>Requires three ordered 32-by-24 Mode 7 slices whose cells fit the eight-bit map lane.</summary>
    private static CeresDestructionMapDocument ReadMap(Stream json)
    {
        CeresDestructionMapDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CeresDestructionMapDocument>(json,
                ArtworkJson.Options)
                ?? throw new InvalidDataException("Ceres destruction map JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres destruction map JSON.", error);
        }
        if (document.Version != CeresDestructionArtworkFormat.Version ||
            document.Width != CeresDestructionArtworkFormat.MapWidth ||
            document.Height != CeresDestructionArtworkFormat.MapHeight ||
            document.Views is not { Length: CeresDestructionArtworkFormat.ViewCount } ||
            document.Views.Any(view => view is not
                { Length: CeresDestructionArtworkFormat.CellsPerView } ||
                view.Any(tile => (uint)tile > byte.MaxValue)))
            throw new InvalidDataException(
                "Ceres destruction requires three ordered 32x24 arrays of eight-bit tile indexes.");
        return document;
    }
}
