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

    /// <summary>The reveal and wall as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Reveal and wall", All, VisualId);

    /// <summary>Pictures every reveal and wall frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed reveal and wall pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectRevealAndWallVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Every reveal and wall frame's draw list, built from the geometry and collision the game writes.</summary>
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
        _ => throw new InvalidDataException($"Reveal and wall draw {draw} has no visual ID."),
    };


    /// <summary>The draw list a frame's own family describes.</summary>
    /// <exception cref="InvalidOperationException">The family does not describe the frame.</exception>
    private static RoomObjectShotBlockDrawDefinitions.DrawList Required(bool described, RoomObjectShotBlockDrawDefinitions.DrawList draw) =>
        described ? draw : throw new InvalidOperationException($"Reveal and wall frame {draw.Draw} has no draw list.");
}
