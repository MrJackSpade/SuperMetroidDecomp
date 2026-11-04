using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Bosses.Torizo.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable visual references for Bomb Torizo's hand PLM.</summary>
public static class RoomPlmBombTorizoHandVisualFiles
{
    /// <summary>The origin-relative continuations the exported draw lists record for a hand draw's strips.</summary>
    extension(BombTorizoHandDraws.Draw draw)
    {
        /// <summary>Gets the next strip's relative X origin or zero at list termination.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="run"/> is not one of the draw's strips.</exception>
        internal int NextX(int run)
        {
            if ((uint)run >= draw.RunCount) throw new ArgumentOutOfRangeException(nameof(run));
            return run + 1 == draw.RunCount ? 0 : draw.OriginX(run + 1);
        }

        /// <summary>Gets the next strip's relative Y origin or zero at list termination.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="run"/> is not one of the draw's strips.</exception>
        internal int NextY(int run)
        {
            if ((uint)run >= draw.RunCount) throw new ArgumentOutOfRangeException(nameof(run));
            return run + 1 == draw.RunCount ? 0 : draw.OriginY(run + 1);
        }
    }

    /// <summary>Editable Bomb Torizo hand PLM visual JSON filename.</summary>
    public const string VisualFileName = "bomb-torizo-hand.json";

    /// <summary>Verifies every native Bomb Torizo hand draw and creates stock JSON plus the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Bomb Torizo hand", All,
            VisualId);

    /// <summary>Validates stock provenance and compiled parity, then selects an optional editable Bomb Torizo hand override without cartridge access.</summary>
    /// <param name="stockDirectory">Installed stock directory containing the shared manifest and Bomb Torizo hand JSON.</param>
    /// <param name="overrideDirectory">Optional directory whose Bomb Torizo hand JSON is selected when present.</param>
    /// <returns>The immutable selected Bomb Torizo hand visual catalog.</returns>
    public static RoomPlmBombTorizoHandVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            new RoomPlmVisualFamily<RoomPlmBombTorizoHandVisualCatalog>(VisualFileName, "Bomb Torizo hand", All, VisualId, entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks))), (catalog, draw, run, block) => catalog.GetAppearance(draw, run, block)));

    /// <summary>Validates complete hand-frame coverage and builds the visuals from the authored appearances that differ from the compiled frames, leaving Bombs gating, collision, draw geometry and debris/music choreography fixed.</summary>
    /// <param name="entries">One entry for every compiled hand frame, with exactly the flattened block count and no physical collision bits.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats a frame, changes a compiled identity/shape, or leaves coverage incomplete.</exception>
    internal static RoomPlmBombTorizoHandVisualCatalog Create(IEnumerable<RoomPlmVisualEntry> entries) =>
        new(RoomPlmVisualOverrideReader.Flattened(entries, "Bomb Torizo hand", TryGetByVisualId, All.Count()));

    /// <summary>The intact and cleared hand draw lists, in authored order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            TryGet(PlmDraw.Of(BombTorizoHandDraw.Intact), out var intact);
            yield return intact;
            TryGet(PlmDraw.Of(BombTorizoHandDraw.Cleared), out var cleared);
            yield return cleared;
        }
    }

    /// <summary>Builds the draw list of a hand draw from its strips.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        bool found = BombTorizoHandDraws.TryDescribe(request, out BombTorizoHandDraws.Draw draw);
        list = found
            ? RoomPlmDrawListExport.Runs(request, draw.RunCount, draw.BlockCount, draw.BlockAt,
                RoomPlmDrawListExport.Horizontal, run => draw.NextX(run), run => draw.NextY(run))
            : default;
        return found;
    }

    /// <summary>Gets the stable artwork ID for a supported hand draw.</summary>
    internal static string VisualId(PlmDraw request) =>
        VisualId(PlmDraw.FrameOf<BombTorizoHandDraw>(request, "Bomb Torizo hand draw with a visual ID"));

    /// <summary>Maps an intact or cleared hand draw to its stable artwork ID.</summary>
    internal static string VisualId(BombTorizoHandDraw frame) =>
        RoomPlmVisualIdDefinitions.BombTorizoHand.TryGetValue(frame, out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined BombTorizoHandDraw {frame}.");

    /// <summary>Resolves an ordinal artwork ID to the corresponding hand draw list.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmDrawListExport.FindByVisualId(All, VisualId, id, out list);
}
