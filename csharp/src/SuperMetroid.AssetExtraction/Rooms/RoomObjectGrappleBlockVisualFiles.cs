using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of the five breakable Grapple-block frames (#1557), and the published names that key them.</summary>
public static class RoomObjectGrappleBlockVisualFiles
{
    /// <summary>Family-relative installed JSON filename for the five initial, breakup, and blank breakable-Grapple frames.</summary>
    public const string VisualFileName = "grapple-blocks.json";
    /// <summary>The Grapple-block as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Grapple-block", Frames, draw => VisualId(RoomObjectDraw.FrameOf<GrappleBlockDraw>(draw, "Grapple-block")));

    /// <summary>Pictures every Grapple-block frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed Grapple-block pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectGrappleBlockVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>The published artwork identity of a Grapple-block draw.</summary>
    internal static string VisualId(GrappleBlockDraw frame) => frame switch
    {
        GrappleBlockDraw.Intact => "intact",
        GrappleBlockDraw.Break0 => "break-0",
        GrappleBlockDraw.Break1 => "break-1",
        GrappleBlockDraw.Break2 => "break-2",
        GrappleBlockDraw.Blank => "blank",
        _ => throw new InvalidOperationException($"Undefined {nameof(GrappleBlockDraw)} {frame}."),
    };

    /// <summary>Enumerates the initial Grapple block, three breakup frames, and blank air in authored order.</summary>
    internal static IEnumerable<RoomObjectGrappleBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (GrappleBlockDraw frame in Enum.GetValues<GrappleBlockDraw>())
            {
                GrappleBlockObjectDraws.TryGet(RoomObjectDraw.Of(frame), out var draw);
                yield return draw;
            }
        }
    }

    /// <summary>Every Grapple-block frame's one-block draw list, built from the collision the game writes.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> Frames =>
        All.Select(draw => RoomObjectDrawListExport.SingleRun(RoomObjectDraw.Of(draw.Frame), RoomObjectRunDirection.Horizontal, [draw.Collision]));
}
