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

    /// <summary>Checks Kraid's bank-$84 draw runs, terrain, and offsets against compiled definitions, then exports presentation-only block/flip words.</summary>
    /// <param name="bus">Cartridge source for the reachable ceiling/spike draws at $84:9367–93EE, excluding the unused $938B record.</param>
    /// <param name="directory">Family asset directory, created if absent; receives new <see cref="VisualFileName"/> and <c>manifest.json</c> files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied provenance hash; a later stock load requires the supported cartridge hash.</param>
    /// <remarks>No existing output is overwritten; separate JSON/manifest writes can leave the first file behind if the second fails.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">A native draw differs from its compiled shape, words, or continuation offset.</exception>
    /// <exception cref="IOException">An output already exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomObjectDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Kraid room",
            All, VisualId);

    /// <summary>Validates the installed Kraid room manifest's provenance and loads the installed Kraid room JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and Kraid room JSON.</param>
    /// <returns>A ROM-independent catalog of the ten compiled Kraid layouts; the separately installed elevatube appearance is not changed.</returns>
    /// <exception cref="InvalidDataException">Manifest provenance, document version, identities, coverage, shapes, or appearance are invalid.</exception>
    /// <exception cref="IOException">The installed manifest or Kraid room JSON cannot be read.</exception>
    public static InstalledRoomObjectKraidVisuals Load(string directory) =>
        RoomObjectDoorVisualFileCodec.Load(directory,
            new RoomObjectVisualFamily<InstalledRoomObjectKraidVisuals>(VisualFileName, "Kraid room", entries => Create(entries.Select(entry =>
                new RoomObjectVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>Validates and copies all ten Kraid ceiling and spike layouts, building the visuals without changing collision appearances, crumble timing, or movement callbacks.</summary>
    /// <param name="entries">Exactly one visual-only entry per compiled role, with one block for individual stages or fifteen/twenty-two for the clearing layouts.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats a role, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    internal static InstalledRoomObjectKraidVisuals Create(IEnumerable<RoomObjectVisualEntry> entries)
    {
        Ensure.NotNull(entries);
        var frames = new Dictionary<KraidRoomDraw, RoomBlockAppearance[]>();
        foreach (RoomObjectVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !TryGetByVisualId(entry.Id, out var draw) ||
                entry.Blocks.Length != draw.BlockCount)
                throw new InvalidDataException(
                    "Kraid room visuals changed a frame identity or draw shape.");
            if (!frames.TryAdd(draw.Frame, entry.Blocks.ToArray()))
                throw new InvalidDataException(
                    $"Kraid room visuals repeat frame {entry.Id}.");
        }
        if (frames.Count != Enum.GetValues<KraidRoomDraw>().Length)
            throw new InvalidDataException(
                "Kraid room visuals do not cover all ten compiled draws.");
        return new(frames);
    }

    /// <summary>Resolves an ordinal artwork identifier to its calculated Kraid draw.</summary>
    internal static bool TryGetByVisualId(string id, out KraidRoomObjectDraws.Draw draw)
    {
        foreach (KraidRoomDraw candidate in Enum.GetValues<KraidRoomDraw>())
            if (string.Equals(id, VisualId(candidate), StringComparison.Ordinal))
                return KraidRoomObjectDraws.TryGet(RoomObjectDraw.Of(candidate), out draw);
        draw = default;
        return false;
    }

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
