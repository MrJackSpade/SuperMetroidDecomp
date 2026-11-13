using System.Text.Json;
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
/// that lead followed by that tail decodes to. The halves stay here: the installed file holds the classes of blocks that
/// share a lead or a tail, by their collision kind, mirroring and block numbers, and the terrain each pair of classes
/// makes (#1709).
/// </remarks>
public static class StraddledTerrainFiles
{
    /// <summary>Every mirroring, in the order the lead classes are listed within a collision kind.</summary>
    private static readonly BlockMirroring[] Mirrorings = Enum.GetValues<BlockMirroring>();

    /// <summary>Every collision kind, in the order the lead classes are listed.</summary>
    private static readonly RoomCollisionType[] Kinds = Enum.GetValues<RoomCollisionType>();

    /// <summary>How many block numbers a block set holds.</summary>
    private const int BlockCount = RoomBlockAppearance.LastBlock + 1;

    /// <summary>Every mirroring by its name.</summary>
    private static readonly Dictionary<string, BlockMirroring> MirroringsByName =
        Mirrorings.ToDictionary(mirroring => mirroring.ToString(), StringComparer.Ordinal);

    /// <summary>Every collision kind by its name.</summary>
    private static readonly Dictionary<string, RoomCollisionType> KindsByName =
        Kinds.ToDictionary(kind => kind.ToString(), StringComparer.Ordinal);

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
    /// giving its block's lead and tail; the blocks sharing a lead, and the block numbers sharing a tail, listed as
    /// classes; and every lead and tail decoded as the word they are stored as.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The codec does not store every terrain block as exactly one word, the start of a block depends on more than its
    /// number, or blocks of different collision kinds or mirrorings share a lead.
    /// </exception>
    internal static StraddledTerrainDocument Decode()
    {
        var leadOf = new Dictionary<RoomTerrainBlock, int>();
        int[] tailOf = Enumerable.Repeat(-1, BlockCount).ToArray();
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            RoomTerrainBlock terrain = RoomLevelWordCodec.Decode((ushort)value);
            if (!leadOf.TryAdd(terrain, RoomLevelWordCodec.SecondStoredHalf((ushort)value)))
                throw new InvalidOperationException($"Two level words decode to {terrain}.");
            int block = terrain.Appearance.Block;
            int tail = RoomLevelWordCodec.FirstStoredHalf((ushort)value);
            if (tailOf[block] >= 0 && tailOf[block] != tail)
                throw new InvalidOperationException($"The start of block {block} depends on more than its number.");
            tailOf[block] = tail;
        }
        if (leadOf.Count != Kinds.Length * Mirrorings.Length * BlockCount || tailOf.Contains(-1))
            throw new InvalidOperationException("A terrain block is stored as no level word.");

        // Each class is listed by what it holds, in the order of the first block it holds.
        int[][] tailClasses = [.. Enumerable.Range(0, BlockCount)
            .GroupBy(block => tailOf[block])
            .Select(blocks => blocks.Order().ToArray())
            .OrderBy(blocks => blocks[0])];
        int[] tailHalves = [.. tailClasses.Select(blocks => tailOf[blocks[0]])];
        var leads = new List<StraddledTerrainDocument.Lead>();
        foreach (IGrouping<int, RoomTerrainBlock> leadClass in leadOf.Keys
            .OrderBy(terrain => terrain.Collision).ThenBy(terrain => terrain.Appearance.Mirroring).ThenBy(terrain => terrain.Appearance.Block)
            .GroupBy(terrain => leadOf[terrain]))
        {
            RoomTerrainBlock first = leadClass.First();
            if (leadClass.Any(terrain => terrain.Collision != first.Collision || terrain.Appearance.Mirroring != first.Appearance.Mirroring))
                throw new InvalidOperationException($"Blocks of more than one collision kind or mirroring share the lead of {first}.");
            leads.Add(new StraddledTerrainDocument.Lead(
                first.Collision.ToString(),
                first.Appearance.Mirroring.ToString(),
                Runs(leadClass.Select(terrain => terrain.Appearance.Block)),
                [.. tailHalves.Select(tail => Outcome(RoomLevelWordCodec.Decode(RoomLevelWordCodec.OfStoredHalves(leadClass.Key, tail))))]));
        }
        return new StraddledTerrainDocument(
            StraddledTerrainFormat.Version,
            [.. tailClasses.Select(blocks => new StraddledTerrainDocument.Tail(Runs(blocks)))],
            leads);
    }

    /// <summary>
    /// Builds the straddled terrain from a document, requiring every block number in exactly one tail class, every block
    /// of every mirroring and collision kind in exactly one lead class, and a straddled block for every lead and tail.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The document is incomplete, places a block in two classes, or names a block, mirroring or collision kind the game
    /// does not have.
    /// </exception>
    internal static StraddledTerrain<RoomTerrainBlock> Read(StraddledTerrainDocument document)
    {
        Ensure.NotNull(document);
        if (document.Version != StraddledTerrainFormat.Version)
            throw new InvalidDataException("Straddled-terrain file has an unsupported version.");
        IReadOnlyList<StraddledTerrainDocument.Tail?> tailClasses = Require(document.Tails, "tails");
        int[] tailOf = Enumerable.Repeat(-1, BlockCount).ToArray();
        for (int tail = 0; tail < tailClasses.Count; tail++)
        {
            foreach (int block in Blocks(Require(tailClasses[tail], $"tail class {tail}").Blocks, $"tail class {tail}"))
            {
                if (tailOf[block] >= 0)
                    throw new InvalidDataException($"Straddled-terrain block {block} lies in two tail classes.");
                tailOf[block] = tail;
            }
        }
        if (tailOf.Contains(-1))
            throw new InvalidDataException("Straddled-terrain tail classes must hold every block number.");

        IReadOnlyList<StraddledTerrainDocument.Lead?> leadClasses = Require(document.Leads, "leads");
        var leads = new Dictionary<RoomTerrainBlock, int>();
        var tails = new Dictionary<RoomTerrainBlock, int>();
        var outcomes = new IReadOnlyList<RoomTerrainBlock>[leadClasses.Count];
        for (int lead = 0; lead < leadClasses.Count; lead++)
        {
            StraddledTerrainDocument.Lead leadClass = Require(leadClasses[lead], $"lead class {lead}");
            RoomCollisionType kind = Named(KindsByName, leadClass.Collision);
            BlockMirroring mirroring = Named(MirroringsByName, leadClass.Mirroring);
            foreach (int block in Blocks(leadClass.Blocks, $"lead class {lead}"))
            {
                RoomTerrainBlock terrain = RoomTerrainBlock.Of(block, mirroring, kind);
                if (!leads.TryAdd(terrain, lead))
                    throw new InvalidDataException($"Straddled-terrain block {terrain} lies in two lead classes.");
                // The start of a block contributes the same whatever its mirroring and kind.
                tails.Add(terrain, tailOf[block]);
            }
            IReadOnlyList<StraddledTerrainDocument.Outcome?> straddles = Require(leadClass.Straddles, $"straddles of lead class {lead}");
            if (straddles.Count != tailClasses.Count)
                throw new InvalidDataException($"Straddled-terrain lead class {lead} makes terrain with {straddles.Count} tail classes, not {tailClasses.Count}.");
            outcomes[lead] = [.. straddles.Select(Terrain)];
        }
        if (leads.Count != Kinds.Length * Mirrorings.Length * BlockCount)
            throw new InvalidDataException("Straddled-terrain lead classes must hold every block of every mirroring and collision kind.");
        try
        {
            return new StraddledTerrain<RoomTerrainBlock>(leads, tails, outcomes);
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException($"Straddled-terrain file is incomplete: {error.Message}", error);
        }
    }

    /// <summary>The runs of consecutive block numbers <paramref name="blocks"/> holds, lowest first.</summary>
    private static StraddledTerrainDocument.BlockRun[] Runs(IEnumerable<int> blocks)
    {
        var runs = new List<StraddledTerrainDocument.BlockRun>();
        foreach (int block in blocks.Order())
        {
            if (runs.Count > 0 && runs[^1].Last + 1 == block)
                runs[^1] = runs[^1] with { Last = block };
            else
                runs.Add(new StraddledTerrainDocument.BlockRun(block, block));
        }
        return [.. runs];
    }

    /// <summary>Every block number a class's runs hold.</summary>
    /// <exception cref="InvalidDataException">The class holds no runs, or a run is empty or names a block no block set holds.</exception>
    private static IEnumerable<int> Blocks(IReadOnlyList<StraddledTerrainDocument.BlockRun?>? runs, string what)
    {
        if (runs is null || runs.Count == 0)
            throw new InvalidDataException($"Straddled-terrain {what} holds no blocks.");
        foreach (StraddledTerrainDocument.BlockRun? run in runs)
        {
            if (run is null || run.First < 0 || run.Last < run.First || run.Last > RoomBlockAppearance.LastBlock)
                throw new InvalidDataException($"Straddled-terrain {what} holds a run of blocks no block set holds.");
            for (int block = run.First; block <= run.Last; block++)
                yield return block;
        }
    }

    /// <summary>How the file names a straddled terrain block.</summary>
    private static StraddledTerrainDocument.Outcome Outcome(RoomTerrainBlock terrain) =>
        new(terrain.Appearance.Block, terrain.Appearance.Mirroring.ToString(), terrain.Collision.ToString());

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
            : throw new InvalidDataException($"Straddled-terrain file names {typeof(T).Name} '{name}', which the game does not have.");

    /// <summary>Requires a document member the file must hold.</summary>
    /// <exception cref="InvalidDataException">The file holds no such member.</exception>
    private static T Require<T>(T? value, string name) where T : class =>
        value ?? throw new InvalidDataException($"Straddled-terrain file has no {name}.");
}
