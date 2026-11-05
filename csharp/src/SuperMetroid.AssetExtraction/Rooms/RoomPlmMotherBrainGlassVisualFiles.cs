using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge-checked, strictly visual-only installed glass artwork.</summary>
public static class RoomPlmMotherBrainGlassVisualFiles
{
    /// <summary>Family-relative installed JSON filename for Mother Brain's initial, damaged, shifted, shattering, and cleared glass PLM layouts.</summary>
    public const string VisualFileName = "mother-brain-glass.json";

    /// <summary>Checks native bank-$84 glass draw shapes, decoded terrain blocks, and offsets, then exports their block appearances in flattened run order.</summary>
    /// <param name="bus">Cartridge source for the compiled glass layouts; this extraction does not export shard sprites, hit thresholds, or event timing.</param>
    /// <param name="directory">Family directory, created if absent; receives new <see cref="VisualFileName"/> and <c>manifest.json</c> files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied provenance hash, recorded without recomputing it; a stock load requires the supported cartridge identity.</param>
    /// <remarks>Existing outputs are not overwritten. A failure writing the second file can leave the first file behind.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Native draw data does not match the compiled glass layouts.</exception>
    /// <exception cref="IOException">An output already exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Mother Brain glass", All,
            VisualId);

    /// <summary>Validates the installed Mother Brain glass manifest's provenance and loads the installed Mother Brain glass JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and Mother Brain glass JSON.</param>
    /// <returns>A ROM-independent glass metatile catalog; draw geometry, hit thresholds, shards, and event timing remain compiled.</returns>
    /// <exception cref="InvalidDataException">Manifest provenance, JSON version, frame coverage/identity, shape, or presentation-only word bits are invalid.</exception>
    /// <exception cref="IOException">The installed manifest or Mother Brain glass JSON cannot be read.</exception>
    public static RoomPlmMotherBrainGlassVisualCatalog Load(string directory) =>
        RoomPlmDoorVisualFileCodec.Load(directory,
            new RoomPlmVisualFamily<RoomPlmMotherBrainGlassVisualCatalog>(VisualFileName, "Mother Brain glass", entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>Validates all eleven glass layouts and builds the visuals from the appearances that differ from the compiled layouts, without changing damage thresholds, shards, event timing, terrain blocks, or signed draw offsets.</summary>
    /// <param name="entries">Exactly one visual-only entry per compiled glass identity, with the full flattened count of its run cells.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves frame coverage incomplete.</exception>
    internal static RoomPlmMotherBrainGlassVisualCatalog Create(IEnumerable<RoomPlmVisualEntry> entries) =>
        new(RoomPlmVisualOverrideReader.Flattened(entries, "Mother Brain glass", TryGetByVisualId, All.Count()));

    /// <summary>Builds the eleven glass layouts' draw lists in order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (MotherBrainGlassDraw frame in Enum.GetValues<MotherBrainGlassDraw>())
            {
                TryGet(PlmDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }

    /// <summary>Builds the draw list of a glass layout from its cells.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        bool found = MotherBrainGlassDraws.TryDescribe(request, out MotherBrainGlassDraws.Draw draw);
        list = found
            ? RoomPlmDrawListExport.Runs(request, draw.RunCount, draw.BlockCount, draw.BlockAt,
                run => draw.Vertical(run) ? PlmRunDirection.Vertical : PlmRunDirection.Horizontal, draw.NextX, draw.NextY)
            : default;
        return found;
    }

    /// <summary>Finds a glass layout by its stable artwork identifier.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (MotherBrainGlassDraw frame in Enum.GetValues<MotherBrainGlassDraw>())
            if (string.Equals(id, VisualId(PlmDraw.Of(frame)), StringComparison.Ordinal)) return TryGet(PlmDraw.Of(frame), out list);
        list = default;
        return false;
    }

    /// <summary>Maps a glass draw to its editable presentation frame name.</summary>
    internal static string VisualId(PlmDraw request) =>
        RoomPlmVisualIdDefinitions.MotherBrainGlass.TryGetValue(
            PlmDraw.FrameOf<MotherBrainGlassDraw>(request, "Mother Brain glass draw"), out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined Mother Brain glass draw {request}.");
}
