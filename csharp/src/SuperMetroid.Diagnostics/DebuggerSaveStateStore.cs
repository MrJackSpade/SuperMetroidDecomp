using SuperMetroid.Core;
using System.IO.Compression;
using System.Security.Cryptography;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Desktop;

/// <summary>Host-independent debugger states with ten manual slots and one automatic slot with warning-only build identity checks.</summary>
internal sealed class DebuggerSaveStateStore
{

    /// <summary>Stores directory.</summary>
    private readonly string directory;
    /// <summary>Stores rom digest.</summary>
    private readonly byte[] romDigest;
    /// <summary>Stores host options.</summary>
    private readonly SuperMetroidGameOptions? hostOptions;
    /// <summary>Stores content identity.</summary>
    private readonly GameContentIdentity contentIdentity;

    /// <summary>Initializes a new debugger save state store instance.</summary>
    internal DebuggerSaveStateStore(
        string directory,
        byte[] romDigest,
        SuperMetroidGameOptions? hostOptions,
        GameContentIdentity contentIdentity)
    {
        Ensure.NotNull(contentIdentity);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Ensure.LengthEqual(romDigest, SHA256.HashSizeInBytes);
        // Null preserves the captured policy for exact replay. Interactive hosts
        // explicitly supply their active INI options instead of trusting old state.
        this.hostOptions = hostOptions;
        this.contentIdentity = contentIdentity;
        this.directory = Path.GetFullPath(directory);
        this.romDigest = romDigest.ToArray();
    }

    /// <summary>
    /// Creates a store for an installed game whose source revision was already verified by
    /// the asset installer. No private cartridge file is opened to establish identity.
    /// </summary>
    public static DebuggerSaveStateStore ForInstalledGame(
        string dataDirectory,
        SuperMetroidGameOptions? hostOptions,
        GameContentIdentity contentIdentity,
        string? directoryOverride = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);
        Ensure.NotNull(contentIdentity);
        if (!contentIdentity.SourceCartridgeSha256.Equals(
                SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "Installed content identity does not name the supported source cartridge revision.");
        }
        return new DebuggerSaveStateStore(
            directoryOverride is null
                ? Path.Combine(Path.GetFullPath(dataDirectory), "debug-states")
                : Path.GetFullPath(directoryOverride),
            SupportedCartridge.CreateSha256Digest(),
            hostOptions,
            contentIdentity);
    }

    /// <summary>Slot name.</summary>
    public static string SlotName(int slot)
    {
        ValidateSlot(slot);
        return slot == DebuggerStateFormat.AutomaticSlot ? "auto" : slot.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>Gets slot path.</summary>
    public string GetSlotPath(int slot)
    {
        ValidateSlot(slot);
        return Path.Combine(directory, $"SuperMetroid-debug-slot-{SlotName(slot)}.smstate");
    }

    /// <summary>Saves.</summary>
    public DebuggerSaveStateMetadata Save(
        int slot,
        SuperMetroidAddressSpace addressSpace,
        SuperMetroidGame game,
        ManagedSpcPlayer? audioPlayer)
    {
        ValidateSlot(slot);
        Ensure.NotNull(addressSpace);
        Ensure.NotNull(game);
        EnsureRomMatches(addressSpace);

        Directory.CreateDirectory(directory);
        string destination = GetSlotPath(slot);
        string temporary = Path.Combine(directory, $".{Path.GetFileName(destination)}.{Guid.NewGuid():N}.tmp");
        var metadata = new DebuggerSaveStateMetadata(
            DateTimeOffset.UtcNow,
            game.FrameNumber,
            game.GameState,
            game.GameplayActiveRoomPointer,
            game.GameplayActiveRoomStatePointer,
            destination);

        try
        {
            using (var stream = new FileStream(
                       temporary,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       bufferSize: 128 * 1024,
                       FileOptions.WriteThrough))
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(DebuggerStateFormat.Magic);
                writer.Write((int)DebuggerStateVersion.Current);
                writer.Write(typeof(SuperMetroidGame).Module.ModuleVersionId.ToByteArray());
                writer.Write(typeof(DebuggerSaveStateStore).Module.ModuleVersionId.ToByteArray());
                writer.Write(romDigest);
                WriteContentIdentity(writer, contentIdentity.ToSnapshot());
                writer.Write(metadata.SavedUtc.UtcTicks);
                writer.Write(metadata.FrameNumber);
                writer.Write((ushort)metadata.GameState);
                WriteNullableWord(writer, metadata.RoomPointer);
                WriteNullableWord(writer, metadata.RoomStatePointer);
                writer.Flush();

                using (var compressed = new GZipStream(stream, CompressionLevel.SmallestSize, leaveOpen: true))
                    DebuggerObjectGraphSerializer.Serialize(
                        compressed, new DebuggerSaveStateRoot(addressSpace, game, audioPlayer));
                stream.Flush(flushToDisk: true);
            }
            if (File.Exists(destination))
                File.Replace(temporary, destination, destinationBackupFileName: null);
            else
                File.Move(temporary, destination);
            return metadata;
        }
        catch
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
            throw;
        }
    }

    /// <summary>Loads.</summary>
    public DebuggerSaveStateLoadResult Load(int slot)
    {
        ValidateSlot(slot);
        string path = GetSlotPath(slot);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Debugger save-state slot {SlotName(slot)} does not exist.", path);

        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 128 * 1024,
            FileOptions.SequentialScan);
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        byte[] magic = reader.ReadBytes(DebuggerStateFormat.Magic.Length);
        if (!magic.AsSpan().SequenceEqual(DebuggerStateFormat.Magic))
            throw new InvalidDataException($"'{path}' is not a Super Metroid debugger state.");
        int storedVersion = reader.ReadInt32();
        if (storedVersion != (int)DebuggerStateVersion.Current)
        {
            throw new InvalidDataException(
                $"{DebuggerObjectGraphSerializer.UnsupportedVersion}: '{path}' has schema {storedVersion}; " +
                $"this build reads only schema {(int)DebuggerStateVersion.Current}.");
        }
        var warnings = new List<string>();
        ReadBuildIdentity(reader, typeof(SuperMetroidGame).Module.ModuleVersionId, "core build", warnings);
        ReadBuildIdentity(reader, typeof(DebuggerSaveStateStore).Module.ModuleVersionId, "desktop build", warnings);
        byte[] storedDigest = reader.ReadBytes(SHA256.HashSizeInBytes);
        if (storedDigest.Length != SHA256.HashSizeInBytes ||
            !CryptographicOperations.FixedTimeEquals(storedDigest, romDigest))
        {
            throw new InvalidDataException(
                $"Debugger state slot {slot} was captured from a different ROM (SHA-256 mismatch).");
        }

        GameContentIdentitySnapshot storedContentIdentity = ReadContentIdentity(reader);
        warnings.AddRange(contentIdentity.GetCompatibilityWarnings(storedContentIdentity, "debugger state"));
        foreach (string warning in warnings)
            Console.Error.WriteLine($"WARNING: {warning}");

        DateTimeOffset savedUtc = new(reader.ReadInt64(), TimeSpan.Zero);
        ushort frame = reader.ReadUInt16();
        SuperMetroidGameState gameState = (SuperMetroidGameState)reader.ReadUInt16();
        ushort? room = ReadNullableWord(reader);
        ushort? roomState = ReadNullableWord(reader);
        using var compressed = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
        DebuggerSaveStateRoot root;
        try
        {
            root = DebuggerObjectGraphSerializer.Deserialize<DebuggerSaveStateRoot>(compressed);
        }
        catch (InvalidDataException exception) when (
            exception.Message.Contains(DebuggerObjectGraphSerializer.UnsupportedVersion, StringComparison.Ordinal))
        {
            // Older builds' layouts are not converted (#1270): name the file before the nested field path.
            throw new InvalidDataException(
                $"{DebuggerObjectGraphSerializer.UnsupportedVersion}: '{path}' was saved by a build with a different state layout. " +
                exception.Message, exception);
        }
        EnsureRomMatches(root.AddressSpace);
        if (root.Game.FrameNumber != frame || root.Game.GameState != gameState ||
            root.Game.GameplayActiveRoomPointer != room ||
            root.Game.GameplayActiveRoomStatePointer != roomState)
        {
            throw new InvalidDataException(
                "Debugger state payload does not agree with its frame/room metadata header.");
        }

        if (hostOptions is not null)
            root.Game.ApplyHostOptions(hostOptions);

        return new DebuggerSaveStateLoadResult(
            root.AddressSpace,
            root.Game,
            root.AudioPlayer,
            new DebuggerSaveStateMetadata(savedUtc, frame, gameState, room, roomState, path), warnings.AsReadOnly());
    }

    /// <summary>
    /// Loads an occupied debugger slot without treating an empty slot as corrupted state.
    /// Every malformed, incompatible, or wrong-ROM file still throws: only the ordinary
    /// absence represented by the ten-slot UI returns <see langword="false"/>.
    /// </summary>
    public bool TryLoad(int slot, out DebuggerSaveStateLoadResult result)
    {
        ValidateSlot(slot);
        if (!File.Exists(GetSlotPath(slot)))
        {
            result = default;
            return false;
        }

        result = Load(slot);
        return true;
    }

    /// <summary>Ensure rom matches.</summary>
    private void EnsureRomMatches(SuperMetroidAddressSpace addressSpace)
    {
        Ensure.NotNull(addressSpace);
        // The live address space contains only mutable memory. Source revision is
        // validated when assets are installed and recorded in the content receipt.
        if (!CryptographicOperations.FixedTimeEquals(SupportedCartridge.CreateSha256Digest(), romDigest))
            throw new InvalidDataException(
                "Debugger states require an installed, revision-verified content identity.");
    }

    /// <summary>Reads build identity.</summary>
    private static void ReadBuildIdentity(BinaryReader reader, Guid expected, string label, List<string> warnings)
    {
        byte[] bytes = reader.ReadBytes(DebuggerStateFormat.GuidBytes);
        if (bytes.Length != DebuggerStateFormat.GuidBytes)
            throw new InvalidDataException($"Debugger state {label} identity is truncated.");
        if (new Guid(bytes) != expected)
            warnings.Add($"Debugger state {label} differs from this executable; attempting compatible restoration. Behavior may differ from the captured build.");
    }

    /// <summary>Writes content identity.</summary>
    private static void WriteContentIdentity(
        BinaryWriter writer,
        GameContentIdentitySnapshot identity)
    {
        writer.Write(identity.FormatVersion);
        writer.Write(identity.CompiledDefinitionsBuildId.ToByteArray());
        WriteDigest(writer, identity.AudioContentSha256, "audio");
        WriteDigest(writer, identity.MapContentSha256, "map");
        WriteDigest(writer, identity.ProjectileContentSha256, "projectile");
        WriteDigest(writer, identity.CompositeSha256, "composite");
        writer.Flush();
        GameContentComponentFormat.Write(writer.BaseStream, identity.AdditionalContentSha256);
    }

    /// <summary>Reads content identity.</summary>
    private static GameContentIdentitySnapshot ReadContentIdentity(BinaryReader reader)
    {
        int formatVersion = reader.ReadInt32();
        if (formatVersion <= 0)
            throw new InvalidDataException("Debugger state has an invalid content-identity version.");
        byte[] buildId = ReadExact(reader, DebuggerStateFormat.GuidBytes, "content build identity");
        return new GameContentIdentitySnapshot
        {
            FormatVersion = formatVersion,
            CompiledDefinitionsBuildId = new Guid(buildId),
            AudioContentSha256 = ReadExact(reader, DebuggerStateFormat.DigestBytes, "audio content digest"),
            MapContentSha256 = ReadExact(reader, DebuggerStateFormat.DigestBytes, "map content digest"),
            ProjectileContentSha256 = ReadExact(reader, DebuggerStateFormat.DigestBytes, "projectile content digest"),
            CompositeSha256 = ReadExact(reader, DebuggerStateFormat.DigestBytes, "composite content digest"),
            AdditionalContentSha256 = GameContentComponentFormat.Read(reader.BaseStream),
        };
    }

    /// <summary>Writes digest.</summary>
    private static void WriteDigest(BinaryWriter writer, byte[] digest, string component)
    {
        if (digest.Length != DebuggerStateFormat.DigestBytes)
        {
            throw new InvalidDataException(
                $"Debugger state requires a {DebuggerStateFormat.DigestBytes}-byte {component} content digest.");
        }
        writer.Write(digest);
    }

    /// <summary>Reads exact.</summary>
    private static byte[] ReadExact(BinaryReader reader, int byteCount, string field)
    {
        byte[] bytes = reader.ReadBytes(byteCount);
        if (bytes.Length != byteCount)
            throw new InvalidDataException($"Debugger state {field} is truncated.");
        return bytes;
    }

    /// <summary>Writes nullable word.</summary>
    private static void WriteNullableWord(BinaryWriter writer, ushort? value)
    {
        writer.Write(value.HasValue);
        if (value.HasValue)
            writer.Write(value.Value);
    }

    /// <summary>Reads nullable word.</summary>
    private static ushort? ReadNullableWord(BinaryReader reader) =>
        reader.ReadBoolean() ? reader.ReadUInt16() : null;

    /// <summary>Validates slot.</summary>
    private static void ValidateSlot(int slot)
    {
        if ((uint)slot > DebuggerStateFormat.AutomaticSlot)
            throw new ArgumentOutOfRangeException(nameof(slot), slot, "Debugger state slot must be 0-9 or auto.");
    }

    /// <summary>Represents debugger save state root.</summary>
    private sealed class DebuggerSaveStateRoot(
        SuperMetroidAddressSpace addressSpace,
        SuperMetroidGame game,
        ManagedSpcPlayer? audioPlayer)
    {
        /// <summary>Stores address space.</summary>
        public readonly SuperMetroidAddressSpace AddressSpace = addressSpace;
        /// <summary>Stores game.</summary>
        public readonly SuperMetroidGame Game = game;
        /// <summary>Stores audio player.</summary>
        public readonly ManagedSpcPlayer? AudioPlayer = audioPlayer;
    }
}

/// <summary>Represents debugger save state metadata.</summary>
internal readonly record struct DebuggerSaveStateMetadata(
    DateTimeOffset SavedUtc,
    ushort FrameNumber,
    SuperMetroidGameState GameState,
    ushort? RoomPointer,
    ushort? RoomStatePointer,
    string Path);

/// <summary>Represents debugger save state load result.</summary>
internal readonly record struct DebuggerSaveStateLoadResult(
    SuperMetroidAddressSpace AddressSpace,
    SuperMetroidGame Game,
    ManagedSpcPlayer? AudioPlayer,
    DebuggerSaveStateMetadata Metadata,
    IReadOnlyList<string> Warnings);
