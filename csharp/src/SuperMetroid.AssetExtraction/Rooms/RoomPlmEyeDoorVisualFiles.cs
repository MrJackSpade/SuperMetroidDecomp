using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge-checked, strictly visual-only installed eye-door artwork, and the editable identities that name it.</summary>
public static class RoomPlmEyeDoorVisualFiles
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
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "eye-door", Editable, VisualId);

    /// <summary>Validates the installed eye-door manifest's provenance and loads the installed eye-door JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and eye-door JSON.</param>
    /// <returns>The component art, with the opening-clear alias mirrored from the shared clearing frame and native door mechanics unchanged.</returns>
    /// <exception cref="InvalidDataException">The manifest provenance or format is incompatible, or the entries violate editable coverage or visual-word rules.</exception>
    public static InstalledRoomPlmEyeDoorVisuals Load(string directory) =>
        RoomPlmDoorVisualFileCodec.Load(directory,
            new RoomPlmVisualFamily<InstalledRoomPlmEyeDoorVisuals>(VisualFileName, "eye-door", Editable, VisualId, entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks))), (catalog, draw, _, block) => catalog.GetAppearance(draw, block)));

    /// <summary>Validates all twenty-three editable eye-door frames and builds the visuals from the appearances that differ from the compiled components.</summary>
    /// <remarks>The mirrored opening clear shares the clearing frame's artwork rather than requiring a separate authored entry.</remarks>
    /// <param name="entries">One visual-only entry per compiled editable identity, retaining each eye or component frame's draw shape.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves editable frame coverage incomplete.</exception>
    internal static InstalledRoomPlmEyeDoorVisuals Create(IEnumerable<RoomPlmVisualEntry> entries) =>
        new(RoomPlmVisualOverrideReader.Flattened(entries, "Eye-door", TryGetByVisualId, Editable.Count()));

    /// <summary>Enumerates both sides' clear, eye, middle, and bottom draws, left side first.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (bool left in new[] { true, false })
            {
                TryGet(PlmDraw.Of(new EyeDoorDraws.Draw(EyeDoorDraws.Component.Clear, 0, left)), out var clear);
                yield return clear;
                foreach (EyeDoorDraws.Component part in new[] { EyeDoorDraws.Component.Eye, EyeDoorDraws.Component.Middle, EyeDoorDraws.Component.Bottom })
                    for (int frame = 0; frame < EyeDoorPlmDrawDefinitions.FrameCounts[part]; frame++)
                    {
                        TryGet(PlmDraw.Of(new EyeDoorDraws.Draw(part, frame, left)), out var value);
                        yield return value;
                    }
            }
        }
    }

    /// <summary>The mirrored clear shares one visual identity, keeping the installed file at 23 frames.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> Editable =>
        All.Where(draw => draw.Draw != EyeDoorPlmDrawDefinitions.MirroredOpeningClear);

    /// <summary>Builds the one-run draw list of a recognized eye-door draw from its cells.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        if (!EyeDoorDraws.TryDescribe(request, out var draw)) { list = default; return false; }
        var blocks = new RoomTerrainBlock[draw.BlockCount];
        for (int cell = 0; cell < blocks.Length; cell++) blocks[cell] = draw.BlockAt(cell);
        list = RoomPlmDrawListExport.SingleRun(request, draw.Vertical ? PlmRunDirection.Vertical : PlmRunDirection.Horizontal, blocks);
        return true;
    }

    /// <summary>Gets a stable side, component, and frame identifier for an editable draw.</summary>
    internal static string VisualId(PlmDraw request)
    {
        if (!EyeDoorDraws.TryDescribe(request, out var draw) || request == EyeDoorPlmDrawDefinitions.MirroredOpeningClear)
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
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmDrawListExport.FindByVisualId(Editable, VisualId, id, out list);
}
