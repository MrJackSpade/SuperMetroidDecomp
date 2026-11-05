using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable Mother Brain fake-death room tile art, and the editable identities that name it.</summary>
public static class RoomPlmMotherBrainFakeDeathVisualFiles
{
    /// <summary>Family-relative stock/override JSON filename for Mother Brain's fake-death room walls, escape door, background rows, and tube/ceiling clears.</summary>
    public const string VisualFileName = "mother-brain-fake-death.json";

    /// <summary>Verifies the bank-$84 fake-death room draw layouts and exports their flattened block appearances, leaving physical room mutations and encounter timing compiled.</summary>
    /// <param name="bus">Cartridge source for draws at $84:94A3–9716, including the cataloged unused background rows E and F.</param>
    /// <param name="directory">Family directory, created if absent; receives new <see cref="VisualFileName"/> and <c>manifest.json</c> files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied cartridge provenance hash; stock loading requires the supported identity rather than trusting arbitrary provenance.</param>
    /// <remarks>Only block appearances are serialized in native run/cell order. Create-new writes refuse existing outputs and are not an atomic JSON/manifest transaction.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Native run shapes, decoded terrain blocks, or offsets differ from compiled definitions.</exception>
    /// <exception cref="IOException">An output already exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Mother Brain fake-death room",
            All, VisualId);

    /// <summary>Validates stock fake-death room artwork before selecting a complete optional replacement of its visual layouts.</summary>
    /// <param name="stockDirectory">Family directory containing the required version-one manifest and stock JSON.</param>
    /// <param name="overrideDirectory">Optional family directory; null or absent override JSON uses validated stock.</param>
    /// <returns>A ROM-independent room-metatile catalog, not Mother Brain's sprite, palette, or glass artwork.</returns>
    /// <remarks>Stock provenance, hash, and compiled visual equality are checked even with overrides. Override JSON must cover every compiled identity/run shape with appearances; it needs no manifest and cannot change physical geometry.</remarks>
    /// <exception cref="InvalidDataException">Integrity, provenance, JSON version, frame coverage/identity, dimensions, or appearances are invalid.</exception>
    /// <exception cref="IOException">Required stock or selected override files cannot be read.</exception>
    public static RoomPlmMotherBrainFakeDeathVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            new RoomPlmVisualFamily<RoomPlmMotherBrainFakeDeathVisualCatalog>(VisualFileName, "Mother Brain fake-death room", entries => Create(
                entries.Select(entry =>
                    new RoomPlmVisualEntry(
                        entry.Id, entry.Blocks)))));

    /// <summary>Validates all twenty-two fake-death room layouts and builds the visuals from the appearances that differ from the compiled layouts, retaining collision kinds, signed draw offsets, and PLM timing.</summary>
    /// <param name="entries">One visual-only entry per compiled layout, with the exact flattened count of all its run cells.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    internal static RoomPlmMotherBrainFakeDeathVisualCatalog Create(IEnumerable<RoomPlmVisualEntry> entries) =>
        new(RoomPlmVisualOverrideReader.Flattened(entries, "Mother Brain fake-death", TryGetByVisualId, All.Count()));

    /// <summary>The wall, door, mural rows and tube-clear draw lists, in family order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All =>
        BoundaryDraws().Concat(BackgroundDraws()).Concat(RegularDraws());

    /// <summary>Validates a draw before assigning its editable artwork ID.</summary>
    internal static string VisualId(PlmDraw request) => VisualId(
        PlmDraw.FrameOf<MotherBrainFakeDeathDraw>(request, "Mother Brain fake-death draw with a visual ID"));

    /// <summary>Maps the closed draw domain to stable visual document names.</summary>
    internal static string VisualId(MotherBrainFakeDeathDraw frame) =>
        RoomPlmVisualIdDefinitions.MotherBrainFakeDeath.TryGetValue(frame, out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined MotherBrainFakeDeathDraw {frame}.");

    /// <summary>Finds a compiled physical draw by its case-sensitive editable visual ID.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList draw) =>
        RoomPlmDrawListExport.FindByVisualId(All, VisualId, id, out draw);

    /// <summary>The draw list of a fill row or tube clear, with vertical flags and continuation offsets.</summary>
    internal static bool TryGetRegular(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        bool found = MotherBrainFakeDeathDraws.TryDescribeRegular(request, out MotherBrainFakeDeathDraws.RegularDraw shape);
        draw = found
            ? RoomPlmDrawListExport.Runs(request, shape.RunCount, shape.Count, shape.BlockAt,
                run => shape.Vertical(run) ? PlmRunDirection.Vertical : PlmRunDirection.Horizontal, shape.NextX, static _ => 0)
            : default;
        return found;
    }

    /// <summary>The two unused fill rows and six tube clears, in the order the room's records list them.</summary>
    private static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> RegularDraws()
    {
        foreach (MotherBrainFakeDeathDraw frame in Enum.GetValues<MotherBrainFakeDeathDraw>())
        {
            if (TryGetRegular(PlmDraw.Of(frame), out var draw))
                yield return draw;
        }
    }

    /// <summary>The one thirteen-cell horizontal mural run of a mural row draw.</summary>
    internal static bool TryGetBackground(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        if (!MotherBrainFakeDeathDraws.TryDescribeBackground(request, out var row))
        {
            draw = default;
            return false;
        }
        var blocks = new RoomTerrainBlock[13];
        for (int column = 0; column < blocks.Length; column++) blocks[column] = row.BlockAt(column);
        draw = RoomPlmDrawListExport.SingleRun(request, PlmRunDirection.Horizontal, blocks);
        return true;
    }

    /// <summary>Enumerates all twelve mural rows, top to bottom.</summary>
    private static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> BackgroundDraws()
    {
        foreach (MotherBrainFakeDeathDraw row in MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRows)
        {
            TryGetBackground(PlmDraw.Of(row), out var draw);
            yield return draw;
        }
    }

    /// <summary>The two vertical runs of the wall-fill or escape-door draw.</summary>
    internal static bool TryGetBoundary(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        bool found = MotherBrainFakeDeathDraws.TryDescribeBoundary(request, out MotherBrainFakeDeathDraws.BoundaryDraw shape);
        draw = found
            ? RoomPlmDrawListExport.Runs(request, 2, shape.Count, shape.BlockAt,
                static _ => PlmRunDirection.Vertical, shape.NextX, shape.NextY)
            : default;
        return found;
    }

    /// <summary>Yields the wall-fill then escape-door draws.</summary>
    private static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> BoundaryDraws()
    {
        TryGetBoundary(PlmDraw.Of(MotherBrainFakeDeathDraw.FillWall), out var wall);
        yield return wall;
        TryGetBoundary(PlmDraw.Of(MotherBrainFakeDeathDraw.EscapeDoor), out var door);
        yield return door;
    }
}
