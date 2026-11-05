using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable Tourian entrance access-floor visuals.</summary>
public static class RoomPlmTourianAccessVisualFiles
{
    /// <summary>Editable Tourian entrance access-floor visual JSON filename.</summary>
    public const string VisualFileName = "tourian-access.json";

    /// <summary>Verifies every native Tourian access-floor draw and creates stock JSON plus the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Tourian access", All,
            VisualId);

    /// <summary>Validates the installed Tourian access manifest's provenance and loads the installed Tourian access JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and Tourian access JSON.</param>
    /// <returns>The immutable Tourian access-floor visual catalog.</returns>
    public static RoomPlmTourianAccessVisuals Load(string directory) =>
        RoomPlmDoorVisualFileCodec.Load(directory,
            new RoomPlmVisualFamily<RoomPlmTourianAccessVisuals>(VisualFileName, "Tourian access", entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>Validates all five Tourian access-floor layouts and builds the visuals from the appearances that differ from the compiled layouts.</summary>
    /// <param name="entries">The empty row, three visible crumble rows, and six-row clear exactly once, each with its compiled block count and appearances.</param>
    /// <remarks>Edited arrays are copied; stock-identical layouts use compiled appearance, so caller-array edits after construction cannot alter the catalog.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">Coverage, uniqueness, layout identity, or block-array dimensions are invalid.</exception>
    internal static RoomPlmTourianAccessVisuals Create(IEnumerable<RoomPlmVisualEntry> entries) =>
        new(RoomPlmVisualOverrideReader.Flattened(entries, "Tourian access", TryGetByVisualId, All.Count()));

    /// <summary>The draw lists of every Tourian access draw.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (TourianAccessDraw draw in Enum.GetValues<TourianAccessDraw>())
                yield return Compile(draw);
        }
    }

    /// <summary>Gets the stable artwork identity of a Tourian access draw.</summary>
    internal static string VisualId(PlmDraw draw) => VisualId(PlmDraw.FrameOf<TourianAccessDraw>(draw, "Tourian access"));

    /// <summary>Names the empty, three crumble, or six-row clear frame for visual files.</summary>
    internal static string VisualId(TourianAccessDraw draw) =>
        RoomPlmVisualIdDefinitions.TourianAccess.TryGetValue(draw, out string? id)
            ? id : throw new InvalidOperationException($"Undefined TourianAccessDraw {draw}.");

    /// <summary>Finds a compiled floor draw by its case-sensitive visual identity.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmDrawListExport.FindByVisualId(All, VisualId, id, out list);

    /// <summary>Builds the four-air-cell runs of a Tourian access draw.</summary>
    internal static RoomPlmShotBlockDrawDefinitions.DrawList Compile(TourianAccessDraw draw)
    {
        TourianAccessDraws.Describe(draw, out int rows, out RoomTerrainBlock block);
        return RoomPlmDrawListExport.Runs(PlmDraw.Of(draw), rows, static _ => 4, (_, _) => block,
            RoomPlmDrawListExport.Horizontal, static _ => 0, row => WordWrap.SignedLowByte(row + 1 < rows ? row + 1 : 0));
    }
}
