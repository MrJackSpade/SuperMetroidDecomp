using SuperMetroid.AssetExtraction.Cartridge;
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
            return run + 1 < shape.RunCount ? run + 1 : 0;
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

    /// <summary>Validates the installed Crocomire arena manifest's provenance and loads the installed Crocomire arena JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and Crocomire arena JSON.</param>
    /// <returns>The bridge and wall art, independent of compiled solidity, crumble programs, and placement.</returns>
    /// <exception cref="InvalidDataException">The manifest provenance or format is incompatible, or the entries violate layout coverage.</exception>
    public static RoomPlmCrocomireVisualCatalog Load(string directory) =>
        RoomPlmDoorVisualFileCodec.Load(directory,
            new RoomPlmVisualFamily<RoomPlmCrocomireVisualCatalog>(VisualFileName, "Crocomire arena", entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks)))));

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
