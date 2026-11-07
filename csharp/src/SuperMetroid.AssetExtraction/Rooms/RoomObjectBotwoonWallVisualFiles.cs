using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable Botwoon wall-clear visual blocks, and the editable identity that names them.</summary>
public static class RoomObjectBotwoonWallVisualFiles
{
    /// <summary>Editable Botwoon wall-clear visual JSON filename.</summary>
    public const string VisualFileName = "botwoon-wall.json";

    /// <summary>Verifies every native Botwoon wall draw and creates stock JSON plus the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomObjectDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Botwoon wall", All, VisualId);

    /// <summary>Validates the installed Botwoon wall manifest's provenance and loads the installed Botwoon wall JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and Botwoon wall JSON.</param>
    /// <returns>The immutable Botwoon wall visual catalog.</returns>
    public static InstalledRoomObjectBotwoonWallVisuals Load(string directory) =>
        RoomObjectDoorVisualFileCodec.Load(directory,
            new RoomObjectVisualFamily<InstalledRoomObjectBotwoonWallVisuals>(VisualFileName, "Botwoon wall", entries => Create(entries.Select(entry =>
                new RoomObjectVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>Validates the single nine-block wall-clear appearance and builds the visuals, keeping an owned copy only when it differs from the compiled blocks.</summary>
    /// <param name="entries">Exactly one known clear-frame entry containing nine visual-only appearances; crumble frames belong to the separate shot-block catalog.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">The entry count, identity, or block count is invalid.</exception>
    internal static InstalledRoomObjectBotwoonWallVisuals Create(IEnumerable<RoomObjectVisualEntry> entries)
    {
        Ensure.NotNull(entries);
        RoomObjectVisualEntry[] selected = entries.ToArray();
        if (selected.Length != 1 || selected[0] is null ||
            selected[0].Id != VisualId(BotwoonWallObjectDrawDefinitions.Clear) ||
            selected[0].Blocks is null || selected[0].Blocks.Length != BotwoonWallObjectDrawDefinitions.BlockCount)
            throw new InvalidDataException(
                "Botwoon wall visuals must contain exactly one nine-block clear frame.");
        bool stock = true;
        for (int index = 0; index < BotwoonWallObjectDrawDefinitions.BlockCount; index++)
            stock &= selected[0].Blocks[index] == BotwoonWallObjectDraws.BlockAt(index).Appearance;
        return new(stock ? null : selected[0].Blocks.ToArray());
    }

    /// <summary>Assigns the lone wall-clear draw its stable artwork key and rejects other draw identities.</summary>
    internal static string VisualId(RoomObjectDraw draw) => draw == BotwoonWallObjectDrawDefinitions.Clear
        ? "clear-wall"
        : throw new InvalidDataException($"Botwoon wall draw {draw} has no visual ID.");

    /// <summary>The nine-block vertical clear draw, built from the blocks the game writes.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            var blocks = new RoomTerrainBlock[BotwoonWallObjectDrawDefinitions.BlockCount];
            for (int index = 0; index < blocks.Length; index++) blocks[index] = BotwoonWallObjectDraws.BlockAt(index);
            yield return RoomObjectDrawListExport.SingleRun(BotwoonWallObjectDrawDefinitions.Clear, RoomObjectRunDirection.Vertical, blocks);
        }
    }
}
