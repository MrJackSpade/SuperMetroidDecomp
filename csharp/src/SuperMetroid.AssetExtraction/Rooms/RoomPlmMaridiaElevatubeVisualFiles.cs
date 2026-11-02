using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable Maridia elevatube PLM visual block, and the editable identity that names it.</summary>
public static class RoomPlmMaridiaElevatubeVisualFiles
{
    /// <summary>Family-relative stock/override JSON filename for the Maridia elevatube's single visual block, separate from Kraid's use of the same native draw.</summary>
    public const string VisualFileName = "maridia-elevatube.json";

    /// <summary>Checks the one-block elevatube draw at $84:9367 and exports its metatile/flip appearance without changing the solid word, sixteen-update wait, or sound.</summary>
    /// <param name="bus">Cartridge source whose native run count, $8180 terrain, and terminal offset are checked against compiled definitions.</param>
    /// <param name="directory">Family directory, created if absent; receives new <see cref="VisualFileName"/> and <c>manifest.json</c> files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank importer-supplied provenance hash; not recomputed here and required to identify the supported cartridge on stock load.</param>
    /// <remarks>Existing outputs are not overwritten; the separate JSON and manifest writes are not rolled back together if a later write fails.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">The native draw differs from its compiled shape, word, or offset.</exception>
    /// <exception cref="IOException">An output already exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Maridia elevatube",
            AllDraws, DrawVisualId);

    /// <summary>Validates the installed elevatube stock image and selects an optional one-block visual replacement.</summary>
    /// <param name="stockDirectory">Family directory containing stock JSON and its required version-one manifest.</param>
    /// <param name="overrideDirectory">Optional family directory; null or an absent override JSON selects validated stock.</param>
    /// <returns>A ROM-independent catalog for <c>elevatube-block</c>; Kraid appearance and elevatube mechanics remain separate.</returns>
    /// <remarks>Stock must match supported-cartridge provenance, its byte hash, and compiled appearance before overrides are considered. Override JSON needs the sole known identity and one twelve-bit appearance, but no manifest.</remarks>
    /// <exception cref="InvalidDataException">Stock integrity/provenance, JSON version, identity, coverage, block count, or visual-only bits are invalid.</exception>
    /// <exception cref="IOException">Required stock or selected override files cannot be read.</exception>
    public static InstalledRoomPlmMaridiaElevatubeVisuals Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            new RoomPlmVisualFamily<InstalledRoomPlmMaridiaElevatubeVisuals>(VisualFileName, "Maridia elevatube", AllDraws, DrawVisualId, entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks))), (catalog, draw, run, block) => catalog.GetAppearance(draw, run, block)));

    /// <summary>Runs the elevatube stock checks from <see cref="Load"/> with no override, cartridge reads, or writes.</summary>
    /// <param name="directory">Family directory containing elevatube stock JSON and manifest.</param>
    /// <exception cref="InvalidDataException">Stock provenance, integrity, schema, or compiled appearance checks fail.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

    /// <summary>Stable asset identity assigned to the elevatube's one-block artwork.</summary>
    internal const string VisualId = "elevatube-block";

    /// <summary>Validates the elevatube's single visual block and builds the visuals without changing its terrain, sixteen-update hold, sound, or deletion.</summary>
    /// <param name="entries">Exactly one entry with the compiled elevatube identity and one appearance.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">The sequence does not contain exactly one valid single-block elevatube entry.</exception>
    internal static InstalledRoomPlmMaridiaElevatubeVisuals Create(IEnumerable<RoomPlmVisualEntry> entries)
    {
        Ensure.NotNull(entries);
        RoomPlmVisualEntry[] selected = entries.ToArray();
        if (selected.Length != 1 || selected[0] is null ||
            !TryGetDrawByVisualId(selected[0].Id, out _) ||
            selected[0].Blocks is not { Length: 1 } blocks)
            throw new InvalidDataException(
                "Maridia elevatube visuals require exactly one visual block.");
        return new(blocks[0]);
    }

    /// <summary>The elevatube's one-block draw list, built from the block the game writes.</summary>
    internal static RoomPlmShotBlockDrawDefinitions.DrawList BlockDrawList =>
        RoomPlmDrawListExport.SingleRun(MaridiaElevatubePlmDefinitions.BlockDraw, PlmRunDirection.Horizontal,
            [MaridiaElevatubePlmDefinitions.Terrain]);

    /// <summary>The sole elevatube draw list.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> AllDraws => [BlockDrawList];

    /// <summary>Maps the elevatube draw to its asset identity and rejects unrelated draws.</summary>
    internal static string DrawVisualId(PlmDraw request) => request == MaridiaElevatubePlmDefinitions.BlockDraw
        ? VisualId
        : throw new InvalidDataException($"Maridia elevatube draw {request} has no visual ID.");

    /// <summary>Resolves the extracted elevatube artwork by its stable asset identity.</summary>
    internal static bool TryGetDrawByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList draw) =>
        RoomPlmDrawListExport.FindByVisualId([BlockDrawList], _ => VisualId, id, out draw);
}
