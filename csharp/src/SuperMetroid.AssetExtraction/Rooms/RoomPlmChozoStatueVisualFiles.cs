using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable visual references for Chozo statue terrain PLMs.</summary>
public static class RoomPlmChozoStatueVisualFiles
{
    /// <summary>JSON filename for the cleared Chozo hand and Wrecked Ship slope-access appearances.</summary>
    public const string VisualFileName = "chozo-statues.json";

    /// <summary>Verifies the three native Chozo layouts and creates flattened block-appearance JSON plus its provenance and digest manifest.</summary>
    /// <param name="bus">Cartridge source for bank-$84 comparisons of decoded terrain blocks, run counts, and signed offsets.</param>
    /// <param name="directory">Destination directory, created if absent; the visual JSON and manifest must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the manifest; subsequent loading requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">Native draw contents or geometry differ from the compiled Chozo definitions.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Chozo statue", All,
            VisualId);

    /// <summary>Validates the installed Chozo statue manifest's provenance and loads the installed Chozo statue JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and Chozo statue JSON.</param>
    /// <returns>The hand and slope-access catalog, with native run geometry and collision unchanged.</returns>
    /// <exception cref="InvalidDataException">The manifest provenance or format is incompatible, or the entries violate layout coverage.</exception>
    public static RoomPlmChozoStatueVisualCatalog Load(string directory) =>
        RoomPlmDoorVisualFileCodec.Load(directory,
            new RoomPlmVisualFamily<RoomPlmChozoStatueVisualCatalog>(VisualFileName, "Chozo statue", entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>Validates complete Chozo-layout coverage and builds the visuals from the authored appearances that differ from the compiled layouts, without replacing terrain blocks or calculated run geometry.</summary>
    /// <param name="entries">One entry per compiled layout with its exact flattened block count and appearance payload.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, duplicates an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    internal static RoomPlmChozoStatueVisualCatalog Create(IEnumerable<RoomPlmVisualEntry> entries) =>
        new(RoomPlmVisualOverrideReader.Flattened(entries, "Chozo statue", TryGetByVisualId, All.Count()));

    /// <summary>Enumerates the three supported physical draw identities for artwork export.</summary>
    private static IEnumerable<PlmDraw> Draws()
    {
        yield return PlmDraw.Of(ChozoStatueDraw.LowerNorfairClearedHand);
        yield return PlmDraw.Of(ChozoStatueDraw.ClearSlopeAccess);
        yield return PlmDraw.Of(ChozoStatueDraw.BlockSlopeAccess);
    }

    /// <summary>The draw lists of the three statue draws.</summary>
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

    /// <summary>Builds the draw list of a statue draw from its cells.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        bool found = ChozoStatueDraws.TryDescribe(request, out ChozoStatueDraws.Draw draw);
        list = found
            ? RoomPlmDrawListExport.Runs(request, draw.RunCount, draw.BlockCount, draw.BlockAt,
                RoomPlmDrawListExport.Horizontal, draw.NextX, draw.NextY)
            : default;
        return found;
    }

    /// <summary>Finds a compiled statue draw by its published visual resource identity.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmDrawListExport.FindByVisualId(Draws(), VisualId, TryGet, id, out list);

    /// <summary>Names the cleared hand or blocked/clear slope layout without changing collision ownership.</summary>
    internal static string VisualId(PlmDraw request) =>
        RoomPlmVisualIdDefinitions.ChozoStatue.TryGetValue(
            PlmDraw.FrameOf<ChozoStatueDraw>(request, "Chozo statue draw with a visual ID"), out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined {nameof(ChozoStatueDraw)} {request}.");
}
