using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Bosses.Torizo.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of Bomb Torizo's intact and cleared hand frames (#1557), and the published names that key them.</summary>
public static class RoomObjectBombTorizoHandVisualFiles
{
    /// <summary>Editable Bomb Torizo hand PLM visual JSON filename.</summary>
    public const string VisualFileName = "bomb-torizo-hand.json";

    /// <summary>The Bomb Torizo hand as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Bomb Torizo hand", All, VisualId);

    /// <summary>Pictures every Bomb Torizo hand frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <param name="sourceCartridgeSha256">The cartridge's identity, recorded in the file.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, sourceCartridgeSha256, Family);

    /// <summary>Loads the installed Bomb Torizo hand pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format or source is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectBombTorizoHandVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>The intact and cleared hand draw lists, in authored order.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            TryGet(RoomObjectDraw.Of(BombTorizoHandDraw.Intact), out var intact);
            yield return intact;
            TryGet(RoomObjectDraw.Of(BombTorizoHandDraw.Cleared), out var cleared);
            yield return cleared;
        }
    }

    /// <summary>Builds the draw list of a hand draw from its strips.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        bool found = BombTorizoHandDraws.TryDescribe(request, out BombTorizoHandDraws.Draw draw);
        list = found
            ? RoomObjectDrawListExport.Strips(request, draw.RunCount, draw.BlockCount, draw.CollisionAt, draw.Start)
            : default;
        return found;
    }

    /// <summary>Gets the stable artwork ID for a supported hand draw.</summary>
    internal static string VisualId(RoomObjectDraw request) =>
        VisualId(RoomObjectDraw.FrameOf<BombTorizoHandDraw>(request, "Bomb Torizo hand draw with a visual ID"));

    /// <summary>Maps an intact or cleared hand draw to its stable artwork ID.</summary>
    internal static string VisualId(BombTorizoHandDraw frame) =>
        RoomObjectVisualIdDefinitions.BombTorizoHand.TryGetValue(frame, out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined BombTorizoHandDraw {frame}.");
}
