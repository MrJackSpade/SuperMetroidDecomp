using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed pictures of the room changes of Mother Brain's fake death (#1557): the wall fill, the escape door, the
/// twelve mural rows, the two fill rows and the six tube clears.
/// </summary>
public static class RoomObjectMotherBrainFakeDeathVisualFiles
{
    /// <summary>The installed file holding the fake-death room frames' pictures.</summary>
    public const string VisualFileName = "mother-brain-fake-death.json";

    /// <summary>The fake-death room changes as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<MotherBrainFakeDeathDraw> Family { get; } =
        new(VisualFileName, "Mother Brain fake-death room", Layouts(), RoomObjectVisualIdDefinitions.MotherBrainFakeDeath);

    /// <summary>Pictures the twenty-two fake-death room frames from the cartridge's draw lists and writes the installed file.</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <param name="sourceCartridgeSha256">The cartridge's identity, recorded in the file.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the frames draw by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, sourceCartridgeSha256, Family);

    /// <summary>Loads the installed fake-death room pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format or source is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledMotherBrainFakeDeathFrames Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Every fake-death room frame's geometry, from the shapes the room's changes draw by.</summary>
    private static Dictionary<MotherBrainFakeDeathDraw, RoomObjectFrameLayout> Layouts()
    {
        var layouts = new Dictionary<MotherBrainFakeDeathDraw, RoomObjectFrameLayout>();
        foreach (MotherBrainFakeDeathDraw frame in Enum.GetValues<MotherBrainFakeDeathDraw>())
            layouts.Add(frame, Layout(RoomObjectDraw.Of(frame)));
        return layouts;
    }

    /// <summary>The geometry of a mural row, a boundary or a fill row or tube clear.</summary>
    private static RoomObjectFrameLayout Layout(RoomObjectDraw draw)
    {
        if (MotherBrainFakeDeathDraws.TryDescribeBackground(draw, out MotherBrainFakeDeathDraws.BackgroundDraw row))
            return RoomObjectFrameLayout.Of(draw, 1, static _ => MotherBrainFakeDeathObjectDrawDefinitions.BackgroundWidth,
                (_, column) => row.CollisionAt(column), RoomObjectDrawListExport.Horizontal, static _ => Offset16<RoomBlocks>.Zero);
        if (MotherBrainFakeDeathDraws.TryDescribeBoundary(draw, out MotherBrainFakeDeathDraws.BoundaryDraw boundary))
            return RoomObjectFrameLayout.Of(draw, MotherBrainFakeDeathDraws.BoundaryDraw.RunCount, boundary.Count, boundary.CollisionAt,
                static _ => RoomObjectRunDirection.Vertical, boundary.Start);
        if (MotherBrainFakeDeathDraws.TryDescribeRegular(draw, out MotherBrainFakeDeathDraws.RegularDraw regular))
            return RoomObjectFrameLayout.Of(draw, regular.RunCount, regular.Count, regular.CollisionAt,
                run => regular.Vertical(run) ? RoomObjectRunDirection.Vertical : RoomObjectRunDirection.Horizontal, regular.Start);
        throw new InvalidOperationException($"Mother Brain fake-death frame {draw} has no shape.");
    }
}
