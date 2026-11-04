using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable Crocomire arena bridge and wall tile art.</summary>
public static class RoomPlmCrocomireVisualFiles
{
    /// <summary>The run layout the exported draw lists record for a bridge or wall draw.</summary>
    extension(CrocomireArenaDraws.Draw shape)
    {
        /// <summary>Wall columns write down their column; the bridge writes along its row.</summary>
        internal PlmRunDirection Direction => shape.Wall ? PlmRunDirection.Vertical : PlmRunDirection.Horizontal;

        /// <summary>Offsets the next wall column from the draw origin rather than from the prior column.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="run"/> is not one of the draw's runs.</exception>
        internal int NextX(int run)
        {
            if ((uint)run >= shape.RunCount) throw new ArgumentOutOfRangeException(nameof(run));
            return run + 1 < shape.RunCount ? WordWrap.SignedLowByte(run + 1) : 0;
        }
    }

    /// <summary>JSON filename for Crocomire bridge and invisible-wall visual layouts.</summary>
    public const string VisualFileName = "crocomire-arena.json";

    /// <summary>Verifies all five native Crocomire arena layouts and creates flattened block-appearance JSON plus its provenance and digest manifest.</summary>
    /// <param name="bus">Cartridge source for bank-$84 comparisons of decoded bridge and wall terrain blocks, run shapes, and origin-relative offsets.</param>
    /// <param name="directory">Destination directory, created if absent; the visual JSON and manifest must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the manifest; subsequent loading requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">Native draw contents or geometry differ from the compiled arena definitions.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Crocomire arena",
            All,
            VisualId);

    /// <summary>Validates stock Crocomire art against its manifest and compiled layouts, then selects a complete optional visual-only override without cartridge access.</summary>
    /// <param name="stockDirectory">Stock directory containing the arena JSON and its provenance/digest manifest.</param>
    /// <param name="overrideDirectory">Optional directory whose arena JSON replaces stock when present; an absent file retains stock.</param>
    /// <returns>The selected bridge and wall art, independent of compiled solidity, crumble programs, and placement.</returns>
    /// <exception cref="InvalidDataException">Stock provenance, digest, format, or compiled equality fails, or selected entries violate layout coverage.</exception>
    public static RoomPlmCrocomireVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            new RoomPlmVisualFamily<RoomPlmCrocomireVisualCatalog>(VisualFileName, "Crocomire arena", All, VisualId, entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks))), (catalog, draw, run, block) => catalog.GetAppearance(draw, run, block)));

    /// <summary>Validates all five Crocomire arena layouts and builds the visuals from the appearances that differ from the compiled layouts, without changing terrain blocks, run geometry, or PLM programs.</summary>
    /// <param name="entries">Exactly one visual-only entry per compiled draw identity with its full flattened block count.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    internal static RoomPlmCrocomireVisualCatalog Create(IEnumerable<RoomPlmVisualEntry> entries) =>
        new(RoomPlmVisualOverrideReader.Flattened(entries, "Crocomire", TryGetByVisualId, All.Count()));

    /// <summary>The draw lists of every bridge and wall draw.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (CrocomireArenaDraw frame in Enum.GetValues<CrocomireArenaDraw>())
            {
                TryGet(PlmDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }

    /// <summary>Builds the draw list of one bridge or wall draw with its run offsets.</summary>
    internal static bool TryGet(PlmDraw draw, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        bool found = CrocomireArenaDraws.TryDescribe(draw, out CrocomireArenaDraws.Draw shape);
        list = found
            ? RoomPlmDrawListExport.Runs(draw, shape.RunCount, _ => shape.BlocksPerRun, shape.BlockAt,
                _ => shape.Direction, run => shape.NextX(run), static _ => 0)
            : default;
        return found;
    }

    /// <summary>Maps an arena draw to the published bridge or wall artwork key.</summary>
    internal static string VisualId(PlmDraw draw) =>
        RoomPlmVisualIdDefinitions.CrocomireArena[PlmDraw.FrameOf<CrocomireArenaDraw>(draw, "Crocomire arena")];

    /// <summary>Finds the physical bridge or wall layout for one editable visual identity.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList draw) =>
        RoomPlmDrawListExport.FindByVisualId(All, VisualId, id, out draw);
}
