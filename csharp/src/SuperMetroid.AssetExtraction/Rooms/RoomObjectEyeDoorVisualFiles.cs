using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge-checked, strictly visual-only installed eye-door artwork, and the editable identities that name it.</summary>
public static class RoomObjectEyeDoorVisualFiles
{
    /// <summary>JSON filename for editable eye-door eye, middle, bottom, and shared clearing frames.</summary>
    public const string VisualFileName = "eye-doors.json";

    /// <summary>Verifies the twenty-three editable native eye-door layouts and creates visual-only JSON plus its provenance and digest manifest.</summary>
    /// <param name="bus">Cartridge source for bank-$84 comparisons of full component block and their one-, two-, or four-block run shapes.</param>
    /// <param name="directory">Destination directory, created if absent; the visual JSON and manifest must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the manifest; subsequent loading requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">Native draw contents or geometry differ from the compiled editable eye-door definitions.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomObjectDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "eye-door", Editable, VisualId);

    /// <summary>Validates the installed eye-door manifest's provenance and loads the installed eye-door JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and eye-door JSON.</param>
    /// <returns>The component art, with the opening-clear alias mirrored from the shared clearing frame and native door mechanics unchanged.</returns>
    /// <exception cref="InvalidDataException">The manifest provenance or format is incompatible, or the entries violate editable coverage or visual-word rules.</exception>
    public static InstalledRoomObjectEyeDoorVisuals Load(string directory) =>
        RoomObjectDoorVisualFileCodec.Load(directory,
            new RoomObjectVisualFamily<InstalledRoomObjectEyeDoorVisuals>(VisualFileName, "eye-door", entries => Create(entries.Select(entry =>
                new RoomObjectVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>Validates all twenty-three editable eye-door frames and builds the visuals from every frame's picture as installed.</summary>
    /// <remarks>The mirrored opening clear shares the clearing frame's artwork rather than requiring a separate authored entry.</remarks>
    /// <param name="entries">One visual-only entry per compiled editable identity, retaining each eye or component frame's draw shape.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves editable frame coverage incomplete.</exception>
    internal static InstalledRoomObjectEyeDoorVisuals Create(IEnumerable<RoomObjectVisualEntry> entries) =>
        new(RoomObjectFramePictureReader.Flattened(entries, "Eye-door", TryGetByVisualId, Editable.Count()));

    /// <summary>Enumerates both sides' clear, eye, middle, and bottom draws, left side first.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (bool left in new[] { true, false })
            {
                TryGet(RoomObjectDraw.Of(new EyeDoorDraws.Draw(EyeDoorDraws.Component.Clear, 0, left)), out var clear);
                yield return clear;
                foreach (EyeDoorDraws.Component part in new[] { EyeDoorDraws.Component.Eye, EyeDoorDraws.Component.Middle, EyeDoorDraws.Component.Bottom })
                    for (int frame = 0; frame < EyeDoorObjectDrawDefinitions.FrameCounts[part]; frame++)
                    {
                        TryGet(RoomObjectDraw.Of(new EyeDoorDraws.Draw(part, frame, left)), out var value);
                        yield return value;
                    }
            }
        }
    }

    /// <summary>The mirrored clear shares one visual identity, keeping the installed file at 23 frames.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> Editable =>
        All.Where(draw => draw.Draw != EyeDoorObjectDrawDefinitions.MirroredOpeningClear);

    /// <summary>Builds the one-run draw list of a recognized eye-door draw from its cells.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        if (!EyeDoorDraws.TryDescribe(request, out var draw)) { list = default; return false; }
        var blocks = new RoomTerrainBlock[draw.BlockCount];
        for (int cell = 0; cell < blocks.Length; cell++) blocks[cell] = draw.BlockAt(cell);
        list = RoomObjectDrawListExport.SingleRun(request, draw.Vertical ? RoomObjectRunDirection.Vertical : RoomObjectRunDirection.Horizontal, blocks);
        return true;
    }

    /// <summary>Gets a stable side, component, and frame identifier for an editable draw.</summary>
    internal static string VisualId(RoomObjectDraw request)
    {
        if (!EyeDoorDraws.TryDescribe(request, out var draw) || request == EyeDoorObjectDrawDefinitions.MirroredOpeningClear)
            throw new InvalidDataException($"Eye-door draw {request} has no visual ID.");
        string component = draw.Part switch
        {
            EyeDoorDraws.Component.Clear => "clear",
            EyeDoorDraws.Component.Eye => "eye",
            EyeDoorDraws.Component.Middle => "middle",
            EyeDoorDraws.Component.Bottom => "bottom",
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorDraws.Component)} {draw.Part}."),
        };
        return draw.Part == EyeDoorDraws.Component.Clear ? "left-eye-clear" : $"{(draw.Left ? "left" : "right")}-{component}-frame-{draw.Frame}";
    }

    /// <summary>Resolves an exact editable visual identifier to its draw.</summary>
    internal static bool TryGetByVisualId(string id, out RoomObjectShotBlockDrawDefinitions.DrawList list) =>
        RoomObjectDrawListExport.FindByVisualId(Editable, VisualId, id, out list);
}
