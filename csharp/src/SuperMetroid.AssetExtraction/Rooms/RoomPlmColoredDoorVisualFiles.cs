using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable yellow, green, and red door-cap visual references, and the editable identities that name them.</summary>
public static class RoomPlmColoredDoorVisualFiles
{
    /// <summary>JSON filename for yellow, green, and red door-cap visual frames.</summary>
    public const string VisualFileName = "colored-doors.json";

    /// <summary>Verifies all forty-eight native colored-door frames and creates four-block visual entries plus their provenance and digest manifest.</summary>
    /// <param name="bus">Cartridge source for bank-$84 comparisons of door-cap block and horizontal or vertical run geometry.</param>
    /// <param name="directory">Destination directory, created if absent; the visual JSON and manifest must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the manifest; subsequent loading requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">Native draw contents or geometry differ from the compiled colored-door definitions.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Colored-door", All, VisualId);

    /// <summary>Validates stock colored-door art against its manifest and compiled frames, then selects a complete optional visual-only override without cartridge access.</summary>
    /// <param name="stockDirectory">Stock directory containing the colored-door JSON and its provenance/digest manifest.</param>
    /// <param name="overrideDirectory">Optional directory whose colored-door JSON replaces stock when present; an absent file retains stock.</param>
    /// <returns>The selected three-color, four-orientation cap artwork; collision and door-opening mechanics remain compiled.</returns>
    /// <exception cref="InvalidDataException">Stock provenance, digest, format, or compiled equality fails, or selected entries violate frame coverage or visual-word rules.</exception>
    public static InstalledRoomPlmColoredDoorVisuals Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            new RoomPlmVisualFamily<InstalledRoomPlmColoredDoorVisuals>(VisualFileName, "Colored-door", All, VisualId, entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks))), (catalog, draw, _, block) => catalog.GetAppearance(draw, block)));

    /// <summary>Validates complete coverage of the forty-eight colored-door frames and builds the visuals from the appearances that differ from the compiled caps.</summary>
    /// <param name="entries">Exactly one four-block, visual-only entry for each compiled color, orientation, and frame identity.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves frame coverage incomplete.</exception>
    internal static InstalledRoomPlmColoredDoorVisuals Create(IEnumerable<RoomPlmVisualEntry> entries) =>
        new(RoomPlmVisualOverrideReader.Flattened(entries, "Colored-door", TryGetByVisualId, All.Count()));

    /// <summary>Enumerates yellow, green, and red cap frames in their authored order.</summary>
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

    /// <summary>Builds the draw list of a recognized colored cap from its cells.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        bool found = ColoredDoorCaps.TryDescribe(request, out ColoredDoorCaps.Draw cap);
        list = found ? RoomPlmDrawListExport.Cap(request, cap.Vertical, cap.BlockAt) : default;
        return found;
    }

    /// <summary>Names a compiled cap frame by color, direction, and phase for editable artwork.</summary>
    internal static string VisualId(PlmDraw request)
    {
        if (!ColoredDoorCaps.TryDescribe(request, out var draw))
            throw new InvalidDataException($"Colored-door draw {request} has no visual ID.");
        string direction = DoorCapOrientations.VisualName(draw.Orientation);
        string color = draw.Color switch
        {
            ColoredDoorColor.Yellow => "yellow",
            ColoredDoorColor.Green => "green",
            ColoredDoorColor.Red => "red",
            _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorColor)} {draw.Color}."),
        };
        return $"{color}-{direction}-frame-{draw.Frame}";
    }

    /// <summary>Resolves an exact ordinal visual identity back to its compiled draw record.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmDrawListExport.FindByVisualId(Draws(), VisualId, TryGet, id, out list);

    /// <summary>Every colour's caps, each facing's four frames in turn.</summary>
    private static IEnumerable<PlmDraw> Draws()
    {
        foreach (ColoredDoorColor color in Enum.GetValues<ColoredDoorColor>())
            for (int orientation = 0; orientation < 4; orientation++)
                for (int frame = 0; frame < 4; frame++)
                    yield return PlmDraw.Of(new ColoredDoorCaps.Draw(color, DoorCapOrientations.FromTableIndex(orientation), frame));
    }
}
