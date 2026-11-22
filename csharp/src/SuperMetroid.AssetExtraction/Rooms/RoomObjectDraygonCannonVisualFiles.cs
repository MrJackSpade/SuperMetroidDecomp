using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of Draygon's wall cannons' twelve frames (#1557).</summary>
public static partial class RoomObjectDraygonCannonVisualFiles
{
    /// <summary>The way each cannon frame reaches from the cannon's core: the right cannon's frames right, the left's left.</summary>
    private static readonly IReadOnlyDictionary<DraygonCannonDraw, HorizontalDirection> Facings = new Dictionary<DraygonCannonDraw, HorizontalDirection>
    {
        [DraygonCannonDraw.RightShield] = HorizontalDirection.Right,
        [DraygonCannonDraw.RightShieldFlash] = HorizontalDirection.Right,
        [DraygonCannonDraw.RightSparksFlaring] = HorizontalDirection.Right,
        [DraygonCannonDraw.RightSparksSpreading] = HorizontalDirection.Right,
        [DraygonCannonDraw.RightSparksReceding] = HorizontalDirection.Right,
        [DraygonCannonDraw.RightSparksFlat] = HorizontalDirection.Right,
        [DraygonCannonDraw.LeftShield] = HorizontalDirection.Left,
        [DraygonCannonDraw.LeftShieldFlash] = HorizontalDirection.Left,
        [DraygonCannonDraw.LeftSparksFlaring] = HorizontalDirection.Left,
        [DraygonCannonDraw.LeftSparksSpreading] = HorizontalDirection.Left,
        [DraygonCannonDraw.LeftSparksReceding] = HorizontalDirection.Left,
        [DraygonCannonDraw.LeftSparksFlat] = HorizontalDirection.Left,
    };

    /// <summary>
    /// The rows a frame reaching each way is written as, where each starts and how many blocks long: a frame reaching right
    /// of the cannon's core is written as its two rows, core row first; a frame reaching left splits its top row into the
    /// core and the cell to its left, then writes the lower row from its left end.
    /// </summary>
    private static readonly Dictionary<HorizontalDirection, RoomObjectShapeRun[]> RowsByFacing =
        new Dictionary<HorizontalDirection, RoomObjectShapeRun[]>
        {
            [HorizontalDirection.Right] = [RoomObjectShapeRun.Row(new(0, 0), 2), RoomObjectShapeRun.Row(new(0, 1), 2)],
            [HorizontalDirection.Left] =
                [RoomObjectShapeRun.Row(new(0, 0), 1), RoomObjectShapeRun.Row(new(-1, 0), 1), RoomObjectShapeRun.Row(new(-1, 1), 2)],
        };

    /// <summary>The installed file holding the right- and left-facing cannon frames' pictures.</summary>
    internal const string VisualFileName = "draygon-cannons.json";

    /// <summary>The cannon frames in the installed file's order (#2567): the right-facing frames, then the left-facing.</summary>
    private static readonly IReadOnlyList<DraygonCannonDraw> FrameOrder = Ensure.EveryMemberOnce<DraygonCannonDraw>(
    [
        DraygonCannonDraw.RightShield,
        DraygonCannonDraw.RightShieldFlash,
        DraygonCannonDraw.RightSparksFlaring,
        DraygonCannonDraw.RightSparksSpreading,
        DraygonCannonDraw.RightSparksReceding,
        DraygonCannonDraw.RightSparksFlat,
        DraygonCannonDraw.LeftShield,
        DraygonCannonDraw.LeftShieldFlash,
        DraygonCannonDraw.LeftSparksFlaring,
        DraygonCannonDraw.LeftSparksSpreading,
        DraygonCannonDraw.LeftSparksReceding,
        DraygonCannonDraw.LeftSparksFlat,
    ]);

    /// <summary>The cannons as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<DraygonCannonDraw> Family { get; } =
        new(VisualFileName, "Draygon cannon", Layouts());

    /// <summary>Pictures the twelve cannon frames from the cartridge's draw lists and writes the installed file.</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the cannons' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the cannons draw by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed cannon pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledDraygonCannonFrames Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>
    /// Every cannon frame's geometry as the cartridge writes it: the runs of its draw list, each cell colliding as the
    /// game's block shape for the frame says (#1608). The run form is the cartridge's; the game draws the shape.
    /// </summary>
    private static List<RoomObjectFamilyFrame<DraygonCannonDraw>> Layouts()
    {
        var layouts = new List<RoomObjectFamilyFrame<DraygonCannonDraw>>();
        foreach (DraygonCannonDraw frame in FrameOrder)
        {
            RoomObjectDraw draw = DraygonCannonDraws.Family.Of(frame);
            if (!RoomObjectDrawIdentity.TryShape(DraygonCannonDraws.Family, draw, out RoomBlockShape? described))
                throw new InvalidOperationException($"Draygon cannon frame {frame} has no shape.");
            layouts.Add(new(frame, RoomObjectFrameLayout.Of(draw, described, Ensure.Listed(RowsByFacing, Ensure.Listed(Facings, frame))),
                DraygonCannonDrawKeys.Installed.Of(frame)));
        }
        return layouts;
    }

}
