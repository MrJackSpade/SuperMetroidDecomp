using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable Spore Spawn ceiling visual blocks.</summary>
public static class RoomObjectSporeSpawnCeilingVisualFiles
{
    /// <summary>Editable Spore Spawn ceiling visual JSON filename.</summary>
    public const string VisualFileName = "spore-spawn-ceiling.json";

    /// <summary>Verifies every native Spore Spawn ceiling draw and creates stock JSON plus the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomObjectDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Spore Spawn ceiling",
            All,
            VisualId);

    /// <summary>Validates the installed Spore Spawn ceiling manifest's provenance and loads the installed Spore Spawn ceiling JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and Spore Spawn ceiling JSON.</param>
    /// <returns>The immutable Spore Spawn ceiling visual catalog.</returns>
    public static RoomObjectSporeSpawnCeilingVisualCatalog Load(string directory) =>
        RoomObjectDoorVisualFileCodec.Load(directory,
            new RoomObjectVisualFamily<RoomObjectSporeSpawnCeilingVisualCatalog>(VisualFileName, "Spore Spawn ceiling", entries => Create(entries.Select(entry =>
                new RoomObjectVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>Validates complete coverage of the clear and three crumble frames and builds the visuals from every frame's picture as installed, without changing compiled geometry or collision.</summary>
    /// <param name="entries">Each of the four compiled visual identities exactly once, with four appearances per frame.</param>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">A frame is missing, duplicated, null, unknown, or incorrectly shaped.</exception>
    internal static RoomObjectSporeSpawnCeilingVisualCatalog Create(IEnumerable<RoomObjectVisualEntry> entries) =>
        new(RoomObjectFramePictureReader.Flattened(entries, "Spore Spawn ceiling", TryGetByVisualId, All.Count()));

    /// <summary>The draw lists of the clear and crumble frames.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (SporeSpawnCeilingDraw frame in Enum.GetValues<SporeSpawnCeilingDraw>())
            {
                TryGet(RoomObjectDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }

    /// <summary>Selects a ceiling draw's published artwork key.</summary>
    internal static string VisualId(RoomObjectDraw request) =>
        VisualId(RoomObjectDraw.FrameOf<SporeSpawnCeilingDraw>(request, "Spore Spawn ceiling draw with a visual ID"));

    /// <summary>Names the clear layout and the three successive crumble appearances for visual editing.</summary>
    internal static string VisualId(SporeSpawnCeilingDraw frame) =>
        RoomObjectVisualIdDefinitions.SporeSpawnCeiling.TryGetValue(frame, out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined SporeSpawnCeilingDraw {frame}.");

    /// <summary>Finds the physical square associated with an editable ceiling-frame key.</summary>
    internal static bool TryGetByVisualId(string id, out RoomObjectShotBlockDrawDefinitions.DrawList list) =>
        RoomObjectDrawListExport.FindByVisualId(All, VisualId, id, out list);

    /// <summary>Builds the two-run square of a supported clear or crumble draw.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        if (SporeSpawnCeilingDraws.TryGetBlock(request, out RoomTerrainBlock block))
        {
            list = Square(request, block);
            return true;
        }
        list = default;
        return false;
    }

    /// <summary>Duplicates one terrain block across the two horizontal runs of the ceiling's two-by-two layout.</summary>
    private static RoomObjectShotBlockDrawDefinitions.DrawList Square(RoomObjectDraw draw, RoomTerrainBlock block) =>
        RoomObjectDrawListExport.Runs(draw, 2, static _ => 2, (_, _) => block,
            RoomObjectDrawListExport.Horizontal, RoomObjectDrawListExport.Stacked);
}
