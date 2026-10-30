using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Editable maps and actor placements unique to the destruction of Ceres and the following Zebes reveal. The Ceres
/// character sheets and the first two map slices are shared with <see cref="CeresFlightArtwork"/>; the Zebes reveal's
/// tilemap and characters are the import's, which pictures the planet from them (#1467).
/// </summary>
public sealed class CeresDestructionArtwork
{
    /// <summary>Retains validated Ceres map slices and actor placements.</summary>
    private CeresDestructionArtwork(byte[] ceresMaps,
        CeresRevealActorPlacements revealActors, CeresDestructionActorPlacements destructionActors)
    {
        CeresMaps = ceresMaps;
        RevealActors = revealActors;
        DestructionActors = destructionActors;
    }

    /// <summary>Two destruction views followed by the native clear-map slice.</summary>
    public ReadOnlyMemory<byte> CeresMaps { get; }
    /// <summary>Initial screen-pixel placements of the six Zebes-reveal actors; instruction lists, slide acceleration, deletion, and phase transitions remain compiled cinematic behavior.</summary>
    public CeresRevealActorPlacements RevealActors { get; }
    /// <summary>Initial screen-pixel placements of the three persistent destruction-background actors; movement and wrap behavior are not editable through this layout.</summary>
    public CeresDestructionActorPlacements DestructionActors { get; }

    /// <summary>Loads the bounded destruction presentation resources, validating each map and actor layout without reading cartridge data.</summary>
    /// <param name="ceresMapJson">Version-1 three-view Mode 7 map JSON from <c>ceres-destruction-mode7-maps.json</c>.</param>
    /// <param name="revealActorsJson">Six ordered initial Zebes-reveal actor placements.</param>
    /// <param name="destructionActorsJson">Three ordered initial destruction-background actor placements.</param>
    /// <returns>The compiled presentation; all input streams are consumed from their current positions and remain caller-owned.</returns>
    public static CeresDestructionArtwork Load(Stream ceresMapJson,
        Stream revealActorsJson, Stream destructionActorsJson)
    {
        Ensure.NotNull(ceresMapJson);
        Ensure.NotNull(revealActorsJson);
        Ensure.NotNull(destructionActorsJson);
        CeresDestructionMapDocument document = ReadMap(ceresMapJson);
        var maps = new byte[CeresDestructionArtworkFormat.MapByteCount];
        for (int view = 0; view < document.Views.Length; view++)
            for (int cell = 0; cell < CeresDestructionArtworkFormat.CellsPerView; cell++)
                maps[view * CeresDestructionArtworkFormat.CellsPerView + cell] =
                    checked((byte)document.Views[view][cell]);
        return new CeresDestructionArtwork(maps,
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
