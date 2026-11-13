using System.Text.Json;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Installs and loads the terrain a Wave shot's scan past the room's ceiling finds where it lands halfway through a block
/// (#1612): a read from the end of one block to the start of the next, which the import decodes once, from
/// <see cref="RoomLevelWordCodec"/>, for every first block and every second block, so the game looks up the straddled
/// terrain of the blocks the room holds when the scan reaches them and never knows the level word's layout.
/// </summary>
/// <remarks>
/// The table needs nothing from the cartridge but its level-word layout, which the codec owns: every level word decodes
/// to exactly one terrain block and every terrain block is stored as exactly one level word, so decoding each word once
/// gives each block's two stored halves. A first block's lead is its second stored half, a second block's tail is its
/// first stored half (which only its block number decides), and a lead and tail make the terrain the word stored as
/// that lead followed by that tail decodes to.
/// </remarks>
public static class StraddledTerrainFiles
{
    /// <summary>Every mirroring, in the order the leads are written.</summary>
    private static readonly BlockMirroring[] Mirrorings = Enum.GetValues<BlockMirroring>();

    /// <summary>Every mirroring by its name.</summary>
    private static readonly Dictionary<string, BlockMirroring> MirroringsByName =
        Mirrorings.ToDictionary(mirroring => mirroring.ToString(), StringComparer.Ordinal);

    /// <summary>Every collision kind by its name.</summary>
    private static readonly Dictionary<string, RoomCollisionType> KindsByName =
        Enum.GetValues<RoomCollisionType>().ToDictionary(kind => kind.ToString(), StringComparer.Ordinal);

    /// <summary>
    /// Decodes the straddled terrain and writes it as JSON. No manifest is written (#1693): the document names its own
    /// format, and it is decoded from the level-word codec, not read from the cartridge.
    /// </summary>
    /// <param name="directory">Destination directory, created if absent; an existing file is replaced.</param>
    public static void Extract(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        File.WriteAllBytes(Path.Combine(directory, StraddledTerrainFormat.FileName),
            JsonSerializer.SerializeToUtf8Bytes(Decode(), StraddledTerrainFormat.JsonOptions));
    }

    /// <summary>Builds the straddled terrain from the installed file, as the player may have edited it.</summary>
    /// <param name="directory">Installed straddled-terrain directory holding the file.</param>
    /// <exception cref="InvalidDataException">The file's format or shape is invalid.</exception>
    public static StraddledTerrain<RoomTerrainBlock> Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string path = Path.Combine(directory, StraddledTerrainFormat.FileName);
        StraddledTerrainDocument document = JsonAssetDocument.Read<StraddledTerrainDocument>(
            new MemoryStream(File.ReadAllBytes(path), writable: false), StraddledTerrainFormat.JsonOptions,
            $"Straddled terrain {path}");
        return Read(document);
    }

    /// <summary>
    /// The straddled terrain decoded from <see cref="RoomLevelWordCodec"/>: every level word decoded once, its halves
    /// giving its block's lead and tail, and every lead and tail decoded as the word they are stored as.
    /// </summary>
    /// <exception cref="InvalidOperationException">The codec does not store every terrain block as exactly one word.</exception>
    internal static StraddledTerrainDocument Decode()
    {
        var leadRows = new Dictionary<RoomCollisionType, Dictionary<BlockMirroring, int[]>>();
        foreach (RoomCollisionType kind in RoomLevelWordCodec.KindsByNibble)
            leadRows.Add(kind, Mirrorings.ToDictionary(mirroring => mirroring, _ => Unset(RoomBlockAppearance.LastBlock + 1)));
        int[] tails = Unset(RoomBlockAppearance.LastBlock + 1);
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            RoomTerrainBlock terrain = RoomLevelWordCodec.Decode((ushort)value);
            int block = terrain.Appearance.Block;
            int[] leads = leadRows[terrain.Collision][terrain.Appearance.Mirroring];
            if (leads[block] >= 0)
                throw new InvalidOperationException($"Two level words decode to {terrain}.");
            leads[block] = RoomLevelWordCodec.SecondStoredHalf((ushort)value);
            int tail = RoomLevelWordCodec.FirstStoredHalf((ushort)value);
            if (tails[block] >= 0 && tails[block] != tail)
                throw new InvalidOperationException($"The start of block {block} depends on more than its number.");
            tails[block] = tail;
        }
        if (leadRows.Values.SelectMany(rows => rows.Values).Any(row => row.Contains(-1)) || tails.Contains(-1))
            throw new InvalidOperationException("A terrain block is stored as no level word.");
        var outcomes = new StraddledTerrainDocument.Outcome[RoomLevelWordCodec.HalfValues][];
        for (int lead = 0; lead < outcomes.Length; lead++)
        {
            outcomes[lead] = new StraddledTerrainDocument.Outcome[RoomLevelWordCodec.HalfValues];
            for (int tail = 0; tail < outcomes[lead].Length; tail++)
            {
                RoomTerrainBlock straddled = RoomLevelWordCodec.Decode(RoomLevelWordCodec.OfStoredHalves(lead, tail));
                outcomes[lead][tail] = new StraddledTerrainDocument.Outcome(straddled.Appearance.Block,
                    straddled.Appearance.Mirroring.ToString(), straddled.Collision.ToString());
            }
        }
        return new StraddledTerrainDocument(
            StraddledTerrainFormat.Version,
            leadRows.ToDictionary(
                kinds => kinds.Key.ToString(),
                kinds => (IReadOnlyDictionary<string, IReadOnlyList<int>>)kinds.Value.ToDictionary(
                    rows => rows.Key.ToString(), rows => (IReadOnlyList<int>)rows.Value)),
            tails,
            outcomes);
    }

    /// <summary>Builds the straddled terrain from a document, requiring a lead for every first block and a tail for every block number.</summary>
    /// <exception cref="InvalidDataException">The document is incomplete or names a mirroring or collision kind the game does not have.</exception>
    internal static StraddledTerrain<RoomTerrainBlock> Read(StraddledTerrainDocument document)
    {
        Ensure.NotNull(document);
        if (document.Version != StraddledTerrainFormat.Version)
            throw new InvalidDataException("Straddled-terrain file has an unsupported version.");
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<int>>> leadRows =
            Require(document.Leads, "leads");
        RoomCollisionType[] kinds = Enum.GetValues<RoomCollisionType>();
        if (leadRows.Count != kinds.Length)
            throw new InvalidDataException("Straddled-terrain leads must name every collision kind once.");
        IReadOnlyList<int> tailsByNumber = Require(document.Tails, "tails");
        if (tailsByNumber.Count != RoomBlockAppearance.LastBlock + 1)
            throw new InvalidDataException("Straddled-terrain tails must hold every block number.");
        var leads = new Dictionary<RoomTerrainBlock, int>();
        var tails = new Dictionary<RoomTerrainBlock, int>();
        foreach (RoomCollisionType kind in kinds)
        {
            if (!leadRows.TryGetValue(kind.ToString(), out IReadOnlyDictionary<string, IReadOnlyList<int>>? rows) || rows is null ||
                rows.Count != Mirrorings.Length)
                throw new InvalidDataException($"Straddled-terrain leads must name every mirroring once for {kind}.");
            foreach (BlockMirroring mirroring in Mirrorings)
            {
                if (!rows.TryGetValue(mirroring.ToString(), out IReadOnlyList<int>? blocks) || blocks is null ||
                    blocks.Count != RoomBlockAppearance.LastBlock + 1)
                    throw new InvalidDataException(
                        $"Straddled-terrain leads must hold every block number for {kind} mirrored {mirroring}.");
                for (int block = 0; block < blocks.Count; block++)
                {
                    RoomTerrainBlock terrain = RoomTerrainBlock.Of(block, mirroring, kind);
                    leads.Add(terrain, blocks[block]);
                    // The start of a block contributes the same whatever its mirroring and kind.
                    tails.Add(terrain, tailsByNumber[block]);
                }
            }
        }
        IReadOnlyList<IReadOnlyList<StraddledTerrainDocument.Outcome>> rowsByLead =
            Require(document.Outcomes, "outcomes");
        var outcomes = new IReadOnlyList<RoomTerrainBlock>[rowsByLead.Count];
        for (int lead = 0; lead < outcomes.Length; lead++)
        {
            outcomes[lead] = Require(rowsByLead[lead], $"outcomes for lead {lead}").Select(Terrain).ToArray();
        }
        try
        {
            return new StraddledTerrain<RoomTerrainBlock>(leads, tails, outcomes);
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException($"Straddled-terrain file is incomplete: {error.Message}", error);
        }
    }

    /// <summary>The terrain block an outcome names.</summary>
    /// <exception cref="InvalidDataException">The outcome names a block, mirroring or collision kind the game does not have.</exception>
    private static RoomTerrainBlock Terrain(StraddledTerrainDocument.Outcome? outcome)
    {
        if (outcome is null)
            throw new InvalidDataException("Straddled-terrain file has an empty outcome.");
        if (outcome.Block is < 0 or > RoomBlockAppearance.LastBlock)
            throw new InvalidDataException($"Straddled-terrain outcome names block {outcome.Block}, which no block set holds.");
        return RoomTerrainBlock.Of(outcome.Block, Named(MirroringsByName, outcome.Mirroring), Named(KindsByName, outcome.Collision));
    }

    /// <summary>The value <paramref name="name"/> names exactly.</summary>
    /// <exception cref="InvalidDataException">No value has that name.</exception>
    private static T Named<T>(Dictionary<string, T> byName, string? name) where T : struct, Enum =>
        name is not null && byName.TryGetValue(name, out T value)
            ? value
            : throw new InvalidDataException($"Straddled-terrain outcome names {typeof(T).Name} '{name}', which the game does not have.");

    /// <summary>Requires a document member the file must hold.</summary>
    /// <exception cref="InvalidDataException">The file holds no such member.</exception>
    private static T Require<T>(T? value, string name) where T : class =>
        value ?? throw new InvalidDataException($"Straddled-terrain file has no {name}.");

    /// <summary>A row of <paramref name="count"/> entries, none of them decoded yet.</summary>
    private static int[] Unset(int count) => Enumerable.Repeat(-1, count).ToArray();
}
