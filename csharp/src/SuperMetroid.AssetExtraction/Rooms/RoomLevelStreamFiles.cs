using SuperMetroid.Core;
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Installs the typed terrain of every retail room level (#1469): each level's grid of terrain blocks, behaviours and
/// background appearances, and the terrain past its grid, decoded once from the cartridge by
/// <see cref="RoomLevelStreamDecoder"/>. The family is stock-only. It has no override directory, so artwork edits cannot
/// alter terrain, behaviours, slopes, hazards or the terrain beyond a room.
/// </summary>
/// <remarks>
/// Each level is one little-endian file: the grid's width and height in blocks, how many decoded blocks follow the grid,
/// and how many blocks the level space holds past the grid (all four as 32-bit integers); then each foreground block as
/// its collision kind, block number, mirroring and behaviour; each background block as its block number and mirroring;
/// and each following block as its collision kind, block number and mirroring. A collision kind is written as its
/// position in the manifest's <c>collisionKinds</c>, which names the kinds; mirroring is written as
/// <see cref="MirroringCodes"/>'s position of its value.
/// </remarks>
public static class RoomLevelStreamFiles
{
    /// <summary>Stock manifest filename recording provenance, the collision kinds' names and each level's file, byte count and SHA-256.</summary>
    public const string ManifestFileName = "room-level-streams.json";
    /// <summary>Manifest schema version this loader accepts; version 3 installs typed blocks instead of decompressed level data.</summary>
    private const int FormatVersion = 3;

    /// <summary>The collision kinds in the order the level files write them by position.</summary>
    private static readonly ImmutableArray<RoomCollisionType> CollisionKinds = [.. Enum.GetValues<RoomCollisionType>()];

    /// <summary>Each mirroring in the order the level files write it by position.</summary>
    private static readonly ImmutableArray<BlockMirroring> MirroringCodes =
        [BlockMirroring.None, BlockMirroring.Horizontal, BlockMirroring.Vertical, BlockMirroring.Horizontal | BlockMirroring.Vertical];

    /// <summary>Serializer settings shared by manifest writes and reads.</summary>
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
    /// <param name="directory">Family directory, created if absent; receives new per-level files and the required manifest.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied provenance hash, recorded without recomputation and checked against the supported identity on load.</param>
    /// <remarks>Create-new writes refuse existing files and do not roll back earlier outputs.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory/source hash is blank or the address space lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">A compressed level is invalid or malformed.</exception>
    /// <exception cref="IOException">An output exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        Directory.CreateDirectory(directory);
        var entries = new Dictionary<string, LevelFileEntry>();
        foreach ((RoomLevelId level, int width) in RoomVisualLayoutFiles.RetailLevels.OrderBy(item => item.Key.Name, StringComparer.Ordinal))
        {
            byte[] stream = RomDataReader.Decompress(cartridge, RoomLevelSourceDefinitions.Addresses[level]);
            byte[] file = Encode(RoomLevelStreamDecoder.Decode(level, stream, width));
            string name = LevelFileName(level);
            using (var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(file);
            entries.Add(name, new LevelFileEntry(level.Name, file.Length, Convert.ToHexString(SHA256.HashData(file))));
        }
        using var manifest = new FileStream(Path.Combine(directory, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new LevelFileManifest(FormatVersion, sourceCartridgeSha256, [.. CollisionKinds.Select(kind => kind.ToString())], entries),
            JsonOptions);
    }

    /// <summary>Checks the complete retail-level manifest and every file's length and hash, then builds the stock catalog.</summary>
    /// <param name="directory">Family directory containing all per-level files and the required provenance manifest.</param>
    /// <returns>A ROM-independent catalog of every level's typed terrain.</returns>
    /// <exception cref="ArgumentException"><paramref name="directory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">The version, provenance, collision kinds, coverage, a file's identity, length, hash or contents are invalid; the message names the file.</exception>
    /// <exception cref="IOException">The manifest or a level file cannot be read.</exception>
    public static InstalledRoomLevelStreams Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, ManifestFileName);
        if (!File.Exists(manifestPath))
            throw new InvalidDataException($"Room-level manifest {manifestPath} is missing.");
        LevelFileManifest manifest;
        using (FileStream manifestStream = File.OpenRead(manifestPath))
            manifest = JsonAssetDocument.Read<LevelFileManifest>(
                manifestStream, JsonOptions, $"room-level manifest {manifestPath}");
        if (manifest.Version != FormatVersion)
            throw new InvalidDataException(
                $"Room-level manifest {manifestPath} has unsupported version {manifest.Version}.");
        if (!string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Room-level manifest {manifestPath} was not extracted from the supported cartridge.");
        if (manifest.Entries is null)
            throw new InvalidDataException($"Room-level manifest {manifestPath} has no entries.");
        RoomCollisionType[] kinds = ReadKinds(manifest.CollisionKinds, manifestPath);

        var levels = new Dictionary<RoomLevelId, InstalledRoomLevel>(manifest.Entries.Count);
        foreach ((string name, LevelFileEntry entry) in manifest.Entries)
        {
            string path = Path.Combine(directory, name);
            if (entry?.Level is null || name != LevelFileName(entry.Id) ||
                !RoomVisualLayoutFiles.RetailLevels.TryGetValue(entry.Id, out int width))
                throw new InvalidDataException(
                    $"Room-level manifest {manifestPath} names unknown level file {path}.");
            if (!File.Exists(path))
                throw new InvalidDataException($"Room-level file {path} is missing.");
            byte[] file = File.ReadAllBytes(path);
            if (file.Length != entry.ByteCount)
                throw new InvalidDataException(
                    $"Room-level file {path} has {file.Length} bytes; the manifest records {entry.ByteCount}.");
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(file)), entry.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Room-level file {path} failed its manifest hash.");
            InstalledRoomLevel installed = Decode(file, kinds, path);
            if (installed.Size.X != width)
                throw new InvalidDataException($"Room-level file {path} is {installed.Size.X} blocks wide; its rooms are {width}.");
            if (!levels.TryAdd(entry.Id, installed))
                throw new InvalidDataException($"Room-level manifest {manifestPath} repeats {path}.");
        }
        foreach (RoomLevelId level in RoomVisualLayoutFiles.RetailLevels.Keys.OrderBy(candidate => candidate.Name, StringComparer.Ordinal))
            if (!levels.ContainsKey(level))
                throw new InvalidDataException(
                    $"Room-level manifest {manifestPath} lacks {Path.Combine(directory, LevelFileName(level))}.");
        return new InstalledRoomLevelStreams(levels, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(manifestPath))),
            RoomVisualLayoutSourceDefinitions.All);
    }

    /// <summary>Loads and validates every stock level through <see cref="Load"/> with no cartridge reads or file writes.</summary>
    /// <param name="directory">Room-level family directory containing the manifest and all referenced files.</param>
    /// <exception cref="InvalidDataException">Provenance, hashes, lengths, schema, contents or coverage are invalid.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory);

    /// <summary>Writes one level as the installed file format.</summary>
    private static byte[] Encode(InstalledRoomLevel level)
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
                writer.Write(checked((byte)level.Behaviors[index].Value));
            }
            foreach (RoomBlockAppearance appearance in level.Background)
                WriteAppearance(writer, appearance);
            foreach (RoomTerrainBlock block in level.Following)
                WriteTerrain(writer, block);
        }
        return memory.ToArray();
    }

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
    /// <exception cref="InvalidDataException">The file is truncated, has trailing bytes, or holds an unknown kind, mirroring or block.</exception>
    private static InstalledRoomLevel Decode(byte[] file, RoomCollisionType[] kinds, string path)
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
                foreground.Add(ReadTerrain(reader, kinds));
                behaviors.Add(new RoomBlockBehavior(reader.ReadByte()));
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
            throw new InvalidDataException($"Collision kind {kind} is not among the manifest's {kinds.Length} kinds.");
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

    /// <summary>Reads the manifest's collision kinds by name, requiring every kind exactly once.</summary>
    private static RoomCollisionType[] ReadKinds(string[]? names, string manifestPath)
    {
        if (names is null || names.Length != CollisionKinds.Length)
            throw new InvalidDataException($"Room-level manifest {manifestPath} does not name the {CollisionKinds.Length} collision kinds.");
        var kinds = new RoomCollisionType[names.Length];
        for (int index = 0; index < names.Length; index++)
        {
            if (!Enum.TryParse(names[index], ignoreCase: false, out kinds[index]) || !Enum.IsDefined(kinds[index]))
                throw new InvalidDataException($"Room-level manifest {manifestPath} names unknown collision kind '{names[index]}'.");
        }
        if (kinds.Distinct().Count() != kinds.Length)
            throw new InvalidDataException($"Room-level manifest {manifestPath} repeats a collision kind.");
        return kinds;
    }

    /// <summary>One retail room level: its name, file byte count and SHA-256 digest.</summary>
    private sealed record LevelFileEntry(string Level, int ByteCount, string Sha256)
    {
        /// <summary>The level the entry names.</summary>
        [JsonIgnore]
        public RoomLevelId Id => new(Level);
    }
    /// <summary>Records schema and source-ROM identity, the collision kinds by file position, and every level file entry.</summary>
    private sealed record LevelFileManifest(int Version, string SourceCartridgeSha256, string[] CollisionKinds,
        Dictionary<string, LevelFileEntry> Entries);
}
