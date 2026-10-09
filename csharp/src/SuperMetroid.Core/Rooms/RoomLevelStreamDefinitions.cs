using System.Buffers.Binary;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Immutable native room-level allocations used as the collision/BTS and streaming
/// baseline. Editable BG1/BG2 visual words are selected separately by
/// <see cref="RoomVisualLayoutCatalog"/> and never rewrite this baseline.
/// </summary>
public static class RoomLevelStreamDefinitions
{
    /// <summary>Assembly resource containing the compiled decompressed level-stream corpus.</summary>
    private const string ResourceName = "SuperMetroid.RoomLevelStreams.bin";

    /// <summary>Version identifier stored in the corpus header after its magic bytes.</summary>
    private const int FormatVersion = 1;

    /// <summary>Size of the fixed corpus header before the per-source records begin.</summary>
    private const int HeaderByteCount = 44;

    /// <summary>Loads and validates the embedded corpus once, on first access.</summary>
    private static readonly Lazy<Corpus> Installed = new(Load);

    /// <summary>Reads one complete decompressed native level stream by cartridge source identity.</summary>
    public static ReadOnlyMemory<byte> Get(int sourceAddress) =>
        Installed.Value.Streams.TryGetValue(sourceAddress, out ReadOnlyMemory<byte> stream)
            ? stream
            : throw new InvalidDataException(
                $"Compiled room-level data lacks source ${sourceAddress:X6}.");

    /// <summary>Parses the embedded corpus and validates its header, source set, record lengths, and exact end.</summary>
    /// <returns>The indexed native level streams backed by the loaded resource bytes.</returns>
    /// <exception cref="InvalidDataException">The resource is missing, malformed, incomplete, or contains unexpected sources.</exception>
    private static Corpus Load()
    {
        using Stream source = typeof(RoomLevelStreamDefinitions).Assembly
            .GetManifestResourceStream(ResourceName)
            ?? throw new InvalidDataException($"Missing compiled resource {ResourceName}.");
        using var copy = new MemoryStream();
        source.CopyTo(copy);
        byte[] data = copy.ToArray();
        if (data.Length < HeaderByteCount || !data.AsSpan(0, 4).SequenceEqual("SMLV"u8) ||
            BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(4)) != FormatVersion)
            throw new InvalidDataException("Compiled room-level corpus has an invalid header.");
        int count = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(40));
        HashSet<int> expected = BuildExpectedSources();
        if (count != expected.Count)
            throw new InvalidDataException(
                $"Compiled room-level corpus has {count} sources, expected {expected.Count}.");

        var streams = new Dictionary<int, ReadOnlyMemory<byte>>(count);
        int cursor = HeaderByteCount;
        for (int index = 0; index < count; index++)
        {
            if (cursor > data.Length - 8)
                throw new InvalidDataException("Compiled room-level corpus ends inside a source header.");
            int address = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(cursor));
            int length = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(cursor + 4));
            cursor += 8;
            if (!expected.Contains(address) || length < 2 || length > data.Length - cursor ||
                !streams.TryAdd(address, data.AsMemory(cursor, length)))
                throw new InvalidDataException(
                    $"Compiled room-level corpus has invalid source ${address:X6} or length {length}.");
            cursor += length;
        }
        if (cursor != data.Length)
            throw new InvalidDataException("Compiled room-level corpus has trailing bytes.");
        return new Corpus(streams);
    }

    /// <summary>Collects every compressed level-data source selected by the installed room states.</summary>
    /// <returns>The distinct cartridge addresses that the compiled corpus must contain.</returns>
    private static HashSet<int> BuildExpectedSources()
    {
        var expected = new HashSet<int>();
        foreach (RoomHeaderDefinition room in RoomHeaderDefinitions.All)
        foreach (ushort statePointer in RoomStateSelectionDefinitions.GetStatePointers(room.Pointer))
            expected.Add(RoomStateDefinitions.Get(statePointer).CompressedLevelDataAddress);
        return expected;
    }

    /// <summary>Validated room-level streams indexed by their original cartridge source address.</summary>
    /// <param name="Streams">Complete native level allocations keyed by compressed level-data address.</param>
    private sealed record Corpus(
        IReadOnlyDictionary<int, ReadOnlyMemory<byte>> Streams);
}
