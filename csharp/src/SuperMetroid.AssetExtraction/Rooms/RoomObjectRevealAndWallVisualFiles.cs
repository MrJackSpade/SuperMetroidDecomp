using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
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

    /// <summary>The bombed reveals in the installed file's order (#2567), which lists them first.</summary>
    private static readonly IReadOnlyList<RoomObjectBombedRevealDraw> Reveals = Ensure.EveryMemberOnce<RoomObjectBombedRevealDraw>(
    [
        RoomObjectBombedRevealDraw.CrumbleSingle,
        RoomObjectBombedRevealDraw.PowerBomb,
        RoomObjectBombedRevealDraw.SuperMissile,
    ]);

    /// <summary>The rescue wall's frames in the installed file's order (#2567), which lists them last: cracking, breaking, crumbling, blank.</summary>
    private static readonly IReadOnlyList<EscapeAnimalDraw> RescueWallFrames = Ensure.EveryMemberOnce<EscapeAnimalDraw>(
    [
        EscapeAnimalDraw.Cracking,
        EscapeAnimalDraw.Breaking,
        EscapeAnimalDraw.Crumbling,
        EscapeAnimalDraw.Blank,
    ]);

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
    private static IEnumerable<RoomObjectDrawLists.DrawList> All
    {
        get
        {
            // Each reveal is written as one block, the passage as one row of two, the shaft wall as two columns of three and
            // each rescue wall as one column of three (#1862).
            foreach (RoomObjectBombedRevealDraw reveal in Reveals)
            {
                RoomObjectDraw draw = BombedRevealObjectDraws.Family.Of(reveal);
                yield return RoomObjectShapeRun.DrawList(draw, BombedRevealObjectDraws.Family.Shape(reveal),
                    RoomObjectShapeRun.Row(new(0, 0), 1));
            }
            RoomObjectFrameDraw<CrateriaMainstreetEscapePassageObjectDefinitions.Draw> passage = CrateriaMainstreetEscapePassageObjectDefinitions.ClearPairDraw;
            yield return RoomObjectShapeRun.DrawList(passage, CrateriaMainstreetEscapePassageDraws.Family.Shape(passage.Frame),
                RoomObjectShapeRun.Row(new(0, 0), 2));
            RoomObjectFrameDraw<OldTourianEscapeShaftWallObjectDefinitions.Draw> wall = OldTourianEscapeShaftWallObjectDefinitions.OpenWallDraw;
            yield return RoomObjectShapeRun.DrawList(wall, OldTourianEscapeShaftWallDraws.Family.Shape(wall.Frame),
                RoomObjectShapeRun.Column(new(0, 0), 3), RoomObjectShapeRun.Column(new(1, 0), 3));
            foreach (EscapeAnimalDraw frame in RescueWallFrames)
            {
                RoomObjectDraw animalWall = EscapeAnimalWalls.Family.Of(frame);
                yield return RoomObjectShapeRun.DrawList(animalWall,
                    EscapeAnimalWalls.Family.Shape(frame),
                    RoomObjectShapeRun.Column(new(0, 0), 3));
            }
        }
    }

    /// <summary>The installed identity of a reveal or wall frame.</summary>
    /// <exception cref="InvalidDataException">The draw is not a reveal or wall frame.</exception>
    private static string VisualId(RoomObjectDraw draw) => draw switch
    {
        RoomObjectFrameDraw<RoomObjectBombedRevealDraw> { Frame: RoomObjectBombedRevealDraw.CrumbleSingle } => "bombed-reveal-crumble-block",
        RoomObjectFrameDraw<RoomObjectBombedRevealDraw> { Frame: RoomObjectBombedRevealDraw.PowerBomb } => "bombed-reveal-power-bomb-block",
        RoomObjectFrameDraw<RoomObjectBombedRevealDraw> { Frame: RoomObjectBombedRevealDraw.SuperMissile } => "bombed-reveal-super-missile-block",
        RoomObjectFrameDraw<CrateriaMainstreetEscapePassageObjectDefinitions.Draw> => "crateria-escape-passage-cleared",
        RoomObjectFrameDraw<OldTourianEscapeShaftWallObjectDefinitions.Draw> => "old-tourian-escape-shaft-wall-opened",
        RoomObjectFrameDraw<EscapeAnimalDraw> { Frame: EscapeAnimalDraw.Cracking } => "escape-animal-wall-cracking",
        RoomObjectFrameDraw<EscapeAnimalDraw> { Frame: EscapeAnimalDraw.Breaking } => "escape-animal-wall-breaking",
        RoomObjectFrameDraw<EscapeAnimalDraw> { Frame: EscapeAnimalDraw.Crumbling } => "escape-animal-wall-crumbling",
        RoomObjectFrameDraw<EscapeAnimalDraw> { Frame: EscapeAnimalDraw.Blank } => "escape-animal-wall-blank",
        _ => throw new InvalidDataException($"Reveal and wall draw {draw} has no visual ID."),
    };
}
