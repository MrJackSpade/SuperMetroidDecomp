using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The destruction's editable Mode 7 maps (#1467): two destruction views and the native clear-map slice, which the import
/// composes the Ceres floor's shots from; the game draws those installed floor pictures and reads none of these cells.
/// </summary>
public sealed class CeresDestructionMaps
{
    /// <summary>Retains the validated map cells.</summary>
    private CeresDestructionMaps(byte[] views) => Views = views;

    /// <summary>Two destruction views followed by the native clear-map slice, one map-lane byte per cell.</summary>
    public ReadOnlyMemory<byte> Views { get; }

    /// <summary>Loads the three-view Mode 7 map, validating its version, dimensions and eight-bit cells without reading cartridge data.</summary>
    /// <param name="json">Version-1 three-view Mode 7 map JSON from <see cref="CeresDestructionMapFormat.FileName"/>, consumed from its current position and left open.</param>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The document is malformed.</exception>
    public static CeresDestructionMaps Load(Stream json)
    {
        Ensure.NotNull(json);
        CeresDestructionMapDocument document = Read(json);
        var maps = new byte[CeresDestructionMapFormat.MapByteCount];
        for (int view = 0; view < document.Views.Length; view++)
            for (int cell = 0; cell < CeresDestructionMapFormat.CellsPerView; cell++)
                maps[view * CeresDestructionMapFormat.CellsPerView + cell] =
                    checked((byte)document.Views[view][cell]);
        return new CeresDestructionMaps(maps);
    }

    /// <summary>Serializes the three-view Mode 7 map as UTF-8 JSON, validating version, dimensions, cell counts, and eight-bit tile indexes before writing any bytes.</summary>
    /// <param name="json">Destination written at its current position and left open.</param>
    /// <param name="document">Ordered destruction-view and clear-view cells to serialize.</param>
    public static void Write(Stream json, CeresDestructionMapDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Read(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    /// <summary>Requires three ordered 32-by-24 Mode 7 slices whose cells fit the eight-bit map lane.</summary>
    private static CeresDestructionMapDocument Read(Stream json)
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
        if (document.Version != CeresDestructionMapFormat.Version ||
            document.Width != CeresDestructionMapFormat.MapWidth ||
            document.Height != CeresDestructionMapFormat.MapHeight ||
            document.Views is not { Length: CeresDestructionMapFormat.ViewCount } ||
            document.Views.Any(view => view is not
                { Length: CeresDestructionMapFormat.CellsPerView } ||
                view.Any(tile => (uint)tile > byte.MaxValue)))
            throw new InvalidDataException(
                "Ceres destruction requires three ordered 32x24 arrays of eight-bit tile indexes.");
        return document;
    }
}
