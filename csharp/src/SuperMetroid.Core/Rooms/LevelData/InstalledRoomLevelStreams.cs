namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The installed terrain of every retail room level (#1469): each level's grid of terrain blocks, behaviours and
/// background appearances, and the terrain past its grid, decoded once at installation. The catalog is stock-only:
/// replaceable initial appearances are selected separately by <see cref="RoomLevelArtworks"/> and never change it.
/// </summary>
public sealed class InstalledRoomLevelStreams
{
    /// <summary>Installed levels keyed by their room level.</summary>
    internal readonly Dictionary<RoomLevelId, InstalledRoomLevel> levels;

    /// <summary>Copies a validated installation and requires exactly every retail room level.</summary>
    /// <param name="levels">The installed levels keyed by their room level.</param>
    /// <param name="contentIdentity">SHA-256 of the installed manifest.</param>
    /// <param name="requiredLevels">Every retail room level, each of which the installation must hold.</param>
    /// <exception cref="InvalidDataException">A level is null, missing or unknown.</exception>
    internal InstalledRoomLevelStreams(IReadOnlyDictionary<RoomLevelId, InstalledRoomLevel> levels, string contentIdentity,
        IEnumerable<RoomLevelId> requiredLevels)
    {
        Ensure.NotNull(levels);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentIdentity);
        ContentIdentity = contentIdentity;
        var required = Ensure.NotNull(requiredLevels).ToHashSet();
        this.levels = new Dictionary<RoomLevelId, InstalledRoomLevel>(levels.Count);
        foreach ((RoomLevelId level, InstalledRoomLevel installed) in levels)
        {
            if (!required.Contains(level))
                throw new InvalidDataException($"Installed room levels contain unknown level {level}.");
            this.levels.Add(level, installed ?? throw new InvalidDataException($"Installed room level {level} is null."));
        }
        foreach (RoomLevelId level in required)
            if (!this.levels.ContainsKey(level))
                throw new InvalidDataException($"Installed room levels lack required level {level}.");
    }

    /// <summary>SHA-256 of the installed manifest, which names every level file's hash.</summary>
    public string ContentIdentity { get; }

    /// <summary>Reads one installed level.</summary>
    /// <param name="level">The level the room state names.</param>
    /// <returns>The level's installed terrain.</returns>
    /// <exception cref="InvalidDataException">The requested level is not installed; room loads never fall back to the ROM.</exception>
    public InstalledRoomLevel Get(RoomLevelId level) =>
        levels.TryGetValue(level, out InstalledRoomLevel? installed)
            ? installed
            : throw new InvalidDataException($"Installed room-level data lacks level {level}.");
}
