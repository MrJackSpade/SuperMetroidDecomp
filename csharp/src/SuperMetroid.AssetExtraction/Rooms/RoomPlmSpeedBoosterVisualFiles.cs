using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable Speed Booster bomb-reveal visual block, and the editable identity that names it.</summary>
public static class RoomPlmSpeedBoosterVisualFiles
{
    /// <summary>Editable Speed Booster bomb-reveal visual JSON filename.</summary>
    public const string VisualFileName = "speed-booster.json";

    /// <summary>Verifies the native Speed Booster bomb-reveal draw and creates stock JSON plus the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time bank-$84 comparison.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Speed Booster", All, VisualId);

    /// <summary>Validates the installed Speed Booster manifest's provenance and loads the installed Speed Booster JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and Speed Booster JSON.</param>
    /// <returns>The immutable Speed Booster visual catalog.</returns>
    public static InstalledRoomPlmSpeedBoosterVisuals Load(string directory) =>
        RoomPlmDoorVisualFileCodec.Load(directory,
            new RoomPlmVisualFamily<InstalledRoomPlmSpeedBoosterVisuals>(VisualFileName, "Speed Booster", entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>The artwork identity of the bomb reveal of one Speed Booster block.</summary>
    internal const string BombRevealVisualId = "bomb-reveal";

    /// <summary>Validates and captures the single bomb-reveal appearance, building the visuals while preserving compiled Speed Booster collision and timing.</summary>
    /// <param name="entries">Exactly one <c>bomb-reveal</c> entry containing one valid appearance; its value is captured, so later input-array edits do not affect the catalog.</param>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">Entry count, identity, or block count do not match the compiled reveal.</exception>
    internal static InstalledRoomPlmSpeedBoosterVisuals Create(IEnumerable<RoomPlmVisualEntry> entries)
    {
        Ensure.NotNull(entries);
        RoomPlmVisualEntry[] selected = entries.ToArray();
        if (selected.Length != 1 || selected[0] is null ||
            !TryGetByVisualId(selected[0].Id, out _) ||
            selected[0].Blocks is not { Length: 1 } blocks)
            throw new InvalidDataException(
                "Speed Booster visuals require exactly one bomb-reveal visual block.");
        return new(blocks[0]);
    }

    /// <summary>The one-block reveal's draw list, built from the block the game writes.</summary>
    internal static RoomPlmShotBlockDrawDefinitions.DrawList BombReveal =>
        RoomPlmDrawListExport.SingleRun(SpeedBoosterBlockPlmDrawDefinitions.BombRevealDraw, PlmRunDirection.Horizontal,
            [SpeedBoosterBlockPlmDrawDefinitions.BombRevealTerrain]);

    /// <summary>The sole bomb-reveal draw list.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All => [BombReveal];

    /// <summary>Accepts only the bomb-reveal draw and rejects unrelated PLM drawings.</summary>
    internal static string VisualId(PlmDraw request) => request == SpeedBoosterBlockPlmDrawDefinitions.BombRevealDraw
        ? BombRevealVisualId
        : throw new InvalidDataException($"Speed Booster draw {request} has no visual ID.");

    /// <summary>Resolves the one exported visual identity to its draw list.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmDrawListExport.FindByVisualId([BombReveal], _ => BombRevealVisualId, id, out list);
}
