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
    /// <summary>Absolute directory containing numbered and automatic state files.</summary>
    private readonly string directory;
    /// <summary>Verified source-cartridge digest required when loading or saving a graph.</summary>
    private readonly byte[] romDigest;
    /// <summary>Interactive host options to apply after restoration; null preserves captured options.</summary>
    private readonly SuperMetroidGameOptions? hostOptions;
    /// <summary>Optional installed-content identity checked against state headers.</summary>
    private readonly GameContentIdentity? contentIdentity;

    /// <summary>Creates a store bound to a verified cartridge digest and optional host/content policies.</summary>
    /// <param name="directory">State-file directory.</param>
    /// <param name="romDigest">32-byte source-cartridge digest.</param>
    /// <param name="hostOptions">Options imposed by an interactive host, or null to preserve saved options.</param>
    /// <param name="contentIdentity">Installed-content identity used for compatibility warnings.</param>
    internal DebuggerSaveStateStore(
        string directory,
        byte[] romDigest,
        SuperMetroidGameOptions? hostOptions,
        GameContentIdentity? contentIdentity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        if (romDigest.Length != SHA256.HashSizeInBytes)
            throw new ArgumentException("Debugger state source digest must contain 32 bytes.", nameof(romDigest));
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
    /// <param name="dataDirectory">Installed game data directory.</param>
    /// <param name="hostOptions">Active host options, or null to preserve capture options.</param>
    /// <param name="contentIdentity">Verified installed-content receipt.</param>
    /// <param name="directoryOverride">Optional state directory overriding the standard subdirectory.</param>
    /// <returns>A store using the verified supported-cartridge digest.</returns>
    public static DebuggerSaveStateStore ForInstalledGame(
        string dataDirectory,
        SuperMetroidGameOptions? hostOptions,
        GameContentIdentity contentIdentity,
        string? directoryOverride = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);
        ArgumentNullException.ThrowIfNull(contentIdentity);
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

    /// <summary>Gets the filename label for a validated manual or automatic slot.</summary>
    /// <param name="slot">Slot number from zero through the automatic slot.</param>
    /// <returns>Invariant-culture number or the automatic-slot label.</returns>
    public static string SlotName(int slot)
    {
        ValidateSlot(slot);
        return slot == DebuggerStateFormat.AutomaticSlot ? "auto" : slot.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>Gets the state-file path for a validated slot.</summary>
    /// <param name="slot">Slot number from zero through the automatic slot.</param>
    /// <returns>Absolute path of that slot's debugger state.</returns>
    public string GetSlotPath(int slot)
    {
        ValidateSlot(slot);
        return Path.Combine(directory, $"SuperMetroid-debug-slot-{SlotName(slot)}.smstate");
    }

    /// <summary>Serializes a game graph and atomically publishes its state file.</summary>
    /// <param name="slot">Destination slot.</param>
    /// <param name="addressSpace">Current mutable runtime memory.</param>
    /// <param name="game">Game state graph to capture.</param>
    /// <param name="audioPlayer">Managed audio state included when available.</param>
    /// <returns>Metadata written into the state header.</returns>
    public DebuggerSaveStateMetadata Save(
        int slot,
        SuperMetroidAddressSpace addressSpace,
        SuperMetroidGame game,
        ManagedSpcPlayer? audioPlayer)
    {
        ValidateSlot(slot);
        ArgumentNullException.ThrowIfNull(addressSpace);
        ArgumentNullException.ThrowIfNull(game);
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
                writer.Write(contentIdentity is null
                    ? DebuggerStateFormat.NamedDelegateVersion
                    : DebuggerStateFormat.CurrentVersion);
                writer.Write(typeof(SuperMetroidGame).Module.ModuleVersionId.ToByteArray());
                writer.Write(typeof(DebuggerSaveStateStore).Module.ModuleVersionId.ToByteArray());
                writer.Write(romDigest);
                if (contentIdentity is not null)
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

    /// <summary>Loads and validates an occupied slot, including its header and restored graph.</summary>
    /// <param name="slot">State slot to load.</param>
    /// <returns>Restored memory, game, audio, header metadata, and compatibility warnings.</returns>
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
        int version = reader.ReadInt32();
        if (version is not (
                DebuggerStateFormat.CurrentVersion or
                DebuggerStateFormat.IdentifiedVersion or
                DebuggerStateFormat.NamedDelegateVersion or
                DebuggerStateFormat.LegacyTokenVersion))
        {
            throw new InvalidDataException(
                $"Debugger state schema {version} is incompatible with schema {DebuggerStateFormat.CurrentVersion}.");
        }
        var warnings = new List<string>();
        ReadBuildIdentity(reader, typeof(SuperMetroidGame).Module.ModuleVersionId, "core build", warnings);
        ReadBuildIdentity(reader, typeof(DebuggerSaveStateStore).Module.ModuleVersionId, "desktop build", warnings);
        if (warnings.Count != 0 && version == DebuggerStateFormat.LegacyTokenVersion)
            warnings.Add("Legacy debugger state uses compiler method tokens; cross-build delegate compatibility cannot be guaranteed.");
        byte[] storedDigest = reader.ReadBytes(SHA256.HashSizeInBytes);
        if (storedDigest.Length != SHA256.HashSizeInBytes ||
            !CryptographicOperations.FixedTimeEquals(storedDigest, romDigest))
        {
            throw new InvalidDataException(
                $"Debugger state slot {slot} was captured from a different ROM (SHA-256 mismatch).");
        }

        GameContentIdentitySnapshot? storedContentIdentity = version >= DebuggerStateFormat.IdentifiedVersion
            ? ReadContentIdentity(reader, version == DebuggerStateFormat.CurrentVersion)
            : null;
        if (contentIdentity is not null)
        {
            warnings.AddRange(contentIdentity.GetCompatibilityWarnings(
                storedContentIdentity,
                "debugger state"));
        }
        else if (storedContentIdentity is not null)
        {
            warnings.Add(
                "This host has no installed-content identity; only the debugger state's " +
                "source ROM and assembly builds could be verified.");
        }
        foreach (string warning in warnings)
            Console.Error.WriteLine($"WARNING: {warning}");

        DateTimeOffset savedUtc = new(reader.ReadInt64(), TimeSpan.Zero);
        ushort frame = reader.ReadUInt16();
        SuperMetroidGameState gameState = (SuperMetroidGameState)reader.ReadUInt16();
        ushort? room = ReadNullableWord(reader);
        ushort? roomState = ReadNullableWord(reader);
        using var compressed = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
        DebuggerSaveStateRoot root = DebuggerObjectGraphSerializer.Deserialize<DebuggerSaveStateRoot>(
            compressed, legacyDelegateTokens: version == DebuggerStateFormat.LegacyTokenVersion);
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
    /// <param name="slot">State slot to inspect.</param>
    /// <param name="result">Receives restored state when a file exists.</param>
    /// <returns><see langword="true"/> when the slot is occupied and loads successfully.</returns>
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

    /// <summary>Requires verified installed content before capturing mutable runtime memory.</summary>
    /// <param name="addressSpace">Current runtime address space.</param>
    private void EnsureRomMatches(SuperMetroidAddressSpace addressSpace)
    {
        ArgumentNullException.ThrowIfNull(addressSpace);
        // The live address space contains only mutable memory. Source revision is
        // validated when assets are installed and recorded in the content receipt.
        if (contentIdentity is null ||
            !CryptographicOperations.FixedTimeEquals(
                SupportedCartridge.CreateSha256Digest(), romDigest))
            throw new InvalidDataException(
                "Debugger states require an installed, revision-verified content identity.");
    }

    /// <summary>Reads an executable build identifier and records a compatibility warning on mismatch.</summary>
    /// <param name="reader">State stream positioned at the identifier bytes.</param>
    /// <param name="expected">Current module build identifier.</param>
    /// <param name="label">Identity label used in diagnostics.</param>
    /// <param name="warnings">Warnings accumulated for the load result.</param>
    private static void ReadBuildIdentity(BinaryReader reader, Guid expected, string label, List<string> warnings)
    {
        byte[] bytes = reader.ReadBytes(DebuggerStateFormat.GuidBytes);
        if (bytes.Length != DebuggerStateFormat.GuidBytes)
            throw new InvalidDataException($"Debugger state {label} identity is truncated.");
        if (new Guid(bytes) != expected)
            warnings.Add($"Debugger state {label} differs from this executable; attempting compatible restoration. Behavior may differ from the captured build.");
    }

    /// <summary>Writes versioned installed-content fingerprints to a state header.</summary>
    /// <param name="writer">Header writer.</param>
    /// <param name="identity">Snapshot of the installed content to record.</param>
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

    /// <summary>Reads and validates the versioned installed-content identity from a header.</summary>
    /// <param name="reader">Header reader.</param>
    /// <param name="hasComponents">Whether the header includes named component digests.</param>
    /// <returns>The reconstructed identity snapshot.</returns>
    private static GameContentIdentitySnapshot ReadContentIdentity(BinaryReader reader, bool hasComponents)
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
            AdditionalContentSha256 = hasComponents
                ? GameContentComponentFormat.Read(reader.BaseStream)
                : new Dictionary<string, byte[]>(StringComparer.Ordinal),
        };
    }

    /// <summary>Writes a fixed-length content digest after validating its size.</summary>
    /// <param name="writer">Header writer.</param>
    /// <param name="digest">Digest bytes.</param>
    /// <param name="component">Component label used if the size is invalid.</param>
    private static void WriteDigest(BinaryWriter writer, byte[] digest, string component)
    {
        if (digest.Length != DebuggerStateFormat.DigestBytes)
        {
            throw new InvalidDataException(
                $"Debugger state requires a {DebuggerStateFormat.DigestBytes}-byte {component} content digest.");
        }
        writer.Write(digest);
    }

    /// <summary>Reads exactly the requested number of bytes or rejects a truncated header.</summary>
    /// <param name="reader">Header reader.</param>
    /// <param name="byteCount">Required byte count.</param>
    /// <param name="field">Field label used in a truncation error.</param>
    /// <returns>The requested bytes.</returns>
    private static byte[] ReadExact(BinaryReader reader, int byteCount, string field)
    {
        byte[] bytes = reader.ReadBytes(byteCount);
        if (bytes.Length != byteCount)
            throw new InvalidDataException($"Debugger state {field} is truncated.");
        return bytes;
    }

    /// <summary>Writes a presence marker and optional 16-bit room identity.</summary>
    /// <param name="writer">Header writer.</param>
    /// <param name="value">Optional word to write.</param>
    private static void WriteNullableWord(BinaryWriter writer, ushort? value)
    {
        writer.Write(value.HasValue);
        if (value.HasValue)
            writer.Write(value.Value);
    }

    /// <summary>Reads a presence marker followed by an optional 16-bit word.</summary>
    /// <param name="reader">Header reader.</param>
    /// <returns>The word when present, otherwise <see langword="null"/>.</returns>
    private static ushort? ReadNullableWord(BinaryReader reader) =>
        reader.ReadBoolean() ? reader.ReadUInt16() : null;

    /// <summary>Rejects slot numbers outside the ten manual and one automatic positions.</summary>
    /// <param name="slot">Slot number to validate.</param>
    private static void ValidateSlot(int slot)
    {
        if ((uint)slot > DebuggerStateFormat.AutomaticSlot)
            throw new ArgumentOutOfRangeException(nameof(slot), slot, "Debugger state slot must be 0-9 or auto.");
    }

    /// <summary>Serialized graph root bundling memory, game state, and the optional managed audio player.</summary>
    /// <param name="addressSpace">Mutable runtime memory.</param>
    /// <param name="game">Game object graph.</param>
    /// <param name="audioPlayer">Managed audio player state.</param>
    private sealed class DebuggerSaveStateRoot(
        SuperMetroidAddressSpace addressSpace,
        SuperMetroidGame game,
        ManagedSpcPlayer? audioPlayer)
    {
        /// <summary>Mutable runtime address space included in the snapshot.</summary>
        public readonly SuperMetroidAddressSpace AddressSpace = addressSpace;
        /// <summary>Game graph included in the snapshot.</summary>
        public readonly SuperMetroidGame Game = game;
        /// <summary>Managed audio state included when available.</summary>
        public readonly ManagedSpcPlayer? AudioPlayer = audioPlayer;
    }
}

/// <summary>Header metadata stored with a debugger graph.</summary>
/// <param name="SavedUtc">Time at which the snapshot was created.</param>
/// <param name="FrameNumber">Emulated frame captured by the snapshot.</param>
/// <param name="GameState">Game phase captured by the snapshot.</param>
/// <param name="RoomPointer">Active gameplay room, when available.</param>
/// <param name="RoomStatePointer">Active room-state definition, when available.</param>
/// <param name="Path">File path used to store the snapshot.</param>
internal readonly record struct DebuggerSaveStateMetadata(
    DateTimeOffset SavedUtc,
    ushort FrameNumber,
    SuperMetroidGameState GameState,
    ushort? RoomPointer,
    ushort? RoomStatePointer,
    string Path);

/// <summary>Restored graph and header data returned by a successful state load.</summary>
/// <param name="AddressSpace">Restored runtime memory.</param>
/// <param name="Game">Restored game instance.</param>
/// <param name="AudioPlayer">Restored managed audio state, if present.</param>
/// <param name="Metadata">Header metadata associated with the graph.</param>
/// <param name="Warnings">Compatibility warnings discovered during restoration.</param>
internal readonly record struct DebuggerSaveStateLoadResult(
    SuperMetroidAddressSpace AddressSpace,
    SuperMetroidGame Game,
    ManagedSpcPlayer? AudioPlayer,
    DebuggerSaveStateMetadata Metadata,
    IReadOnlyList<string> Warnings);
