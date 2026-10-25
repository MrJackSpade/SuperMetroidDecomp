namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// Installed native room-level allocations used as the collision/BTS and streaming
/// baseline. Each stream is the complete, unmodified decompressor output for one retail
/// room level, extracted from the user's cartridge at installation. The catalog is
/// stock-only: editable BG1/BG2 visual words are selected separately by
/// <see cref="RoomVisualLayoutCatalog"/> and never rewrite this baseline.
/// </summary>
public sealed class RoomLevelStreamCatalog
{
    /// <summary>Smallest valid stream: the leading BG1 layer-size word.</summary>
    internal const int MinimumStreamByteCount = 2;

    /// <summary>Complete decompressed native level streams keyed by their room level.</summary>
    internal readonly Dictionary<RoomLevelId, ReadOnlyMemory<byte>> streams;

    /// <summary>Copies a validated installation and requires exactly every retail room level.</summary>
    /// <param name="streams">Complete decompressed native streams keyed by their room level.</param>
    /// <param name="contentIdentity">SHA-256 of the installed stream manifest.</param>
    /// <param name="requiredLevels">Every retail room level, each of which the installation must hold.</param>
    /// <exception cref="InvalidDataException">A stream is null or shorter than its size word, or a level is missing or unknown.</exception>
    internal RoomLevelStreamCatalog(IReadOnlyDictionary<RoomLevelId, byte[]> streams, string contentIdentity, IEnumerable<RoomLevelId> requiredLevels)
    {
        Ensure.NotNull(streams);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentIdentity);
        ContentIdentity = contentIdentity;
        var required = Ensure.NotNull(requiredLevels).ToHashSet();
        this.streams = new Dictionary<RoomLevelId, ReadOnlyMemory<byte>>(streams.Count);
        foreach ((RoomLevelId level, byte[] stream) in streams)
        {
            if (!required.Contains(level))
                throw new InvalidDataException($"Installed room-level streams contain unknown level {level}.");
            if (stream is null || stream.Length < MinimumStreamByteCount)
                throw new InvalidDataException($"Installed room-level stream {level} has no layer-size word.");
            this.streams.Add(level, stream.ToArray());
        }
        foreach (RoomLevelId level in required)
            if (!this.streams.ContainsKey(level))
                throw new InvalidDataException($"Installed room-level streams lack required level {level}.");
    }

    /// <summary>SHA-256 of the installed stream manifest, which names every stream's hash.</summary>
    public string ContentIdentity { get; }

    /// <summary>Reads one complete decompressed native level stream by its room level.</summary>
    /// <param name="level">The level the room state names.</param>
    /// <returns>The unmodified installed stream, including any native overread tail.</returns>
    /// <exception cref="InvalidDataException">The requested level is not installed; room loads never fall back to the ROM.</exception>
    public ReadOnlyMemory<byte> Get(RoomLevelId level) =>
        streams.TryGetValue(level, out ReadOnlyMemory<byte> stream)
            ? stream
            : throw new InvalidDataException(
                $"Installed room-level data lacks level {level}.");
}
