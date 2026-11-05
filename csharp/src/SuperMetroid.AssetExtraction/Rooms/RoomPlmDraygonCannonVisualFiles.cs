using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Bosses.Draygon.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable visual references for reachable Draygon cannons.</summary>
public static class RoomPlmDraygonCannonVisualFiles
{
    /// <summary>JSON filename for reachable left- and right-facing Draygon cannon artwork.</summary>
    public const string VisualFileName = "draygon-cannons.json";

    /// <summary>Verifies the twelve reachable native cannon layouts and creates flattened shield/damaged-frame visual JSON plus its provenance and digest manifest.</summary>
    /// <param name="bus">Cartridge source for bank-$84 comparisons of decoded cannon terrain blocks and two- or three-run geometry.</param>
    /// <param name="directory">Destination directory, created if absent; the visual JSON and manifest must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the manifest; subsequent loading requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">Native draw contents or geometry differ from the compiled reachable cannon definitions.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Draygon cannon", All,
            VisualId);

    /// <summary>Validates the installed Draygon cannon manifest's provenance and loads the installed Draygon cannon JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and Draygon cannon JSON.</param>
    /// <returns>The reachable cannon art, leaving hit thresholds, physical collision, and control writes compiled.</returns>
    /// <exception cref="InvalidDataException">The manifest provenance or format is incompatible, or the entries violate frame coverage.</exception>
    public static RoomPlmDraygonCannonVisualCatalog Load(string directory) =>
        RoomPlmDoorVisualFileCodec.Load(directory,
            new RoomPlmVisualFamily<RoomPlmDraygonCannonVisualCatalog>(VisualFileName, "Draygon cannon", entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>Validates complete reachable cannon-frame coverage and builds the visuals from the appearances that differ from the compiled frames, without changing shield damage, hit thresholds, control writes, or draw geometry.</summary>
    /// <param name="entries">One visual-only entry per compiled frame with the exact flattened count of all its run cells.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves reachable frame coverage incomplete.</exception>
    internal static RoomPlmDraygonCannonVisualCatalog Create(IEnumerable<RoomPlmVisualEntry> entries) =>
        new(RoomPlmVisualOverrideReader.Flattened(entries, "Draygon cannon", TryGetByVisualId, All.Count()));

    /// <summary>Enumerates only the right and left cannon lists reached by the PLM programs.</summary>
    private static IEnumerable<PlmDraw> Draws()
    {
        foreach (DraygonCannonDraw draw in Enum.GetValues<DraygonCannonDraw>()) yield return PlmDraw.Of(draw);
    }

    /// <summary>Builds the draw lists of all reachable cannon frames.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (PlmDraw draw in Draws())
            {
                TryGet(draw, out var list);
                yield return list;
            }
        }
    }

    /// <summary>Builds the draw list of a cannon frame from its cells.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        bool found = DraygonCannonDraws.TryDescribe(request, out DraygonCannonDraws.Draw draw);
        list = found
            ? RoomPlmDrawListExport.Runs(request, draw.RunCount, draw.BlockCount, draw.BlockAt,
                RoomPlmDrawListExport.Horizontal, draw.NextX, draw.NextY)
            : default;
        return found;
    }

    /// <summary>Finds a reachable cannon draw list by its editable visual ID.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmDrawListExport.FindByVisualId(Draws(), VisualId, TryGet, id, out list);

    /// <summary>Maps a cannon draw to its facing, condition, and pose ID.</summary>
    internal static string VisualId(PlmDraw request) =>
        RoomPlmVisualIdDefinitions.DraygonCannon.TryGetValue(
            PlmDraw.FrameOf<DraygonCannonDraw>(request, "Draygon cannon draw with a visual ID"), out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined {nameof(DraygonCannonDraw)} {request}.");
}
