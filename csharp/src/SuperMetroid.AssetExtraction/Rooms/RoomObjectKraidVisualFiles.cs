using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

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
    /// <param name="sourceCartridgeSha256">The cartridge's identity, recorded in the file.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, sourceCartridgeSha256, Family);

    /// <summary>Loads the installed Kraid room pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format or source is not this importer's, or a frame's picture is missing or misshapen.</exception>
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

    /// <summary>The one horizontal run of each Kraid draw, built from the blocks the game calculates.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (KraidRoomDraw frame in Enum.GetValues<KraidRoomDraw>())
            {
                var draw = new KraidRoomObjectDraws.Draw(frame);
                var blocks = new RoomTerrainBlock[draw.BlockCount];
                for (int block = 0; block < blocks.Length; block++) blocks[block] = draw.BlockAt(block);
                yield return RoomObjectDrawListExport.SingleRun(RoomObjectDraw.Of(frame), RoomObjectRunDirection.Horizontal, blocks);
            }
        }
    }
}
