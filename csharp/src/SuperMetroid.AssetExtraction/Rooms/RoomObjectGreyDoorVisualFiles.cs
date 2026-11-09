using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable grey-door and shared clear-cap visual references, and the editable identities that name them.</summary>
public static class RoomObjectGreyDoorVisualFiles
{
    /// <summary>Family-relative JSON filename for twenty grey-door and shared clear-cap layouts.</summary>
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
        RoomObjectDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Grey-door", All, VisualId);

    /// <summary>Validates the installed Grey-door manifest's provenance and loads the installed Grey-door JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and Grey-door JSON.</param>
    /// <returns>A ROM-independent catalog for all twenty layouts, preserving compiled door mechanics and geometry.</returns>
    /// <exception cref="InvalidDataException">Manifest provenance, JSON format, frame coverage, shapes, or appearance bits are invalid.</exception>
    /// <exception cref="IOException">The installed manifest or Grey-door JSON cannot be read.</exception>
    public static InstalledRoomObjectGreyDoorVisuals Load(string directory) =>
        RoomObjectDoorVisualFileCodec.Load(directory,
            new RoomObjectVisualFamily<InstalledRoomObjectGreyDoorVisuals>(VisualFileName, "Grey-door", entries => Create(entries.Select(entry =>
                new RoomObjectVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>Validates all twenty clear/grey-door frames and builds the visuals from every frame's picture as installed.</summary>
    /// <param name="entries">Exactly one four-block visual-only entry for each compiled orientation and frame identity.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves coverage incomplete.</exception>
    internal static InstalledRoomObjectGreyDoorVisuals Create(IEnumerable<RoomObjectVisualEntry> entries) =>
        new(RoomObjectFramePictureReader.Flattened(entries, "Grey-door", TryGetByVisualId, All.Count()));

    /// <summary>Enumerates all twenty authored cap records for artwork import and export.</summary>
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

    /// <summary>Builds the draw list of a recognized grey or cleared cap from its cells.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        bool found = GreyDoorCaps.TryDescribe(request, out GreyDoorCaps.Draw cap);
        list = found ? RoomObjectDrawListExport.Cap(request, cap.Vertical, cap.BlockAt) : default;
        return found;
    }

    /// <summary>Names a supported clear or grey animation frame by direction and frame number.</summary>
    internal static string VisualId(RoomObjectDraw request)
    {
        if (!GreyDoorCaps.TryDescribe(request, out var draw))
            throw new InvalidDataException($"Grey-door draw {request} has no visual ID.");
        string direction = DoorCapOrientations.VisualName(draw.Orientation);
        return draw.Frame < 0 ? $"clear-{direction}" : $"grey-{direction}-frame-{draw.Frame}";
    }

    /// <summary>Resolves a stable artwork name to its compiled cap draw and shape.</summary>
    internal static bool TryGetByVisualId(string id, out RoomObjectShotBlockDrawDefinitions.DrawList list) =>
        RoomObjectDrawListExport.FindByVisualId(Draws(), VisualId, TryGet, id, out list);

    /// <summary>The four cleared openings, then each facing's four grey frames.</summary>
    private static IEnumerable<RoomObjectDraw> Draws()
    {
        for (int orientation = 0; orientation < 4; orientation++)
            yield return RoomObjectDraw.Of(new GreyDoorCaps.Draw(DoorCapOrientations.FromTableIndex(orientation), GreyDoorObjectDrawDefinitions.ClearFrame));
        for (int orientation = 0; orientation < 4; orientation++)
            for (int frame = 0; frame < 4; frame++)
                yield return RoomObjectDraw.Of(new GreyDoorCaps.Draw(DoorCapOrientations.FromTableIndex(orientation), frame));
    }
}
