using System.IO.Compression;
using System.Security.Cryptography;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Desktop;

/// <summary>Persistent ten-slot exact-build debugger states for the desktop host.</summary>
internal sealed class DebuggerSaveStateStore
{
    private static ReadOnlySpan<byte> Magic => "SMCSTATE"u8;
    private const int FormatVersion = 1;
    private const int SlotCount = 10;

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
        SuperMetroidGame game)
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
                writer.Write(Magic);
                writer.Write(FormatVersion);
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
                    new DebuggerSaveStateRoot(addressSpace, game));
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
        byte[] magic = reader.ReadBytes(Magic.Length);
        if (!magic.AsSpan().SequenceEqual(Magic))
            throw new InvalidDataException($"'{path}' is not a Super Metroid debugger state.");
        int version = reader.ReadInt32();
        if (version != FormatVersion)
        {
            throw new InvalidDataException(
                $"Debugger state schema {version} is incompatible with schema {FormatVersion}.");
        }
        VerifyGuid(reader, typeof(SuperMetroidGame).Module.ModuleVersionId, "core build");
        VerifyGuid(reader, typeof(DebuggerSaveStateStore).Module.ModuleVersionId, "desktop build");
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
        DebuggerSaveStateRoot root = DebuggerObjectGraphSerializer.Deserialize<DebuggerSaveStateRoot>(compressed);
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
            new DebuggerSaveStateMetadata(slot, savedUtc, frame, gameState, room, roomState, path));
    }

    private void EnsureRomMatches(SuperMetroidAddressSpace addressSpace)
    {
        byte[] actual = SHA256.HashData(addressSpace.Rom);
        if (!CryptographicOperations.FixedTimeEquals(actual, romDigest))
            throw new InvalidDataException("Live address space does not match the configured ROM digest.");
    }

    private static void VerifyGuid(BinaryReader reader, Guid expected, string label)
    {
        byte[] bytes = reader.ReadBytes(16);
        if (bytes.Length != 16 || new Guid(bytes) != expected)
        {
            throw new InvalidDataException(
                $"Debugger state {label} does not match this executable; exact-build restore is required.");
        }
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
        if ((uint)slot >= SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slot), slot, "Debugger state slot must be 0-9.");
    }

    private sealed class DebuggerSaveStateRoot(
        SuperMetroidAddressSpace addressSpace,
        SuperMetroidGame game)
    {
        public readonly SuperMetroidAddressSpace AddressSpace = addressSpace;
        public readonly SuperMetroidGame Game = game;
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
    DebuggerSaveStateMetadata Metadata);
