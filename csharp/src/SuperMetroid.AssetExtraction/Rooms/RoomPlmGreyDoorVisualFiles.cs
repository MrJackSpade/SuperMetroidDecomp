using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable grey-door and shared clear-cap visual references, and the editable identities that name them.</summary>
public static class RoomPlmGreyDoorVisualFiles
{
    /// <summary>Family-relative JSON filename for twenty grey-door and shared clear-cap layouts, used in both stock and override directories.</summary>
    public const string VisualFileName = "grey-doors.json";

    /// <summary>Checks the bank-$84 grey-door/clear-cap draw shapes, terrain, and offsets, then exports their twelve-bit appearance in native run order.</summary>
    /// <param name="bus">Cartridge source supplying the compiled draw region $84:A677–A766.</param>
    /// <param name="directory">Family asset directory, created if absent; receives <see cref="VisualFileName"/> and <c>manifest.json</c> as new files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied cartridge hash recorded as provenance; loading requires the supported cartridge identity.</param>
    /// <remarks>Existing files are not overwritten. The JSON and manifest are separate writes, so a later failure can leave an earlier file in place.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">A cartridge draw differs from the compiled native layout.</exception>
    /// <exception cref="IOException">An output already exists or a filesystem read/write fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Grey-door", All, VisualId);

    /// <summary>Validates stock grey-door and shared clear-cap artwork before selecting an optional complete visual replacement.</summary>
    /// <param name="stockDirectory">Family directory containing the required version-one manifest and stock JSON.</param>
    /// <param name="overrideDirectory">Optional family directory; null or an absent <see cref="VisualFileName"/> uses validated stock.</param>
    /// <returns>A ROM-independent catalog for all twenty layouts, preserving compiled door mechanics and geometry.</returns>
    /// <remarks>Stock must match its byte hash, supported-cartridge provenance, and compiled appearance even when an override exists. Override JSON needs all frame identities/shapes and appearance, but no manifest.</remarks>
    /// <exception cref="InvalidDataException">Stock integrity, provenance, JSON format, frame coverage, shapes, or appearance bits are invalid.</exception>
    /// <exception cref="IOException">Required stock or selected override files cannot be read.</exception>
    public static InstalledRoomPlmGreyDoorVisuals Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            new RoomPlmVisualFamily<InstalledRoomPlmGreyDoorVisuals>(VisualFileName, "Grey-door", entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>Validates all twenty clear/grey-door frames and builds the visuals from the appearances that differ from the compiled caps.</summary>
    /// <param name="entries">Exactly one four-block visual-only entry for each compiled orientation and frame identity.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves coverage incomplete.</exception>
    internal static InstalledRoomPlmGreyDoorVisuals Create(IEnumerable<RoomPlmVisualEntry> entries) =>
        new(RoomPlmVisualOverrideReader.Flattened(entries, "Grey-door", TryGetByVisualId, All.Count()));

    /// <summary>Enumerates all twenty authored cap records for artwork import and export.</summary>
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

    /// <summary>Builds the draw list of a recognized grey or cleared cap from its cells.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        bool found = GreyDoorCaps.TryDescribe(request, out GreyDoorCaps.Draw cap);
        list = found ? RoomPlmDrawListExport.Cap(request, cap.Vertical, cap.BlockAt) : default;
        return found;
    }

    /// <summary>Names a supported clear or grey animation frame by direction and frame number.</summary>
    internal static string VisualId(PlmDraw request)
    {
        if (!GreyDoorCaps.TryDescribe(request, out var draw))
            throw new InvalidDataException($"Grey-door draw {request} has no visual ID.");
        string direction = DoorCapOrientations.VisualName(draw.Orientation);
        return draw.Frame < 0 ? $"clear-{direction}" : $"grey-{direction}-frame-{draw.Frame}";
    }

    /// <summary>Resolves a stable artwork name to its compiled cap draw and shape.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmDrawListExport.FindByVisualId(Draws(), VisualId, TryGet, id, out list);

    /// <summary>The four cleared openings, then each facing's four grey frames.</summary>
    private static IEnumerable<PlmDraw> Draws()
    {
        for (int orientation = 0; orientation < 4; orientation++)
            yield return PlmDraw.Of(new GreyDoorCaps.Draw(DoorCapOrientations.FromTableIndex(orientation), GreyDoorPlmDrawDefinitions.ClearFrame));
        for (int orientation = 0; orientation < 4; orientation++)
            for (int frame = 0; frame < 4; frame++)
                yield return PlmDraw.Of(new GreyDoorCaps.Draw(DoorCapOrientations.FromTableIndex(orientation), frame));
    }
}
