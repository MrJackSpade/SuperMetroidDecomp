using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable Botwoon wall-clear visual blocks, and the editable identity that names them.</summary>
public static class RoomPlmBotwoonWallVisualFiles
{
    /// <summary>Editable Botwoon wall-clear visual JSON filename.</summary>
    public const string VisualFileName = "botwoon-wall.json";

    /// <summary>Verifies every native Botwoon wall draw and creates stock JSON plus the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Botwoon wall", All, VisualId);

    /// <summary>Validates stock provenance and compiled parity, then selects an optional editable Botwoon wall override without cartridge access.</summary>
    /// <param name="stockDirectory">Installed stock directory containing the shared manifest and Botwoon wall JSON.</param>
    /// <param name="overrideDirectory">Optional directory whose Botwoon wall JSON is selected when present.</param>
    /// <returns>The immutable selected Botwoon wall visual catalog.</returns>
    public static InstalledRoomPlmBotwoonWallVisuals Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            new RoomPlmVisualFamily<InstalledRoomPlmBotwoonWallVisuals>(VisualFileName, "Botwoon wall", All, VisualId, entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks))), (catalog, draw, run, block) => catalog.GetAppearance(draw, run, block)));

    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled Botwoon wall visuals.</summary>
    /// <param name="directory">Stock Botwoon wall directory; overrides are ignored.</param>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

    /// <summary>Validates the single nine-block wall-clear appearance and builds the visuals, keeping an owned copy only when it differs from the compiled blocks.</summary>
    /// <param name="entries">Exactly one known clear-frame entry containing nine visual-only appearances; crumble frames belong to the separate shot-block catalog.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">The entry count, identity, or block count is invalid.</exception>
    internal static InstalledRoomPlmBotwoonWallVisuals Create(IEnumerable<RoomPlmVisualEntry> entries)
    {
        Ensure.NotNull(entries);
        RoomPlmVisualEntry[] selected = entries.ToArray();
        if (selected.Length != 1 || selected[0] is null ||
            selected[0].Id != VisualId(BotwoonWallPlmDrawDefinitions.Clear) ||
            selected[0].Blocks is null || selected[0].Blocks.Length != BotwoonWallPlmDrawDefinitions.BlockCount)
            throw new InvalidDataException(
                "Botwoon wall visuals must contain exactly one nine-block clear frame.");
        bool stock = true;
        for (int index = 0; index < BotwoonWallPlmDrawDefinitions.BlockCount; index++)
            stock &= selected[0].Blocks[index] == BotwoonWallPlmDraws.BlockAt(index).Appearance;
        return new(stock ? null : selected[0].Blocks.ToArray());
    }

    /// <summary>Assigns the lone wall-clear draw its stable artwork key and rejects other draw identities.</summary>
    internal static string VisualId(PlmDraw draw) => draw == BotwoonWallPlmDrawDefinitions.Clear
        ? "clear-wall"
        : throw new InvalidDataException($"Botwoon wall draw {draw} has no visual ID.");

    /// <summary>The nine-block vertical clear draw, built from the blocks the game writes.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            var blocks = new RoomTerrainBlock[BotwoonWallPlmDrawDefinitions.BlockCount];
            for (int index = 0; index < blocks.Length; index++) blocks[index] = BotwoonWallPlmDraws.BlockAt(index);
            yield return RoomPlmDrawListExport.SingleRun(BotwoonWallPlmDrawDefinitions.Clear, PlmRunDirection.Vertical, blocks);
        }
    }
}
