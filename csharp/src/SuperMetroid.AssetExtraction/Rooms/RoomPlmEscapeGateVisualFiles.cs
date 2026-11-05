using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable visual references for Mother Brain's escape gate.</summary>
public static class RoomPlmEscapeGateVisualFiles
{
    /// <summary>JSON filename for open, half-closed, and closed Mother Brain escape-gate appearances.</summary>
    public const string VisualFileName = "escape-gate.json";

    /// <summary>Verifies the three native four-block gate layouts and creates visual-only JSON plus its provenance and digest manifest.</summary>
    /// <param name="bus">Cartridge source for bank-$84 comparisons of decoded gate terrain blocks and four-cell vertical geometry.</param>
    /// <param name="directory">Destination directory, created if absent; the visual JSON and manifest must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the manifest; subsequent loading requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">Native draw contents or geometry differ from the compiled escape-gate definitions.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Escape-gate", All,
            VisualId);

    /// <summary>Validates the installed Escape-gate manifest's provenance and loads the installed Escape-gate JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and Escape-gate JSON.</param>
    /// <returns>The three-frame gate art; even visually open cells retain their compiled solid collision.</returns>
    /// <exception cref="InvalidDataException">The manifest provenance or format is incompatible, or the entries violate frame coverage.</exception>
    public static RoomPlmEscapeGateVisualCatalog Load(string directory) =>
        RoomPlmDoorVisualFileCodec.Load(directory,
            new RoomPlmVisualFamily<RoomPlmEscapeGateVisualCatalog>(VisualFileName, "Escape-gate", entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>Validates all three escape-gate frames and builds the visuals from the appearances that differ from the compiled frames, without replacing terrain blocks, animation timing, or door handoff.</summary>
    /// <remarks>Even the visually open frame retains compiled solid collision; an artwork edit cannot change that behavior.</remarks>
    /// <param name="entries">Exactly one four-block, visual-only entry for each compiled gate frame.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves frame coverage incomplete.</exception>
    internal static RoomPlmEscapeGateVisualCatalog Create(IEnumerable<RoomPlmVisualEntry> entries) =>
        new(RoomPlmVisualOverrideReader.Flattened(entries, "Escape-gate", TryGetByVisualId, All.Count()));

    /// <summary>Builds the open, half-closed, and closed gate images' draw lists in order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (MotherBrainEscapeGateDraw frame in Enum.GetValues<MotherBrainEscapeGateDraw>())
            {
                TryGet(PlmDraw.Of(frame), out var draw);
                yield return draw;
            }
        }
    }

    /// <summary>Builds the four-cell draw list of a known gate draw from its cells.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        if (!MotherBrainEscapeGateDraws.TryDescribe(request, out var draw))
        {
            list = default;
            return false;
        }
        var blocks = new RoomTerrainBlock[4];
        for (int row = 0; row < blocks.Length; row++) blocks[row] = draw.BlockAt(row);
        list = RoomPlmDrawListExport.SingleRun(request, PlmRunDirection.Vertical, blocks);
        return true;
    }

    /// <summary>Maps a gate draw to its stable authored visual identifier.</summary>
    internal static string VisualId(PlmDraw request) =>
        RoomPlmVisualIdDefinitions.MotherBrainEscapeGate.TryGetValue(
            PlmDraw.FrameOf<MotherBrainEscapeGateDraw>(request, "Mother Brain escape-gate draw"), out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined Mother Brain escape-gate draw {request}.");

    /// <summary>Finds a gate artwork draw list by its exact, case-sensitive visual identifier.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmDrawListExport.FindByVisualId(All, VisualId, id, out list);
}
