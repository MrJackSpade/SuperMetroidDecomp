using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Extracts and loads the eight replaceable Samus Eater block frames.</summary>
public static class RoomObjectSamusEaterVisualFiles
{
    /// <summary>Editable Samus Eater plant visual JSON filename.</summary>
    private const string VisualFileName = "samus-eater.json";

    /// <summary>The plant's eight layouts in the installed file's order (#2567): the floor plant's four, then the ceiling plant's.</summary>
    private static readonly IReadOnlyList<SamusEaterDraw> FrameOrder = Ensure.EveryMemberOnce<SamusEaterDraw>(
    [
        SamusEaterDraw.FloorIdle,
        SamusEaterDraw.FloorTeethPeeking,
        SamusEaterDraw.FloorTeethBared,
        SamusEaterDraw.FloorFangsOut,
        SamusEaterDraw.CeilingIdle,
        SamusEaterDraw.CeilingTeethPeeking,
        SamusEaterDraw.CeilingTeethBared,
        SamusEaterDraw.CeilingFangsOut,
    ]);

    /// <summary>
    /// Where each plant frame's four-wide foliage row starts beside the mouth's left half: below it for a floor plant,
    /// above it for a ceiling one.
    /// </summary>
    private static readonly IReadOnlyDictionary<SamusEaterDraw, Offset16<RoomBlocks>> FoliageStarts = new Dictionary<SamusEaterDraw, Offset16<RoomBlocks>>
    {
        [SamusEaterDraw.FloorIdle] = new(-2, -1),
        [SamusEaterDraw.FloorTeethPeeking] = new(-2, -1),
        [SamusEaterDraw.FloorTeethBared] = new(-2, -1),
        [SamusEaterDraw.FloorFangsOut] = new(-2, -1),
        [SamusEaterDraw.CeilingIdle] = new(-2, 1),
        [SamusEaterDraw.CeilingTeethPeeking] = new(-2, 1),
        [SamusEaterDraw.CeilingTeethBared] = new(-2, 1),
        [SamusEaterDraw.CeilingFangsOut] = new(-2, 1),
    };

    /// <summary>The Samus Eater plant as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Samus Eater plant", All, VisualId);

    /// <summary>Pictures every Samus Eater plant frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed Samus Eater plant pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectPictures Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Maps a validated draw to the artwork catalog's orientation and phase ID.</summary>
    internal static string VisualId(RoomObjectDraw request) =>
        VisualId(RoomObjectDrawIdentity.FrameOf<SamusEaterDraw>(request, "Samus Eater draw with a visual ID"));

    /// <summary>Names a typed draw list by its floor or ceiling orientation and idle or chewing look.</summary>
    internal static string VisualId(SamusEaterDraw frame) => frame switch
    {
        SamusEaterDraw.FloorIdle => "floor-idle",
        SamusEaterDraw.FloorTeethPeeking => "floor-teeth-peeking",
        SamusEaterDraw.FloorTeethBared => "floor-teeth-bared",
        SamusEaterDraw.FloorFangsOut => "floor-fangs-out",
        SamusEaterDraw.CeilingIdle => "ceiling-idle",
        SamusEaterDraw.CeilingTeethPeeking => "ceiling-teeth-peeking",
        SamusEaterDraw.CeilingTeethBared => "ceiling-teeth-bared",
        SamusEaterDraw.CeilingFangsOut => "ceiling-fangs-out",
        _ => throw new InvalidOperationException($"Undefined SamusEaterDraw {frame}."),
    };

    /// <summary>The draw list of <paramref name="draw"/>, which must be one of the Samus eater draws.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a Samus eater draw.</exception>
    internal static RoomObjectDrawLists.DrawList DrawListOf(RoomObjectDraw draw) =>
        TryGet(draw, out RoomObjectDrawLists.DrawList list) ? list : throw new InvalidDataException($"{draw} is not a Samus eater draw.");

    /// <summary>
    /// The draw list of a plant draw (#1862): its block shape written as the right half of the mouth, its left half two
    /// blocks left, and the four-wide foliage below those for a floor plant or above them for a ceiling one.
    /// </summary>
    private static bool TryGet(RoomObjectDraw request, out RoomObjectDrawLists.DrawList list)
    {
        if (!RoomObjectDrawIdentity.TryShape(SamusEaterObjectDraws.Family, request, out RoomBlockShape? shape))
        {
            list = default;
            return false;
        }
        Offset16<RoomBlocks> foliage = Ensure.Listed(FoliageStarts, RoomObjectDrawIdentity.FrameOf<SamusEaterDraw>(request, "Samus Eater"));
        list = RoomObjectShapeRun.DrawList(request, shape, RoomObjectShapeRun.Row(new(0, 0), 2), RoomObjectShapeRun.Row(new(-2, 0), 2),
            RoomObjectShapeRun.Row(foliage, 4));
        return true;
    }

    /// <summary>Builds the eight plant layouts' draw lists.</summary>
    private static IEnumerable<RoomObjectDrawLists.DrawList> All
    {
        get
        {
            foreach (SamusEaterDraw frame in FrameOrder)
            {
                yield return DrawListOf(SamusEaterObjectDraws.Family.Of(frame));
            }
        }
    }
}
