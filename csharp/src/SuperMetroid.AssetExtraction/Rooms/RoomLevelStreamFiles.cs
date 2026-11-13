using SuperMetroid.Core;
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Installs the typed terrain of every retail room level (#1469): each level's grid of terrain blocks, behaviours and
/// background appearances, and the terrain past its grid, decoded once from the cartridge by
/// <see cref="RoomLevelStreamDecoder"/>. The family is not artwork: editing the installed artwork cannot
/// alter terrain, behaviours, slopes, hazards or the terrain beyond a room.
/// </summary>
/// <remarks>
/// Each level is one little-endian file: the grid's width and height in blocks, how many decoded blocks follow the grid,
/// and how many blocks the level space holds past the grid (all four as 32-bit integers); then each foreground block as
/// its collision kind, block number, mirroring and behaviour; each background block as its block number and mirroring;
/// and each following block as its collision kind, block number and mirroring. A collision kind is written as its
/// position in the collision-kind document's <c>collisionKinds</c>, which names the kinds; mirroring is written as
/// <see cref="MirroringCodes"/>'s position of its value.
/// <para>
/// A foreground block's behaviour is written as the reading its collision kind takes of it (#1709, #1722), one byte per
/// named field: a reacting kind (shootable, bombable, special, spike and grapple blocks, and their air siblings) writes its
/// reaction as its position in the collision-kind document's <c>reactions</c> list for the kind's family, which names
/// the reactions; a slope writes its shape, 0 through 31, then its mirroring's code; an extension writes its offset as a
/// signed byte, -128 through 127; a door writes the entry of the room's door list it selects, 0 through 127; air,
/// unrevealed air and solid blocks write nothing and hold <see cref="RoomBlockBehavior.None"/>. The load rebuilds every
/// other reading from the one written, as the game reads one behaviour under every kind
/// (<see cref="SuperMetroid.Core.Compatibility.BlockParameterCarryOver{TBehavior, TReading}"/>).
/// </para>
/// </remarks>
public static class RoomLevelStreamFiles
{
    /// <summary>
    /// The family's document naming the collision kinds, in the order the level files write them by position; the levels
    /// are the retail rooms' (#1693), each in its own file.
    /// </summary>
    public const string CollisionKindsFileName = "room-level-streams.json";
    /// <summary>
    /// Collision-kind document schema version this loader accepts; version 3 installs typed blocks instead of decompressed
    /// level data, 4 names the collision kinds alone, with no level list, hash or cartridge digest (#1693), 5 writes each
    /// block's behaviour as the reading its collision kind takes instead of the game's behaviour byte (#1709), and 6 names
    /// each reacting kind's reactions, written by position, instead of a list code and an entry, and names unrevealed air
    /// (#1722).
    /// </summary>
    private const int FormatVersion = 6;

    /// <summary>The collision kinds in the order the level files write them by position.</summary>
    private static readonly ImmutableArray<RoomCollisionType> CollisionKinds = [.. Enum.GetValues<RoomCollisionType>()];

    /// <summary>Each mirroring in the order the level files write it by position.</summary>
    private static readonly ImmutableArray<BlockMirroring> MirroringCodes =
        [BlockMirroring.None, BlockMirroring.Horizontal, BlockMirroring.Vertical, BlockMirroring.Horizontal | BlockMirroring.Vertical];

    /// <summary>The reaction families the collision-kind document names the reactions of.</summary>
    private static readonly ImmutableArray<BlockReactionFamily> Families = [.. Enum.GetValues<BlockReactionFamily>()];

    /// <summary>Serializer settings shared by the collision-kind document's writes and reads.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    /// <summary>Formats the stable per-level filename without validating the level.</summary>
    /// <param name="level">The level, shared by all room states using it.</param>
    /// <returns><c>level-</c> followed by the level's name and <c>.bin</c>.</returns>
    public static string LevelFileName(RoomLevelId level) => $"level-{level.Name}.bin";

    /// <summary>Decompresses and decodes every distinct retail room level and writes each as typed blocks.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for the compressed levels identified by <see cref="RoomVisualLayoutFiles.RetailLevels"/>.</param>
    /// <param name="directory">Family directory, created if absent; receives new per-level files and the collision-kind document.</param>
    /// <remarks>Create-new writes refuse existing files and do not roll back earlier outputs.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory is blank or the address space lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">A compressed level is invalid or malformed.</exception>
    /// <exception cref="IOException">An output exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        Directory.CreateDirectory(directory);
        foreach ((RoomLevelId level, int width) in RoomVisualLayoutFiles.RetailLevels.OrderBy(item => item.Key.Name, StringComparer.Ordinal))
        {
            byte[] stream = RomDataReader.Decompress(cartridge, RoomLevelSourceDefinitions.Addresses[level]);
            byte[] file = Encode(level, RoomLevelStreamDecoder.Decode(level, stream, width));
            using var output = new FileStream(Path.Combine(directory, LevelFileName(level)), FileMode.CreateNew, FileAccess.Write);
            output.Write(file);
        }
        using var kinds = new FileStream(Path.Combine(directory, CollisionKindsFileName), FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(kinds, new CollisionKindDocument(FormatVersion, [.. CollisionKinds.Select(kind => kind.ToString())],
            Families.ToDictionary(family => family.ToString(),
                family => BlockReactionFamilies.Reactions(family).Select(reaction => reaction.ToString()).ToArray())),
            JsonOptions);
    }

    /// <summary>Reads the collision kinds the level files name, then every retail level's file, and builds the catalog.</summary>
    /// <param name="directory">Family directory containing every retail level's file and the collision-kind document.</param>
    /// <returns>A ROM-independent catalog of every level's typed terrain.</returns>
    /// <exception cref="ArgumentException"><paramref name="directory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">The version or collision kinds are invalid, or a level's file is missing or its contents are invalid; the message names the file.</exception>
    /// <exception cref="IOException">The collision-kind document or a level file cannot be read.</exception>
    public static InstalledRoomLevelStreams Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string kindsPath = Path.Combine(directory, CollisionKindsFileName);
        if (!File.Exists(kindsPath))
            throw new InvalidDataException($"Room-level collision kinds {kindsPath} are missing.");
        CollisionKindDocument document;
        using (FileStream kindsStream = File.OpenRead(kindsPath))
            document = JsonAssetDocument.Read<CollisionKindDocument>(
                kindsStream, JsonOptions, $"room-level collision kinds {kindsPath}");
        if (document.Version != FormatVersion)
            throw new InvalidDataException(
                $"Room-level collision kinds {kindsPath} have unsupported version {document.Version}.");
        RoomCollisionType[] kinds = ReadKinds(document.CollisionKinds, kindsPath);
        Dictionary<BlockReactionFamily, Enum[]> reactions = ReadReactions(document.Reactions, kindsPath);

        // The levels are the retail rooms' (#1693): no manifest lists them.
        var levels = new Dictionary<RoomLevelId, InstalledRoomLevel>(RoomVisualLayoutFiles.RetailLevels.Count);
        foreach ((RoomLevelId level, int width) in RoomVisualLayoutFiles.RetailLevels.OrderBy(item => item.Key.Name, StringComparer.Ordinal))
        {
            string path = Path.Combine(directory, LevelFileName(level));
            if (!File.Exists(path))
                throw new InvalidDataException($"Room-level file {path} is missing.");
            // The level file describes its own size and is read to its end (#1532): an edited room's level may hold more or
            // fewer blocks than extraction wrote, so the length extraction recorded is not compared.
            byte[] file = File.ReadAllBytes(path);
            InstalledRoomLevel installed = Decode(file, kinds, reactions, path);
            if (installed.Size.X != width)
                throw new InvalidDataException($"Room-level file {path} is {installed.Size.X} blocks wide; its rooms are {width}.");
            levels.Add(level, installed);
        }
        return new InstalledRoomLevelStreams(levels, RoomVisualLayoutSourceDefinitions.All);
    }

    /// <summary>Writes one level as the installed file format.</summary>
    /// <exception cref="InvalidDataException">A block's behaviour is not the one its collision kind's reading rebuilds.</exception>
    private static byte[] Encode(RoomLevelId name, InstalledRoomLevel level)
    {
        using var memory = new MemoryStream();
        using (var writer = new BinaryWriter(memory))
        {
            writer.Write((int)level.Size.X);
            writer.Write((int)level.Size.Y);
            writer.Write(level.Following.Length);
            writer.Write(level.BeyondCount);
            for (int index = 0; index < level.Foreground.Length; index++)
            {
                WriteTerrain(writer, level.Foreground[index]);
                WriteBehavior(writer, level.Foreground[index].Collision, level.Behaviors[index], name, index);
            }
            foreach (RoomBlockAppearance appearance in level.Background)
                WriteAppearance(writer, appearance);
            foreach (RoomTerrainBlock block in level.Following)
                WriteTerrain(writer, block);
        }
        return memory.ToArray();
    }

    /// <summary>
    /// Writes a foreground block's behaviour as the reading its collision kind takes, requiring that reading to rebuild the
    /// whole behaviour, so nothing another kind would read of it is lost.
    /// </summary>
    /// <exception cref="InvalidDataException">The reading does not rebuild the behaviour.</exception>
    private static void WriteBehavior(BinaryWriter writer, RoomCollisionType kind, RoomBlockBehavior behavior,
        RoomLevelId level, int index)
    {
        RoomBlockBehavior rebuilt;
        switch (ReadingOf(kind))
        {
            case BehaviorReading.Reaction:
            {
                BlockReactionFamily family = BlockReactionFamilies.Of(kind) ??
                    throw new InvalidOperationException($"Collision kind {kind} reads no reaction.");
                Enum reaction = behavior.ReactionIn(family) ?? throw new InvalidDataException(
                    $"Room level {level} block {index} is {kind} with behaviour {behavior}, which gives its kind no reaction.");
                writer.Write(checked((byte)IndexOf(BlockReactionFamilies.Reactions(family), reaction)));
                rebuilt = RoomBlockBehavior.ReactingAs(reaction);
                break;
            }
            case BehaviorReading.Slope:
                writer.Write(checked((byte)behavior.Slope.Shape));
                writer.Write(checked((byte)MirroringCodes.IndexOf(behavior.Slope.Mirroring)));
                rebuilt = RoomBlockBehavior.Sloping(behavior.Slope);
                break;
            case BehaviorReading.Extension:
                writer.Write(checked((sbyte)behavior.ExtensionOffset));
                rebuilt = RoomBlockBehavior.Extending(behavior.ExtensionOffset);
                break;
            case BehaviorReading.DoorLink:
                writer.Write(checked((byte)behavior.DoorLink));
                rebuilt = RoomBlockBehavior.LinkingDoor(behavior.DoorLink);
                break;
            case BehaviorReading.Nothing:
                rebuilt = RoomBlockBehavior.None;
                break;
            default:
                throw new InvalidOperationException($"Undefined behaviour reading for collision kind {kind}.");
        }
        if (rebuilt != behavior)
            throw new InvalidDataException(
                $"Room level {level} block {index} is {kind} with behaviour {behavior}, which its kind's reading does not hold whole.");
    }

    /// <summary>Reads a foreground block's behaviour written by <see cref="WriteBehavior"/>, rebuilding every reading.</summary>
    /// <exception cref="InvalidDataException">The reaction or mirroring code is unknown.</exception>
    private static RoomBlockBehavior ReadBehavior(BinaryReader reader, RoomCollisionType kind,
        Dictionary<BlockReactionFamily, Enum[]> reactions)
    {
        switch (ReadingOf(kind))
        {
            case BehaviorReading.Reaction:
            {
                Enum[] named = reactions[BlockReactionFamilies.Of(kind) ??
                    throw new InvalidOperationException($"Collision kind {kind} reads no reaction.")];
                int reaction = reader.ReadByte();
                if (reaction >= named.Length)
                    throw new InvalidDataException($"Reaction code {reaction} is not one of the {named.Length} reactions the {kind} family names.");
                return RoomBlockBehavior.ReactingAs(named[reaction]);
            }
            case BehaviorReading.Slope:
            {
                int shape = reader.ReadByte();
                int mirroring = reader.ReadByte();
                if (mirroring >= MirroringCodes.Length)
                    throw new InvalidDataException($"Mirroring code {mirroring} is not one of the {MirroringCodes.Length} codes.");
                return RoomBlockBehavior.Sloping(new SlopeGeometry(shape, MirroringCodes[mirroring]));
            }
            case BehaviorReading.Extension:
                return RoomBlockBehavior.Extending(reader.ReadSByte());
            case BehaviorReading.DoorLink:
                return RoomBlockBehavior.LinkingDoor(reader.ReadByte());
            case BehaviorReading.Nothing:
                return RoomBlockBehavior.None;
            default:
                throw new InvalidOperationException($"Undefined behaviour reading for collision kind {kind}.");
        }
    }

    /// <summary>The reading a collision kind takes of its block's behaviour, which decides how the level files write it.</summary>
    private static BehaviorReading ReadingOf(RoomCollisionType kind) => kind switch
    {
        RoomCollisionType.Slope => BehaviorReading.Slope,
        RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension => BehaviorReading.Extension,
        RoomCollisionType.DoorBlock => BehaviorReading.DoorLink,
        RoomCollisionType.SpikeAir or RoomCollisionType.SpecialAir or RoomCollisionType.ShootableAir or
            RoomCollisionType.BombableAir or RoomCollisionType.SpikeBlock or RoomCollisionType.SpecialBlock or
            RoomCollisionType.ShootableBlock or RoomCollisionType.GrappleBlock or RoomCollisionType.BombableBlock =>
            BehaviorReading.Reaction,
        RoomCollisionType.Air or RoomCollisionType.UnrevealedAir or RoomCollisionType.SolidBlock => BehaviorReading.Nothing,
        _ => throw new InvalidOperationException($"Undefined collision kind {kind}."),
    };

    /// <summary>Writes a terrain block: its collision kind's position, then its appearance.</summary>
    private static void WriteTerrain(BinaryWriter writer, RoomTerrainBlock block)
    {
        writer.Write(checked((byte)CollisionKinds.IndexOf(block.Collision)));
        WriteAppearance(writer, block.Appearance);
    }

    /// <summary>Writes an appearance: its block number, then its mirroring's code.</summary>
    private static void WriteAppearance(BinaryWriter writer, RoomBlockAppearance appearance)
    {
        writer.Write(checked((ushort)appearance.Block));
        writer.Write(checked((byte)MirroringCodes.IndexOf(appearance.Mirroring)));
    }

    /// <summary>Reads one installed level file.</summary>
    /// <exception cref="InvalidDataException">The file is truncated, has trailing bytes, or holds an unknown kind, mirroring, block or reaction list, or a reading out of its range.</exception>
    private static InstalledRoomLevel Decode(byte[] file, RoomCollisionType[] kinds,
        Dictionary<BlockReactionFamily, Enum[]> reactions, string path)
    {
        try
        {
            using var reader = new BinaryReader(new MemoryStream(file, writable: false));
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            int followingCount = reader.ReadInt32();
            int beyondCount = reader.ReadInt32();
            var size = new Extent16<RoomBlocks>(checked((ushort)width), checked((ushort)height));
            int blocks = size.Area;
            var foreground = ImmutableArray.CreateBuilder<RoomTerrainBlock>(blocks);
            var behaviors = ImmutableArray.CreateBuilder<RoomBlockBehavior>(blocks);
            for (int index = 0; index < blocks; index++)
            {
                RoomTerrainBlock block = ReadTerrain(reader, kinds);
                foreground.Add(block);
                behaviors.Add(ReadBehavior(reader, block.Collision, reactions));
            }
            var background = ImmutableArray.CreateBuilder<RoomBlockAppearance>(blocks);
            for (int index = 0; index < blocks; index++)
                background.Add(ReadAppearance(reader));
            var following = ImmutableArray.CreateBuilder<RoomTerrainBlock>(followingCount);
            for (int index = 0; index < followingCount; index++)
                following.Add(ReadTerrain(reader, kinds));
            if (reader.BaseStream.Position != file.Length)
                throw new InvalidDataException($"Room-level file {path} has trailing bytes.");
            return new InstalledRoomLevel(size, foreground.MoveToImmutable(), behaviors.MoveToImmutable(),
                background.MoveToImmutable(), following.MoveToImmutable(), beyondCount);
        }
        catch (Exception error) when (error is EndOfStreamException or OverflowException or ArgumentException)
        {
            throw new InvalidDataException($"Room-level file {path} is malformed: {error.Message}", error);
        }
    }

    /// <summary>Reads a terrain block written by <see cref="WriteTerrain"/>.</summary>
    private static RoomTerrainBlock ReadTerrain(BinaryReader reader, RoomCollisionType[] kinds)
    {
        int kind = reader.ReadByte();
        if (kind >= kinds.Length)
            throw new InvalidDataException($"Collision kind {kind} is not among the {kinds.Length} kinds the collision-kind document names.");
        return new RoomTerrainBlock(ReadAppearance(reader), kinds[kind]);
    }

    /// <summary>Reads an appearance written by <see cref="WriteAppearance"/>.</summary>
    private static RoomBlockAppearance ReadAppearance(BinaryReader reader)
    {
        int block = reader.ReadUInt16();
        int mirroring = reader.ReadByte();
        if (mirroring >= MirroringCodes.Length)
            throw new InvalidDataException($"Mirroring code {mirroring} is not one of the {MirroringCodes.Length} codes.");
        return new RoomBlockAppearance(block, MirroringCodes[mirroring]);
    }

    /// <summary>Reads the document's collision kinds by name, requiring every kind exactly once.</summary>
    private static RoomCollisionType[] ReadKinds(string[]? names, string kindsPath)
    {
        if (names is null || names.Length != CollisionKinds.Length)
            throw new InvalidDataException($"Room-level collision kinds {kindsPath} do not name the {CollisionKinds.Length} collision kinds.");
        var kinds = new RoomCollisionType[names.Length];
        for (int index = 0; index < names.Length; index++)
        {
            if (!Enum.TryParse(names[index], ignoreCase: false, out kinds[index]) || !Enum.IsDefined(kinds[index]))
                throw new InvalidDataException($"Room-level collision kinds {kindsPath} name unknown collision kind '{names[index]}'.");
        }
        if (kinds.Distinct().Count() != kinds.Length)
            throw new InvalidDataException($"Room-level collision kinds {kindsPath} repeat a collision kind.");
        return kinds;
    }

    /// <summary>
    /// Reads the document's reactions by name for every family, requiring each family's every reaction exactly once, in
    /// the order the level files write them by position.
    /// </summary>
    private static Dictionary<BlockReactionFamily, Enum[]> ReadReactions(Dictionary<string, string[]>? names, string kindsPath)
    {
        if (names is null || names.Count != Families.Length)
            throw new InvalidDataException($"Room-level collision kinds {kindsPath} do not name the reactions of the {Families.Length} reaction families.");
        var reactions = new Dictionary<BlockReactionFamily, Enum[]>(Families.Length);
        foreach (BlockReactionFamily family in Families)
        {
            IReadOnlyList<Enum> all = BlockReactionFamilies.Reactions(family);
            if (!names.TryGetValue(family.ToString(), out string[]? familyNames) || familyNames is null ||
                familyNames.Length != all.Count)
                throw new InvalidDataException($"Room-level collision kinds {kindsPath} do not name the {all.Count} {family} reactions.");
            var named = new Enum[familyNames.Length];
            for (int index = 0; index < familyNames.Length; index++)
            {
                named[index] = all.FirstOrDefault(reaction => string.Equals(reaction.ToString(), familyNames[index], StringComparison.Ordinal)) ??
                    throw new InvalidDataException($"Room-level collision kinds {kindsPath} name unknown {family} reaction '{familyNames[index]}'.");
            }
            if (named.Distinct().Count() != named.Length)
                throw new InvalidDataException($"Room-level collision kinds {kindsPath} repeat a {family} reaction.");
            reactions.Add(family, named);
        }
        return reactions;
    }

    /// <summary>The position of <paramref name="reaction"/> among <paramref name="reactions"/>.</summary>
    private static int IndexOf(IReadOnlyList<Enum> reactions, Enum reaction)
    {
        for (int index = 0; index < reactions.Count; index++)
        {
            if (reactions[index].Equals(reaction))
                return index;
        }
        throw new InvalidOperationException($"Reaction {reaction} is not among its family's reactions.");
    }

    /// <summary>The readings a collision kind takes of its block's behaviour, which decide how the level files write it.</summary>
    private enum BehaviorReading
    {
        /// <summary>The kind reads no behaviour; its block holds <see cref="RoomBlockBehavior.None"/>.</summary>
        Nothing,

        /// <summary>The kind reads its family's reaction.</summary>
        Reaction,

        /// <summary>The kind reads a slope's shape and mirroring.</summary>
        Slope,

        /// <summary>The kind reads a signed offset to the block it collides as.</summary>
        Extension,

        /// <summary>The kind reads an entry of the room's door list.</summary>
        DoorLink,
    }

    /// <summary>
    /// The collision kinds, and each reaction family's reactions, by the position the level files write them at, under the
    /// document's schema revision.
    /// </summary>
    private sealed record CollisionKindDocument(int Version, string[] CollisionKinds, Dictionary<string, string[]> Reactions);
}
