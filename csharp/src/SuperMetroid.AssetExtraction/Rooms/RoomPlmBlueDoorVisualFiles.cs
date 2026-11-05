using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable blue-door cap visual block references, and the editable identities that name them.</summary>
public static class RoomPlmBlueDoorVisualFiles
{
    /// <summary>Editable blue-door cap and shared clear-frame JSON filename.</summary>
    public const string VisualFileName = "blue-doors.json";

    /// <summary>Verifies every editable native blue-door draw and creates stock JSON plus the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Blue-door", Editable, VisualId);

    /// <summary>Validates the installed Blue-door manifest's provenance and loads the installed Blue-door JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and Blue-door JSON.</param>
    /// <returns>The immutable blue-door visual catalog.</returns>
    public static InstalledRoomPlmBlueDoorVisuals Load(string directory) =>
        RoomPlmDoorVisualFileCodec.Load(directory,
            new RoomPlmVisualFamily<InstalledRoomPlmBlueDoorVisuals>(VisualFileName, "Blue-door", entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>Validates complete canonical blue-door frame coverage and builds the visuals from the appearances that differ from the compiled caps.</summary>
    /// <param name="entries">Exactly one entry for every editable frame, each retaining the four-block run geometry.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is null, changes an identity/shape, repeats a frame, or leaves coverage incomplete.</exception>
    internal static InstalledRoomPlmBlueDoorVisuals Create(IEnumerable<RoomPlmVisualEntry> entries) =>
        new(RoomPlmVisualOverrideReader.Flattened(entries, "Blue-door", TryGetByVisualId, Editable.Count()));

    /// <summary>Sixteen original artwork identities; the four physical aliases share frame zero.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> Editable
    {
        get
        {
            for (int orientation = 0; orientation < 4; orientation++)
                for (int frame = 0; frame < 4; frame++)
                {
                    TryGet(PlmDraw.Of(new BlueDoorCaps.Draw(DoorCapOrientations.FromTableIndex(orientation), frame)), out var draw);
                    yield return draw;
                }
        }
    }

    /// <summary>Builds the draw list of a recognized blue cap from its cells.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        bool found = BlueDoorCaps.TryDescribe(request, out BlueDoorCaps.Draw cap);
        list = found ? RoomPlmDrawListExport.Cap(request, cap.Vertical, cap.BlockAt) : default;
        return found;
    }

    /// <summary>Names an editable cap frame by direction and opening phase.</summary>
    internal static string VisualId(PlmDraw request)
    {
        if (!BlueDoorCaps.TryDescribe(request, out var draw))
            throw new InvalidDataException($"Blue-door draw {request} has no visual ID.");
        string direction = DoorCapOrientations.VisualName(draw.Orientation);
        return $"{direction}-frame-{Math.Max(draw.Frame, 0)}";
    }

    /// <summary>Finds a blue-door draw by its exact editable visual identity.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmDrawListExport.FindByVisualId(Editable, VisualId, id, out list);
}
