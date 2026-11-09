using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable visual references for Chozo statue terrain PLMs.</summary>
public static class RoomObjectChozoStatueVisualFiles
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
        RoomObjectDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Chozo statue", All,
            VisualId);

    /// <summary>Validates the installed Chozo statue manifest's provenance and loads the installed Chozo statue JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and Chozo statue JSON.</param>
    /// <returns>The hand and slope-access catalog, with native run geometry and collision unchanged.</returns>
    /// <exception cref="InvalidDataException">The manifest provenance or format is incompatible, or the entries violate layout coverage.</exception>
    public static RoomObjectChozoStatueVisualCatalog Load(string directory) =>
        RoomObjectDoorVisualFileCodec.Load(directory,
            new RoomObjectVisualFamily<RoomObjectChozoStatueVisualCatalog>(VisualFileName, "Chozo statue", entries => Create(entries.Select(entry =>
                new RoomObjectVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>Validates complete Chozo-layout coverage and builds the visuals from every frame's picture as installed, without replacing terrain blocks or calculated run geometry.</summary>
    /// <param name="entries">One entry per compiled layout with its exact flattened block count and appearance payload.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, duplicates an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    internal static RoomObjectChozoStatueVisualCatalog Create(IEnumerable<RoomObjectVisualEntry> entries) =>
        new(RoomObjectFramePictureReader.Flattened(entries, "Chozo statue", TryGetByVisualId, All.Count()));

    /// <summary>Enumerates the three supported physical draw identities for artwork export.</summary>
    private static IEnumerable<RoomObjectDraw> Draws()
    {
        yield return RoomObjectDraw.Of(ChozoStatueDraw.LowerNorfairClearedHand);
        yield return RoomObjectDraw.Of(ChozoStatueDraw.ClearSlopeAccess);
        yield return RoomObjectDraw.Of(ChozoStatueDraw.BlockSlopeAccess);
    }

    /// <summary>The draw lists of the three statue draws.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (RoomObjectDraw draw in Draws())
            {
                TryGet(draw, out var list);
                yield return list;
            }
        }
    }

    /// <summary>Builds the draw list of a statue draw from its cells.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        bool found = ChozoStatueDraws.TryDescribe(request, out ChozoStatueDraws.Draw draw);
        list = found
            ? RoomObjectDrawListExport.Strips(request, draw.RunCount, draw.BlockCount, draw.BlockAt, draw.Start)
            : default;
        return found;
    }

    /// <summary>Finds a compiled statue draw by its published visual resource identity.</summary>
    internal static bool TryGetByVisualId(string id, out RoomObjectShotBlockDrawDefinitions.DrawList list) =>
        RoomObjectDrawListExport.FindByVisualId(Draws(), VisualId, TryGet, id, out list);

    /// <summary>Names the cleared hand or blocked/clear slope layout without changing collision ownership.</summary>
    internal static string VisualId(RoomObjectDraw request) =>
        RoomObjectVisualIdDefinitions.ChozoStatue.TryGetValue(
            RoomObjectDraw.FrameOf<ChozoStatueDraw>(request, "Chozo statue draw with a visual ID"), out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined {nameof(ChozoStatueDraw)} {request}.");
}
