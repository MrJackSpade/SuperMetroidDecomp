using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable Mother Brain fake-death room tile art, and the editable identities that name it.</summary>
public static class RoomObjectMotherBrainFakeDeathVisualFiles
{
    /// <summary>Family-relative installed JSON filename for Mother Brain's fake-death room walls, escape door, background rows, and tube/ceiling clears.</summary>
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
        RoomObjectDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Mother Brain fake-death room",
            All, VisualId);

    /// <summary>Validates the installed Mother Brain fake-death room manifest's provenance and loads the installed Mother Brain fake-death room JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and Mother Brain fake-death room JSON.</param>
    /// <returns>A ROM-independent room-metatile catalog, not Mother Brain's sprite, palette, or glass artwork.</returns>
    /// <exception cref="InvalidDataException">Manifest provenance, JSON version, frame coverage/identity, dimensions, or appearances are invalid.</exception>
    /// <exception cref="IOException">The installed manifest or Mother Brain fake-death room JSON cannot be read.</exception>
    public static RoomObjectMotherBrainFakeDeathVisualCatalog Load(string directory) =>
        RoomObjectDoorVisualFileCodec.Load(directory,
            new RoomObjectVisualFamily<RoomObjectMotherBrainFakeDeathVisualCatalog>(VisualFileName, "Mother Brain fake-death room", entries => Create(
                entries.Select(entry =>
                    new RoomObjectVisualEntry(
                        entry.Id, entry.Blocks)))));

    /// <summary>Validates all twenty-two fake-death room layouts and builds the visuals from the appearances that differ from the compiled layouts, retaining collision kinds, signed draw offsets, and PLM timing.</summary>
    /// <param name="entries">One visual-only entry per compiled layout, with the exact flattened count of all its run cells.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    internal static RoomObjectMotherBrainFakeDeathVisualCatalog Create(IEnumerable<RoomObjectVisualEntry> entries) =>
        new(RoomObjectVisualOverrideReader.Flattened(entries, "Mother Brain fake-death", TryGetByVisualId, All.Count()));

    /// <summary>The wall, door, mural rows and tube-clear draw lists, in family order.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All =>
        BoundaryDraws().Concat(BackgroundDraws()).Concat(RegularDraws());

    /// <summary>Validates a draw before assigning its editable artwork ID.</summary>
    internal static string VisualId(RoomObjectDraw request) => VisualId(
        RoomObjectDraw.FrameOf<MotherBrainFakeDeathDraw>(request, "Mother Brain fake-death draw with a visual ID"));

    /// <summary>Maps the closed draw domain to stable visual document names.</summary>
    internal static string VisualId(MotherBrainFakeDeathDraw frame) =>
        RoomObjectVisualIdDefinitions.MotherBrainFakeDeath.TryGetValue(frame, out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined MotherBrainFakeDeathDraw {frame}.");

    /// <summary>Finds a compiled physical draw by its case-sensitive editable visual ID.</summary>
    internal static bool TryGetByVisualId(string id, out RoomObjectShotBlockDrawDefinitions.DrawList draw) =>
        RoomObjectDrawListExport.FindByVisualId(All, VisualId, id, out draw);

    /// <summary>The draw list of a fill row or tube clear, with vertical flags and continuation offsets.</summary>
    internal static bool TryGetRegular(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList draw)
    {
        bool found = MotherBrainFakeDeathDraws.TryDescribeRegular(request, out MotherBrainFakeDeathDraws.RegularDraw shape);
        draw = found
            ? RoomObjectDrawListExport.Runs(request, shape.RunCount, shape.Count, shape.BlockAt,
                run => shape.Vertical(run) ? RoomObjectRunDirection.Vertical : RoomObjectRunDirection.Horizontal, shape.Start)
            : default;
        return found;
    }

    /// <summary>The two unused fill rows and six tube clears, in the order the room's records list them.</summary>
    private static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> RegularDraws()
    {
        foreach (MotherBrainFakeDeathDraw frame in Enum.GetValues<MotherBrainFakeDeathDraw>())
        {
            if (TryGetRegular(RoomObjectDraw.Of(frame), out var draw))
                yield return draw;
        }
    }

    /// <summary>The one thirteen-cell horizontal mural run of a mural row draw.</summary>
    internal static bool TryGetBackground(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList draw)
    {
        if (!MotherBrainFakeDeathDraws.TryDescribeBackground(request, out var row))
        {
            draw = default;
            return false;
        }
        var blocks = new RoomTerrainBlock[13];
        for (int column = 0; column < blocks.Length; column++) blocks[column] = row.BlockAt(column);
        draw = RoomObjectDrawListExport.SingleRun(request, RoomObjectRunDirection.Horizontal, blocks);
        return true;
    }

    /// <summary>Enumerates all twelve mural rows, top to bottom.</summary>
    private static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> BackgroundDraws()
    {
        foreach (MotherBrainFakeDeathDraw row in MotherBrainFakeDeathObjectDrawDefinitions.BackgroundRows)
        {
            TryGetBackground(RoomObjectDraw.Of(row), out var draw);
            yield return draw;
        }
    }

    /// <summary>The two vertical runs of the wall-fill or escape-door draw.</summary>
    internal static bool TryGetBoundary(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList draw)
    {
        bool found = MotherBrainFakeDeathDraws.TryDescribeBoundary(request, out MotherBrainFakeDeathDraws.BoundaryDraw shape);
        draw = found
            ? RoomObjectDrawListExport.Runs(request, MotherBrainFakeDeathDraws.BoundaryDraw.RunCount, shape.Count, shape.BlockAt,
                static _ => RoomObjectRunDirection.Vertical, shape.Start)
            : default;
        return found;
    }

    /// <summary>Yields the wall-fill then escape-door draws.</summary>
    private static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> BoundaryDraws()
    {
        TryGetBoundary(RoomObjectDraw.Of(MotherBrainFakeDeathDraw.FillWall), out var wall);
        yield return wall;
        TryGetBoundary(RoomObjectDraw.Of(MotherBrainFakeDeathDraw.EscapeDoor), out var door);
        yield return door;
    }
}
