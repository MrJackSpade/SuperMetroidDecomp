using System.IO.Compression;
using System.Security.Cryptography;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.Desktop;

/// <summary>Host-independent ten-slot debugger states with warning-only build identity checks.</summary>
internal sealed class DebuggerSaveStateStore
{

    private readonly string directory;
    private readonly byte[] romDigest;

    public DebuggerSaveStateStore(
        string romPath,
        ReadOnlySpan<byte> cartridgeRom,
        string? directoryOverride = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(romPath);
        directory = directoryOverride is null
            ? Path.Combine(
                Path.GetDirectoryName(Path.GetFullPath(romPath))
                    ?? throw new InvalidOperationException("ROM path has no parent directory."),
                "debug-states")
            : Path.GetFullPath(directoryOverride);
        romDigest = SHA256.HashData(cartridgeRom);
    }

    public string DirectoryPath => directory;

    public string GetSlotPath(int slot)
    {
        ValidateSlot(slot);
        return Path.Combine(directory, $"SuperMetroid-debug-slot-{slot}.smstate");
    }

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
            slot,
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
                writer.Write(DebuggerStateFormat.CurrentVersion);
                writer.Write(typeof(SuperMetroidGame).Module.ModuleVersionId.ToByteArray());
                writer.Write(typeof(DebuggerSaveStateStore).Module.ModuleVersionId.ToByteArray());
                writer.Write(romDigest);
                writer.Write(metadata.SavedUtc.UtcTicks);
                writer.Write(metadata.FrameNumber);
                writer.Write((ushort)metadata.GameState);
                WriteNullableWord(writer, metadata.RoomPointer);
                WriteNullableWord(writer, metadata.RoomStatePointer);
                writer.Flush();

                using var compressed = new GZipStream(stream, CompressionLevel.SmallestSize, leaveOpen: true);
                DebuggerObjectGraphSerializer.Serialize(
                    compressed,
                    new DebuggerSaveStateRoot(addressSpace, game, audioPlayer));
            }
            File.Move(temporary, destination, overwrite: true);
            return metadata;
        }
        catch
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
            throw;
        }
    }

    public DebuggerSaveStateLoadResult Load(int slot)
    {
        ValidateSlot(slot);
        string path = GetSlotPath(slot);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Debugger save-state slot {slot} does not exist.", path);

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
        if (version is not (DebuggerStateFormat.CurrentVersion or DebuggerStateFormat.LegacyTokenVersion))
        {
            throw new InvalidDataException(
                $"Debugger state schema {version} is incompatible with schema {DebuggerStateFormat.CurrentVersion}.");
        }
        var warnings = new List<string>();
        ReadBuildIdentity(reader, typeof(SuperMetroidGame).Module.ModuleVersionId, "core build", warnings);
        ReadBuildIdentity(reader, typeof(DebuggerSaveStateStore).Module.ModuleVersionId, "desktop build", warnings);
        if (warnings.Count != 0 && version == DebuggerStateFormat.LegacyTokenVersion)
            warnings.Add("Legacy debugger state uses compiler method tokens; cross-build delegate compatibility cannot be guaranteed.");
        foreach (string warning in warnings) Console.Error.WriteLine($"WARNING: {warning}");
        byte[] storedDigest = reader.ReadBytes(SHA256.HashSizeInBytes);
        if (storedDigest.Length != SHA256.HashSizeInBytes ||
            !CryptographicOperations.FixedTimeEquals(storedDigest, romDigest))
        {
            throw new InvalidDataException(
                $"Debugger state slot {slot} was captured from a different ROM (SHA-256 mismatch).");
        }

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

        return new DebuggerSaveStateLoadResult(
            root.AddressSpace,
            root.Game,
            root.AudioPlayer,
            new DebuggerSaveStateMetadata(slot, savedUtc, frame, gameState, room, roomState, path), warnings.AsReadOnly());
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

    private void EnsureRomMatches(SuperMetroidAddressSpace addressSpace)
    {
        byte[] actual = SHA256.HashData(addressSpace.Rom);
        if (!CryptographicOperations.FixedTimeEquals(actual, romDigest))
            throw new InvalidDataException("Live address space does not match the configured ROM digest.");
    }

    private static void ReadBuildIdentity(BinaryReader reader, Guid expected, string label, List<string> warnings)
    {
        byte[] bytes = reader.ReadBytes(DebuggerStateFormat.GuidBytes);
        if (bytes.Length != DebuggerStateFormat.GuidBytes)
            throw new InvalidDataException($"Debugger state {label} identity is truncated.");
        if (new Guid(bytes) != expected)
            warnings.Add($"Debugger state {label} differs from this executable; attempting compatible restoration. Behavior may differ from the captured build.");
    }

    private static void WriteNullableWord(BinaryWriter writer, ushort? value)
    {
        writer.Write(value.HasValue);
        if (value.HasValue)
            writer.Write(value.Value);
    }

    private static ushort? ReadNullableWord(BinaryReader reader) =>
        reader.ReadBoolean() ? reader.ReadUInt16() : null;

    private static void ValidateSlot(int slot)
    {
        if ((uint)slot >= DebuggerStateFormat.SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slot), slot, "Debugger state slot must be 0-9.");
    }

    private sealed class DebuggerSaveStateRoot(
        SuperMetroidAddressSpace addressSpace,
        SuperMetroidGame game,
        ManagedSpcPlayer? audioPlayer)
    {
        public readonly SuperMetroidAddressSpace AddressSpace = addressSpace;
        public readonly SuperMetroidGame Game = game;
        public readonly ManagedSpcPlayer? AudioPlayer = audioPlayer;
    }
}

internal readonly record struct DebuggerSaveStateMetadata(
    int Slot,
    DateTimeOffset SavedUtc,
    ushort FrameNumber,
    SuperMetroidGameState GameState,
    ushort? RoomPointer,
    ushort? RoomStatePointer,
    string Path);

internal readonly record struct DebuggerSaveStateLoadResult(
    SuperMetroidAddressSpace AddressSpace,
    SuperMetroidGame Game,
    ManagedSpcPlayer? AudioPlayer,
    DebuggerSaveStateMetadata Metadata,
    IReadOnlyList<string> Warnings);
