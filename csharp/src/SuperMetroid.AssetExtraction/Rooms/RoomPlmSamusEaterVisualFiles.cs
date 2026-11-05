using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Extracts and loads the eight replaceable Samus Eater block frames.</summary>
public static class RoomPlmSamusEaterVisualFiles
{
    /// <summary>Editable Samus Eater plant visual JSON filename.</summary>
    public const string VisualFileName = "samus-eater.json";

    /// <summary>Verifies every native Samus Eater plant draw and creates stock JSON plus the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Samus Eater plant",
            All,
            VisualId);

    /// <summary>Validates the installed Samus Eater plant manifest's provenance and loads the installed Samus Eater plant JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and Samus Eater plant JSON.</param>
    /// <returns>The immutable Samus Eater visual catalog.</returns>
    public static InstalledRoomPlmSamusEaterVisuals Load(string directory) =>
        RoomPlmDoorVisualFileCodec.Load(directory,
            new RoomPlmVisualFamily<InstalledRoomPlmSamusEaterVisuals>(VisualFileName, "Samus Eater plant", entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>Validates all eight three-run plant poses and builds the visuals from the authored appearances that differ from the compiled poses, without changing collision, run geometry, chewing timing, or Samus interaction.</summary>
    /// <param name="entries">Exactly one eight-block visual-only entry for each compiled floor and ceiling pose.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves coverage incomplete.</exception>
    internal static InstalledRoomPlmSamusEaterVisuals Create(IEnumerable<RoomPlmVisualEntry> entries) =>
        new(RoomPlmVisualOverrideReader.Flattened(entries, "Samus Eater", TryGetByVisualId, All.Count()));

    /// <summary>Maps a validated draw to the artwork catalog's orientation and phase ID.</summary>
    internal static string VisualId(PlmDraw request) =>
        VisualId(PlmDraw.FrameOf<SamusEaterDraw>(request, "Samus Eater draw with a visual ID"));

    /// <summary>Names a typed draw list by its floor or ceiling orientation and idle or chew phase.</summary>
    internal static string VisualId(SamusEaterDraw frame) => frame switch
    {
        SamusEaterDraw.FloorIdle => "floor-idle",
        SamusEaterDraw.FloorChew1 => "floor-chew-1",
        SamusEaterDraw.FloorChew2 => "floor-chew-2",
        SamusEaterDraw.FloorChew3 => "floor-chew-3",
        SamusEaterDraw.CeilingIdle => "ceiling-idle",
        SamusEaterDraw.CeilingChew1 => "ceiling-chew-1",
        SamusEaterDraw.CeilingChew2 => "ceiling-chew-2",
        SamusEaterDraw.CeilingChew3 => "ceiling-chew-3",
        _ => throw new InvalidOperationException($"Undefined SamusEaterDraw {frame}."),
    };

    /// <summary>Finds the layout matching an artwork catalog ID by searching the eight supported lists.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmDrawListExport.FindByVisualId(All, VisualId, id, out list);

    /// <summary>Builds the three runs of a supported plant draw from its phase and orientation.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        bool found = SamusEaterPlmDraws.TryDescribe(request, out SamusEaterPlmDraws.Draw draw);
        list = found
            ? RoomPlmDrawListExport.Runs(request, 3, SamusEaterPlmDraws.Draw.Count, draw.BlockAt,
                RoomPlmDrawListExport.Horizontal, SamusEaterPlmDraws.Draw.NextX, draw.NextY)
            : default;
        return found;
    }

    /// <summary>Builds the eight plant layouts' draw lists.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (SamusEaterDraw frame in Enum.GetValues<SamusEaterDraw>())
            {
                TryGet(PlmDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }
}
