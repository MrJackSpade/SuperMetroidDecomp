namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// Installed native room-level allocations used as the collision/BTS and streaming
/// baseline. Each stream is the complete, unmodified decompressor output for one retail
/// level source, extracted from the user's cartridge at installation. The catalog is
/// stock-only: editable BG1/BG2 visual words are selected separately by
/// <see cref="RoomVisualLayoutCatalog"/> and never rewrite this baseline.
/// </summary>
public sealed class RoomLevelStreamCatalog
{
    /// <summary>Smallest valid stream: the leading BG1 layer-size word.</summary>
    internal const int MinimumStreamByteCount = 2;

    /// <summary>Complete decompressed native level streams keyed by their 24-bit source address.</summary>
    internal readonly Dictionary<int, ReadOnlyMemory<byte>> streams;

    /// <summary>Copies a validated installation and requires exactly every retail level source.</summary>
    /// <param name="streams">Complete decompressed native streams keyed by their 24-bit source address.</param>
    /// <param name="contentIdentity">SHA-256 of the installed stream manifest.</param>
    /// <exception cref="InvalidDataException">A stream is null or shorter than its size word, or a source is missing or unknown.</exception>
    internal RoomLevelStreamCatalog(IReadOnlyDictionary<int, byte[]> streams, string contentIdentity)
    {
        Ensure.NotNull(streams);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentIdentity);
        ContentIdentity = contentIdentity;
        var required = RoomVisualLayoutSourceDefinitions.All.ToHashSet();
        this.streams = new Dictionary<int, ReadOnlyMemory<byte>>(streams.Count);
        foreach ((int source, byte[] stream) in streams)
        {
            if (!required.Contains(source))
                throw new InvalidDataException($"Installed room-level streams contain unknown source ${source:X6}.");
            if (stream is null || stream.Length < MinimumStreamByteCount)
                throw new InvalidDataException($"Installed room-level stream ${source:X6} has no layer-size word.");
            this.streams.Add(source, stream.ToArray());
        }
        foreach (int source in required)
            if (!this.streams.ContainsKey(source))
                throw new InvalidDataException($"Installed room-level streams lack required source ${source:X6}.");
    }

    /// <summary>SHA-256 of the installed stream manifest, which names every stream's hash.</summary>
    public string ContentIdentity { get; }

    /// <summary>Reads one complete decompressed native level stream by cartridge source identity.</summary>
    /// <param name="sourceAddress">Full 24-bit level-data source identity selected by the room state.</param>
    /// <returns>The unmodified installed stream, including any native overread tail.</returns>
    /// <exception cref="InvalidDataException">The requested source is not installed; room loads never fall back to the ROM.</exception>
    public ReadOnlyMemory<byte> Get(int sourceAddress) =>
        streams.TryGetValue(sourceAddress, out ReadOnlyMemory<byte> stream)
            ? stream
            : throw new InvalidDataException(
                $"Installed room-level data lacks source ${sourceAddress:X6}.");
}
