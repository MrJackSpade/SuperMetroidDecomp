using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable Kraid ceiling and spike visual blocks, and the editable identities that name them.</summary>
public static class RoomObjectKraidVisualFiles
{
    /// <summary>Family-relative installed JSON filename for Kraid's ten ceiling and spike layouts, including the fifteen- and twenty-two-block clears.</summary>
    public const string VisualFileName = "kraid-room.json";

    /// <summary>The Kraid room as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Kraid room", All, VisualId);

    /// <summary>Pictures every Kraid room frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed Kraid room pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectKraidVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Gets the stable artwork identifier of a Kraid room draw.</summary>
    internal static string VisualId(RoomObjectDraw request) =>
        VisualId(RoomObjectDraw.FrameOf<KraidRoomDraw>(request, "Kraid room draw with a visual ID"));

    /// <summary>Maps a named ceiling or spike draw to its stable artwork identifier.</summary>
    private static string VisualId(KraidRoomDraw draw) => draw switch
    {
        KraidRoomDraw.CrumbleFirst => "crumble-first",
        KraidRoomDraw.CrumbleSecond => "crumble-second",
        KraidRoomDraw.CrumbleThird => "crumble-third",
        KraidRoomDraw.CeilingBackground1 => "ceiling-background-one",
        KraidRoomDraw.CeilingBackground2 => "ceiling-background-two",
        KraidRoomDraw.CeilingBackground3 => "ceiling-background-three",
        KraidRoomDraw.SpikeFirst => "spike-first",
        KraidRoomDraw.SpikeSecond => "spike-second",
        KraidRoomDraw.ClearCeiling => "clear-ceiling",
        KraidRoomDraw.ClearSpikes => "clear-spikes",
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidRoomDraw)} {draw}."),
    };

    /// <summary>
    /// Every Kraid room frame's draw list as the cartridge writes it: one run along the row from the object's block, each
    /// cell colliding as the game's block shape for the frame says (#1608). The run form is the cartridge's; the game
    /// draws the shape.
    /// </summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (KraidRoomDraw frame in Enum.GetValues<KraidRoomDraw>())
            {
                RoomObjectDraw draw = RoomObjectDraw.Of(frame);
                if (!KraidRoomObjectDraws.TryDescribe(draw, out RoomBlockShape? shape))
                    throw new InvalidOperationException($"Kraid room frame {frame} has no shape.");
                var collisions = new RoomCollisionType[shape.Cells.Count];
                for (int cell = 0; cell < collisions.Length; cell++) collisions[cell] = shape.CollisionAt(new Offset16<RoomBlocks>(cell, 0));
                yield return RoomObjectDrawListExport.SingleRun(draw, RoomObjectRunDirection.Horizontal, collisions);
            }
        }
    }
}
