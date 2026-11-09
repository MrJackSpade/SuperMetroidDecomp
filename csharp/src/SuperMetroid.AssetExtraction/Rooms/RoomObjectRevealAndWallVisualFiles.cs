using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed pictures of the small reveals and walls room objects draw (#1557): the three bombed reveals, the cleared
/// passage of Crateria's main street escape, the opened wall of the old Tourian escape shaft and the four rescue-wall
/// frames of the escape animals, and the identities that name them.
/// </summary>
public static class RoomObjectRevealAndWallVisualFiles
{
    /// <summary>The installed JSON filename of the reveal and wall pictures.</summary>
    public const string VisualFileName = "reveals-and-walls.json";

    /// <summary>The family's name in diagnostics.</summary>
    private const string Family = "Reveal and wall";

    /// <summary>Verifies every native reveal and wall draw and pictures each from the cartridge, with the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time comparisons.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    /// <exception cref="InvalidDataException">A native draw differs from the game's shape, terrain or offsets.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomObjectDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, Family, All, VisualId);

    /// <summary>Validates the installed manifest's provenance and loads the installed reveal and wall pictures as the player left them, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and the reveal and wall JSON.</param>
    /// <returns>Every reveal and wall frame's picture.</returns>
    /// <exception cref="InvalidDataException">Manifest provenance, document version, identities, coverage or shapes are invalid.</exception>
    public static InstalledRoomObjectRevealAndWallVisuals Load(string directory) =>
        RoomObjectDoorVisualFileCodec.Load(directory,
            new RoomObjectVisualFamily<InstalledRoomObjectRevealAndWallVisuals>(VisualFileName, Family, entries => Create(entries.Select(entry =>
                new RoomObjectVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>Validates every reveal and wall frame and builds the visuals from every frame's picture as installed.</summary>
    /// <param name="entries">Exactly one entry for each reveal and wall frame, with its cells in run then cell order.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats a frame, changes a frame's shape, or a frame is missing.</exception>
    internal static InstalledRoomObjectRevealAndWallVisuals Create(IEnumerable<RoomObjectVisualEntry> entries) =>
        new(RoomObjectFramePictureReader.Flattened(entries, Family, TryGetByVisualId, All.Count()));

    /// <summary>Every reveal and wall frame's draw list, built from the terrain the game writes.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (RoomObjectBombedRevealDraw reveal in Enum.GetValues<RoomObjectBombedRevealDraw>())
                yield return Required(BombedRevealObjectDraws.TryGet(RoomObjectDraw.Of(reveal), out var draw), draw);
            yield return Required(CrateriaMainstreetEscapePassageDraws.TryGetDraw(CrateriaMainstreetEscapePassageObjectDefinitions.ClearPairDraw, out var passage), passage);
            yield return Required(OldTourianEscapeShaftWallDraws.TryGetDraw(OldTourianEscapeShaftWallObjectDefinitions.OpenWallDraw, out var wall), wall);
            foreach (EscapeAnimalDraw frame in Enum.GetValues<EscapeAnimalDraw>())
                yield return Required(EscapeAnimalWalls.TryGet(RoomObjectDraw.Of(frame), out var animalWall), animalWall);
        }
    }

    /// <summary>The installed identity of a reveal or wall frame.</summary>
    /// <exception cref="InvalidDataException">The draw is not a reveal or wall frame.</exception>
    internal static string VisualId(RoomObjectDraw draw) => draw switch
    {
        RoomObjectFrameDraw<RoomObjectBombedRevealDraw> { Frame: RoomObjectBombedRevealDraw.CrumbleSingle } => "bombed-reveal-crumble-block",
        RoomObjectFrameDraw<RoomObjectBombedRevealDraw> { Frame: RoomObjectBombedRevealDraw.PowerBomb } => "bombed-reveal-power-bomb-block",
        RoomObjectFrameDraw<RoomObjectBombedRevealDraw> { Frame: RoomObjectBombedRevealDraw.SuperMissile } => "bombed-reveal-super-missile-block",
        RoomObjectFrameDraw<CrateriaMainstreetEscapePassageObjectDefinitions.Draw> => "crateria-escape-passage-cleared",
        RoomObjectFrameDraw<OldTourianEscapeShaftWallObjectDefinitions.Draw> => "old-tourian-escape-shaft-wall-opened",
        RoomObjectFrameDraw<EscapeAnimalDraw> { Frame: EscapeAnimalDraw.Frame0 } => "escape-animal-wall-frame-0",
        RoomObjectFrameDraw<EscapeAnimalDraw> { Frame: EscapeAnimalDraw.Frame1 } => "escape-animal-wall-frame-1",
        RoomObjectFrameDraw<EscapeAnimalDraw> { Frame: EscapeAnimalDraw.Frame2 } => "escape-animal-wall-frame-2",
        RoomObjectFrameDraw<EscapeAnimalDraw> { Frame: EscapeAnimalDraw.Blank } => "escape-animal-wall-blank",
        _ => throw new InvalidDataException($"{Family} draw {draw} has no visual ID."),
    };

    /// <summary>Resolves an installed identity to its frame's draw list.</summary>
    internal static bool TryGetByVisualId(string id, out RoomObjectShotBlockDrawDefinitions.DrawList list) =>
        RoomObjectDrawListExport.FindByVisualId(All, VisualId, id, out list);

    /// <summary>The draw list a frame's own family describes.</summary>
    /// <exception cref="InvalidOperationException">The family does not describe the frame.</exception>
    private static RoomObjectShotBlockDrawDefinitions.DrawList Required(bool described, RoomObjectShotBlockDrawDefinitions.DrawList draw) =>
        described ? draw : throw new InvalidOperationException($"{Family} frame {draw.Draw} has no draw list.");
}
