using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge-checked stock and strict visual-only n00b-tube overrides.</summary>
public static class RoomPlmNoobTubeVisualFiles
{
    /// <summary>Editable n00b-tube visual JSON filename.</summary>
    public const string VisualFileName = "noob-tube.json";

    /// <summary>Verifies every native n00b-tube draw and creates stock JSON plus the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "n00b-tube", All,
            VisualId);

    /// <summary>Validates stock provenance and compiled parity, then selects an optional editable n00b-tube override without cartridge access.</summary>
    /// <param name="stockDirectory">Installed stock directory containing the shared manifest and n00b-tube JSON.</param>
    /// <param name="overrideDirectory">Optional directory whose n00b-tube JSON is selected when present.</param>
    /// <returns>The immutable selected n00b-tube visual catalog.</returns>
    public static InstalledRoomPlmNoobTubeVisuals Load(string stockDirectory,
        string? overrideDirectory = null) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            new RoomPlmVisualFamily<InstalledRoomPlmNoobTubeVisuals>(VisualFileName, "n00b-tube", All, VisualId, entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks))), (catalog, draw, run, block) => catalog.GetAppearance(draw, run, block)));

    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled n00b-tube visuals.</summary>
    /// <param name="stockDirectory">Stock n00b-tube directory; overrides are ignored.</param>
    public static void ValidateStock(string stockDirectory) => Load(stockDirectory);

    /// <summary>Validates all seven n00b-tube layouts and builds the visuals from the appearances that differ from the compiled layouts, without changing power-bomb gating, debris, event timing, water physics, or physical draw geometry.</summary>
    /// <param name="entries">One visual-only entry per compiled layout, retaining the full flattened count of its single-block or twelve-block runs.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    internal static InstalledRoomPlmNoobTubeVisuals Create(IEnumerable<RoomPlmVisualEntry> entries) =>
        new(RoomPlmVisualOverrideReader.Flattened(entries, "N00b tube", TryGetByVisualId, All.Count()));

    /// <summary>Enumerates tube states in intact-to-cleared and broken presentation order.</summary>
    private static IEnumerable<NoobTubeDraw> Draws()
    {
        yield return NoobTubeDraw.Intact;
        yield return NoobTubeDraw.Damaged;
        yield return NoobTubeDraw.Opened;
        yield return NoobTubeDraw.Cleared;
        yield return NoobTubeDraw.BrokenLate;
        yield return NoobTubeDraw.OpenedRows;
        yield return NoobTubeDraw.BrokenFull;
    }

    /// <summary>Builds the draw list of a tube layout from its computed cells.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        bool found = NoobTubePlmDraws.TryDescribe(request, out NoobTubePlmDraws.Draw draw);
        list = found
            ? RoomPlmDrawListExport.Runs(request, draw.RunCount, draw.BlockCount, draw.BlockAt,
                RoomPlmDrawListExport.Horizontal, static _ => 0, draw.NextY)
            : default;
        return found;
    }

    /// <summary>Converts a validated tube draw to its artwork catalog ID.</summary>
    internal static string VisualId(PlmDraw request) =>
        VisualId(PlmDraw.FrameOf<NoobTubeDraw>(request, "n00b-tube draw with a visual ID"));

    /// <summary>Names the typed intact, damaged, opened, cleared, or broken presentation state.</summary>
    internal static string VisualId(NoobTubeDraw frame) => frame switch
    {
        NoobTubeDraw.Intact => "intact",
        NoobTubeDraw.Damaged => "damaged",
        NoobTubeDraw.Opened => "opened",
        NoobTubeDraw.Cleared => "cleared",
        NoobTubeDraw.BrokenLate => "broken-late",
        NoobTubeDraw.OpenedRows => "opened-rows",
        NoobTubeDraw.BrokenFull => "broken-full",
        _ => throw new InvalidOperationException($"Undefined NoobTubeDraw {frame}."),
    };

    /// <summary>Finds and builds the tube layout corresponding to an artwork catalog ID.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (NoobTubeDraw frame in Draws())
            if (string.Equals(id, VisualId(frame), StringComparison.Ordinal)) return TryGet(PlmDraw.Of(frame), out list);
        list = default;
        return false;
    }

    /// <summary>Builds each known tube layout's draw list.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (NoobTubeDraw frame in Draws())
            {
                TryGet(PlmDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }
}
