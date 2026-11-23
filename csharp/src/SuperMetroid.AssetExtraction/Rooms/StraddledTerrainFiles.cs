using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Rooms.Artwork;
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
/// makes (#1709). A block a collectible that shows its own picture writes under it is stored as the number the import
/// gives it (<see cref="CollectiblePairBlockNumbers"/>), so the file names the block it straddles as (#2354).
/// </remarks>
public static partial class StraddledTerrainFiles
{
    /// <summary>Every mirroring, in the order the lead classes are listed within a collision kind (#2567).</summary>
    private static readonly WrittenOrder<BlockMirroring> Mirrorings = new(Ensure.EveryMemberOnce(BlockMirroringKeys.ListedOrder));

    /// <summary>Every collision kind, in the order the lead classes are listed (#2567).</summary>
    private static readonly WrittenOrder<RoomCollisionType> Kinds = new(Ensure.EveryMemberOnce(RoomCollisionTypeKeys.ListedOrder));

    /// <summary>How many block numbers a level word names; the file names blocks by these numbers (#2100).</summary>
    private const int BlockCount = RoomLevelWordCodec.BlockNumbers;

    /// <summary>Every mirroring by its name.</summary>
    private static readonly Dictionary<string, BlockMirroring> MirroringsByName =
        Mirrorings.ToDictionary(mirroring => BlockMirroringKeys.Installed.Of(mirroring), StringComparer.Ordinal);

    /// <summary>Every collision kind by its name.</summary>
    private static readonly Dictionary<string, RoomCollisionType> KindsByName =
        Kinds.ToDictionary(kind => RoomCollisionTypeKeys.Installed.Of(kind), StringComparer.Ordinal);

    /// <summary>Every block pair by its name.</summary>
    private static readonly Dictionary<string, CollectibleBlockPairs.Pair> PairsByName =
        Enum.GetValues<CollectibleBlockPairs.Pair>().ToDictionary(pair => CollectibleBlockPairKeys.Installed.Of(pair), StringComparer.Ordinal);

    /// <summary>Every collectible look by its name.</summary>
    private static readonly Dictionary<string, CollectibleLook> LooksByName =
        Enum.GetValues<CollectibleLook>().ToDictionary(look => CollectibleLookKeys.Installed.Of(look), StringComparer.Ordinal);

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
            JsonSerializer.SerializeToUtf8Bytes(Decode(), global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.Rooms.StraddledTerrainDocument>(StraddledTerrainFormat.JsonOptions)));
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
    /// classes; every lead and tail decoded as the word they are stored as; and each block a collectible that shows its own
    /// picture writes under it named with the number it is stored as.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The codec does not store every terrain block as exactly one word, the start of a block depends on more than its
    /// number, or blocks of different collision kinds or mirrorings share a lead.
    /// </exception>
    internal static StraddledTerrainDocument Decode()
    {
        var leadOf = new Dictionary<RoomTerrainBlock, int>();
        // Each block's tail, once a level word stores it.
        var storedTail = new Dictionary<int, int>();
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            RoomTerrainBlock terrain = RoomLevelWordCodec.Decode((ushort)value);
            if (!leadOf.TryAdd(terrain, RoomLevelWordCodec.SecondStoredHalf((ushort)value)))
                throw new InvalidOperationException($"Two level words decode to {terrain}.");
            int block = RoomLevelWordCodec.BlockNumberOf(terrain.Appearance);
            int tail = RoomLevelWordCodec.FirstStoredHalf((ushort)value);
            if (storedTail.TryGetValue(block, out int known) && known != tail)
                throw new InvalidOperationException($"The start of block {block} depends on more than its number.");
            storedTail[block] = tail;
        }
        if (leadOf.Count != Kinds.Count * Mirrorings.Count * BlockCount || storedTail.Count != BlockCount)
            throw new InvalidOperationException("A terrain block is stored as no level word.");
        int[] tailOf = [.. Enumerable.Range(0, BlockCount).Select(block => storedTail[block])];

        // Each class is listed by what it holds, in the order of the first block it holds.
        int[][] tailClasses = [.. Enumerable.Range(0, BlockCount)
            .GroupBy(block => tailOf[block])
            .Select(blocks => blocks.Order().ToArray())
            .OrderBy(blocks => blocks[0])];
        var leads = new List<StraddledTerrainDocument.Lead>();
        foreach (IGrouping<int, RoomTerrainBlock> leadClass in leadOf.Keys
            .OrderBy(terrain => Kinds.PlaceOf(terrain.Collision)).ThenBy(terrain => Mirrorings.PlaceOf(terrain.Appearance.Mirroring))
            .ThenBy(terrain => RoomLevelWordCodec.BlockNumberOf(terrain.Appearance))
            .GroupBy(terrain => leadOf[terrain]))
        {
            RoomTerrainBlock first = leadClass.First();
            if (leadClass.Any(terrain => terrain.Collision != first.Collision || terrain.Appearance.Mirroring != first.Appearance.Mirroring))
                throw new InvalidOperationException($"Blocks of more than one collision kind or mirroring share the lead of {first}.");
            leads.Add(new StraddledTerrainDocument.Lead(
                RoomCollisionTypeKeys.Installed.Of(first.Collision),
                BlockMirroringKeys.Installed.Of(first.Appearance.Mirroring),
                Runs(leadClass.Select(terrain => RoomLevelWordCodec.BlockNumberOf(terrain.Appearance))),
                tailClasses.ToDictionary(blocks => TailName(blocks[0]),
                    blocks => Outcome(RoomLevelWordCodec.Decode(RoomLevelWordCodec.OfStoredHalves(leadClass.Key, tailOf[blocks[0]]))),
                    StringComparer.Ordinal)));
        }
        return new StraddledTerrainDocument(
            StraddledTerrainFormat.Version,
            [.. tailClasses.Select(blocks => new StraddledTerrainDocument.Tail(TailName(blocks[0]), Runs(blocks)))],
            leads,
            [.. CollectiblePairBlock.All.Select(block => new StraddledTerrainDocument.CollectibleBlock(
                CollectibleBlockPairKeys.Installed.Of(block.Pair), CollectibleLookKeys.Installed.Of(block.Look), CollectiblePairBlockNumbers.Of(block)))]);
    }

    /// <summary>
    /// Builds the straddled terrain from a document, requiring every block number in exactly one tail class, every block
    /// of every mirroring and collision kind in exactly one lead class, a straddled block for every lead and tail, and
    /// every block a collectible that shows its own picture writes under it named once, which straddles, in every
    /// mirroring and collision kind, as the block it names.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The document is incomplete, places a block in two classes, names a collectible's block twice, or names a block,
    /// mirroring, collision kind, block pair or look the game does not have.
    /// </exception>
    internal static StraddledTerrain<RoomTerrainBlock> Read(StraddledTerrainDocument document)
    {
        Ensure.NotNull(document);
        if (document.Version != StraddledTerrainFormat.Version)
            throw new InvalidDataException("Straddled-terrain file has an unsupported version.");
        var tailsByName = new Dictionary<string, StraddledTerrainTail>(StringComparer.Ordinal);
        // Each block number's tail class, once a class claims it.
        var claimedBy = new Dictionary<int, StraddledTerrainTail>();
        foreach (StraddledTerrainDocument.Tail tailClass in document.Tails)
        {
            var tail = new StraddledTerrainTail(tailClass.Name);
            if (!tailsByName.TryAdd(tail.Name, tail))
                throw new InvalidDataException($"Straddled-terrain file names two tail classes {tail.Name}.");
            foreach (int block in Blocks(tailClass.Blocks, $"tail class {tail.Name}"))
            {
                if (!claimedBy.TryAdd(block, tail))
                    throw new InvalidDataException($"Straddled-terrain block {block} lies in two tail classes.");
            }
        }
        if (claimedBy.Count != BlockCount)
            throw new InvalidDataException("Straddled-terrain tail classes must hold every block number.");
        StraddledTerrainTail[] tailOf = [.. Enumerable.Range(0, BlockCount).Select(block => claimedBy[block])];

        var leads = new Dictionary<RoomTerrainBlock, StraddledTerrainLead<RoomTerrainBlock>>();
        var tails = new Dictionary<RoomTerrainBlock, StraddledTerrainTail>();
        foreach (StraddledTerrainDocument.Lead leadClass in document.Leads)
        {
            RoomCollisionType kind = Named(KindsByName, leadClass.Collision);
            BlockMirroring mirroring = Named(MirroringsByName, leadClass.Mirroring);
            IReadOnlyDictionary<string, StraddledTerrainDocument.Outcome> straddles = leadClass.Straddles;
            if (straddles.Count != tailsByName.Count || tailsByName.Keys.Any(name => !straddles.ContainsKey(name)))
                throw new InvalidDataException($"Straddled-terrain lead class of {leadClass.Collision} {leadClass.Mirroring} blocks must make terrain with exactly each tail class.");
            var lead = new StraddledTerrainLead<RoomTerrainBlock>(tailsByName.ToDictionary(pair => pair.Value, pair => Terrain(straddles[pair.Key])));
            foreach (int block in Blocks(leadClass.Blocks, $"lead class of {leadClass.Collision} {leadClass.Mirroring} blocks"))
            {
                var terrain = new RoomTerrainBlock(RoomLevelWordCodec.AppearanceOf(block, mirroring), kind);
                if (!leads.TryAdd(terrain, lead))
                    throw new InvalidDataException($"Straddled-terrain block {terrain} lies in two lead classes.");
                // The start of a block contributes the same whatever its mirroring and kind.
                tails.Add(terrain, tailOf[block]);
            }
        }
        if (leads.Count != Kinds.Count * Mirrorings.Count * BlockCount)
            throw new InvalidDataException("Straddled-terrain lead classes must hold every block of every mirroring and collision kind.");
        AddCollectibleBlocks(document.CollectibleBlocks, tailOf, leads, tails);
        try
        {
            return new StraddledTerrain<RoomTerrainBlock>(leads, tails);
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException($"Straddled-terrain file is incomplete: {error.Message}", error);
        }
    }

    /// <summary>
    /// Adds each block a collectible that shows its own picture writes under it, in every mirroring and collision kind,
    /// with the lead and tail of the numbered block it straddles as (#2354).
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// A collectible's block is named twice or not at all, or an entry names a block pair, look or block the game does not have.
    /// </exception>
    private static void AddCollectibleBlocks(IReadOnlyList<StraddledTerrainDocument.CollectibleBlock> entries, StraddledTerrainTail[] tailOf,
        Dictionary<RoomTerrainBlock, StraddledTerrainLead<RoomTerrainBlock>> leads, Dictionary<RoomTerrainBlock, StraddledTerrainTail> tails)
    {
        var named = new HashSet<CollectiblePairBlock>();
        foreach (StraddledTerrainDocument.CollectibleBlock block in entries)
        {
            var pairBlock = new CollectiblePairBlock(Named(PairsByName, block.Pair), Named(LooksByName, block.Look));
            if (!named.Add(pairBlock))
                throw new InvalidDataException($"Straddled-terrain file names the {pairBlock} twice.");
            if (block.StraddlesAs is < 0 or >= BlockCount)
                throw new InvalidDataException($"Straddled-terrain file has the {pairBlock} straddle as block {block.StraddlesAs}, which no block set holds.");
            foreach (RoomCollisionType kind in Kinds)
            {
                foreach (BlockMirroring mirroring in Mirrorings)
                {
                    var numbered = new RoomTerrainBlock(RoomLevelWordCodec.AppearanceOf(block.StraddlesAs, mirroring), kind);
                    var written = new RoomTerrainBlock(RoomBlockAppearance.Of(pairBlock).WithMirroring(mirroring), kind);
                    leads.Add(written, leads[numbered]);
                    tails.Add(written, tailOf[block.StraddlesAs]);
                }
            }
        }
        if (named.Count != CollectiblePairBlock.All.Length)
            throw new InvalidDataException("Straddled-terrain file must name every block a collectible that shows its own picture writes.");
    }

    /// <summary>The name of the tail class whose lowest block number is <paramref name="firstBlock"/>.</summary>
    private static string TailName(int firstBlock) => string.Create(System.Globalization.CultureInfo.InvariantCulture, $"from-block-{firstBlock}");

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
    private static IEnumerable<int> Blocks(IReadOnlyList<StraddledTerrainDocument.BlockRun> runs, string what)
    {
        if (runs.Count == 0)
            throw new InvalidDataException($"Straddled-terrain {what} holds no blocks.");
        foreach (StraddledTerrainDocument.BlockRun run in runs)
        {
            if (run.First < 0 || run.Last < run.First || run.Last >= BlockCount)
                throw new InvalidDataException($"Straddled-terrain {what} holds a run of blocks no block set holds.");
            for (int block = run.First; block <= run.Last; block++)
                yield return block;
        }
    }

    /// <summary>How the file names a straddled terrain block.</summary>
    private static StraddledTerrainDocument.Outcome Outcome(RoomTerrainBlock terrain) =>
        new(RoomLevelWordCodec.BlockNumberOf(terrain.Appearance), BlockMirroringKeys.Installed.Of(terrain.Appearance.Mirroring),
            RoomCollisionTypeKeys.Installed.Of(terrain.Collision));

    /// <summary>The terrain block an outcome names.</summary>
    /// <exception cref="InvalidDataException">The outcome names a block, mirroring or collision kind the game does not have.</exception>
    private static RoomTerrainBlock Terrain(StraddledTerrainDocument.Outcome outcome)
    {
        if (outcome.Block is < 0 or >= BlockCount)
            throw new InvalidDataException($"Straddled-terrain outcome names block {outcome.Block}, which no block set holds.");
        return new RoomTerrainBlock(RoomLevelWordCodec.AppearanceOf(outcome.Block, Named(MirroringsByName, outcome.Mirroring)),
            Named(KindsByName, outcome.Collision));
    }

    /// <summary>The value <paramref name="name"/> names exactly.</summary>
    /// <exception cref="InvalidDataException">No value has that name.</exception>
    private static T Named<T>(Dictionary<string, T> byName, string name) where T : struct, Enum =>
        byName.TryGetValue(name, out T value)
            ? value
            : throw new InvalidDataException($"Straddled-terrain file names {typeof(T).Name} '{name}', which the game does not have.");

}
