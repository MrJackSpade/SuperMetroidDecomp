using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed pictures of the blocks room-object setups and block reactions write of their own accord (#1580): the
/// bomb, crumble, Speed Booster, shot, Power Bomb and Super Missile blocks' parents, the struck rescue wall and the
/// Wrecked Ship treadmill's row. At install the import reads each from the level word its setup writes and checks the
/// word's collision against the collision the game gives it; the installed file holds what each shows, which the player
/// edits in place. The game reads the pictures as installed: there is no compiled copy and no override.
/// </summary>
public static class RoomObjectSetupBlockFiles
{
    /// <summary>The installed JSON filename of the setup-block pictures.</summary>
    public const string VisualFileName = "setup-blocks.json";

    /// <summary>The installed document's format; a document naming another is rejected and the content is extracted again.</summary>
    /// <remarks>Revision 2 names no cartridge (#1693): the installation receipt does.</remarks>
    internal const int FormatVersion = 2;

    /// <summary>Camel-case field names, mirroring by name and readable output for the installed document.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Pictures every setup block from the level words its setups write and writes the installed file.</summary>
    /// <param name="bus">The cartridge the setups' level words are read from.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">
    /// A setup's word collides differently from the block the game writes, two setups of one block disagree, or a block
    /// that keeps the struck block's mirroring is mirrored.
    /// </exception>
    /// <exception cref="IOException">The file already exists or cannot be written.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        var entries = new List<BlockEntry>();
        foreach (RoomObjectSetupBlock block in Enum.GetValues<RoomObjectSetupBlock>())
        {
            RoomBlockAppearance shown = Picture(bus, block);
            entries.Add(new BlockEntry(Id(block), shown.Block, shown.Mirroring));
        }
        Directory.CreateDirectory(directory);
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new BlockDocument(FormatVersion, entries.ToArray()), JsonOptions);
        using var output = new FileStream(Path.Combine(directory, VisualFileName), FileMode.CreateNew, FileAccess.Write);
        output.Write(json);
    }

    /// <summary>Loads the installed setup-block pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">
    /// The file's format is not this importer's, a block is missing, repeated or unknown, shows an invalid
    /// block, or is mirrored where it keeps the struck block's mirroring.
    /// </exception>
    /// <exception cref="IOException">The file cannot be read.</exception>
    public static InstalledRoomObjectSetupBlocks Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string path = Path.Combine(directory, VisualFileName);
        BlockDocument document = Read(path);
        try
        {
            return Create(document);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Invalid room-object setup-block pictures {path}: {error.Message}", error);
        }
    }

    /// <summary>The installed identity of a setup block.</summary>
    internal static string Id(RoomObjectSetupBlock block) => block switch
    {
        RoomObjectSetupBlock.BombBlockParent => "bomb-block-parent",
        RoomObjectSetupBlock.CrumbleBlockParent => "crumble-block-parent",
        RoomObjectSetupBlock.SpeedBoosterBlockParent => "speed-booster-block-parent",
        RoomObjectSetupBlock.ShotBlockParent => "shot-block-parent",
        RoomObjectSetupBlock.PowerBombBlockParent => "power-bomb-block-parent",
        RoomObjectSetupBlock.SuperMissileBlockParent => "super-missile-block-parent",
        RoomObjectSetupBlock.CritterWallStruck => "critter-wall-struck",
        RoomObjectSetupBlock.TreadmillIdle => "treadmill-idle",
        RoomObjectSetupBlock.TreadmillRunning => "treadmill-running",
        _ => throw new ArgumentOutOfRangeException(nameof(block), block, "Undefined room-object setup block."),
    };

    /// <summary>
    /// The collision the level word holding <paramref name="block"/> carries: the treadmill's row words carry the row's own
    /// collision; every other setup combines the block it substitutes with the struck block's collision, so its word
    /// carries none of its own.
    /// </summary>
    private static RoomCollisionType WordCollision(RoomObjectSetupBlock block) => block switch
    {
        RoomObjectSetupBlock.TreadmillIdle => WreckedShipTreadmillBlockDefinitions.IdleCollision,
        RoomObjectSetupBlock.TreadmillRunning => WreckedShipTreadmillBlockDefinitions.RunningCollision,
        RoomObjectSetupBlock.BombBlockParent or RoomObjectSetupBlock.CrumbleBlockParent or RoomObjectSetupBlock.SpeedBoosterBlockParent
            or RoomObjectSetupBlock.ShotBlockParent or RoomObjectSetupBlock.PowerBombBlockParent
            or RoomObjectSetupBlock.SuperMissileBlockParent or RoomObjectSetupBlock.CritterWallStruck => RoomCollisionType.Air,
        _ => throw new ArgumentOutOfRangeException(nameof(block), block, "Undefined room-object setup block."),
    };

    /// <summary>Whether the game shows <paramref name="block"/> with the struck block's own mirroring, so its picture names a block only.</summary>
    private static bool KeepsStruckMirroring(RoomObjectSetupBlock block) =>
        block is RoomObjectSetupBlock.CrumbleBlockParent or RoomObjectSetupBlock.SpeedBoosterBlockParent;

    /// <summary>
    /// Reads every level word that holds <paramref name="block"/>, checks each carries the collision the game gives it
    /// and that they agree, and pictures what the block shows.
    /// </summary>
    /// <exception cref="InvalidDataException">A word collides differently, the words disagree, or the block is mirrored where it may not be.</exception>
    private static RoomBlockAppearance Picture(ISnesAddressSpace bus, RoomObjectSetupBlock block)
    {
        if (!RoomObjectSetupBlockSourceDefinitions.Words.TryGetValue(block, out IReadOnlyList<SnesAddress>? words) || words.Count == 0)
            throw new InvalidOperationException($"Room-object setup block {block} has no level word.");
        RoomBlockAppearance? shown = null;
        foreach (SnesAddress address in words)
        {
            RoomTerrainBlock written = RoomLevelWordCodec.Decode(CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank((int)address));
            if (written.Collision != WordCollision(block))
                throw new InvalidDataException($"Room-object setup block {block} collides differently at {address}.");
            if (shown is { } first && first != written.Appearance)
                throw new InvalidDataException($"Room-object setup block {block} shows a different block at {address}.");
            shown = written.Appearance;
        }
        RoomBlockAppearance picture = shown ?? throw new InvalidOperationException($"Room-object setup block {block} has no level word.");
        if (KeepsStruckMirroring(block) && picture.Mirroring != BlockMirroring.None)
            throw new InvalidDataException($"Room-object setup block {block} is mirrored, but keeps the struck block's mirroring.");
        return picture;
    }

    /// <summary>Builds the pictures from an installed document, checking it names every setup block once.</summary>
    private static InstalledRoomObjectSetupBlocks Create(BlockDocument document)
    {
        if (document.Version != FormatVersion || document.Blocks is null)
            throw new InvalidDataException("the document's format is not this importer's.");
        var byId = Enum.GetValues<RoomObjectSetupBlock>().ToDictionary(Id, block => block, StringComparer.Ordinal);
        var blocks = new Dictionary<RoomObjectSetupBlock, RoomBlockAppearance>();
        foreach (BlockEntry entry in document.Blocks)
        {
            if (entry?.Id is null || !byId.TryGetValue(entry.Id, out RoomObjectSetupBlock block))
                throw new InvalidDataException($"block {entry?.Id ?? "(unnamed)"} is not one of the room-object setup blocks.");
            RoomBlockAppearance shown;
            try
            {
                shown = RoomBlockAppearance.Of(entry.Block, entry.Mirroring);
            }
            catch (ArgumentOutOfRangeException error)
            {
                throw new InvalidDataException($"block {entry.Id} shows an invalid block.", error);
            }
            if (KeepsStruckMirroring(block) && shown.Mirroring != BlockMirroring.None)
                throw new InvalidDataException($"block {entry.Id} is mirrored, but shows with the struck block's own mirroring.");
            if (!blocks.TryAdd(block, shown))
                throw new InvalidDataException($"block {entry.Id} is pictured twice.");
        }
        foreach (RoomObjectSetupBlock block in Enum.GetValues<RoomObjectSetupBlock>())
            if (!blocks.ContainsKey(block))
                throw new InvalidDataException($"block {Id(block)} is not pictured.");
        return new InstalledRoomObjectSetupBlocks(blocks);
    }

    /// <summary>Reads and parses the installed document, naming its path in a rejection.</summary>
    private static BlockDocument Read(string path)
    {
        try
        {
            return JsonSerializer.Deserialize<BlockDocument>(File.ReadAllBytes(path), JsonOptions)
                ?? throw new InvalidDataException($"Room-object setup-block pictures {path} are empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid room-object setup-block pictures {path}.", error);
        }
    }

    /// <summary>The installed document: its format and every setup block's picture.</summary>
    private sealed record BlockDocument(int Version, BlockEntry[] Blocks);

    /// <summary>One setup block's picture: its published name, the block it shows and how that block is mirrored.</summary>
    private sealed record BlockEntry(string Id, int Block, BlockMirroring Mirroring);
}
