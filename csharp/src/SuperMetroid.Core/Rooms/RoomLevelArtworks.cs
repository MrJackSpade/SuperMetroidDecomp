using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms;

/// <summary>Complete installed room-layout artwork selected by the room state's level.</summary>
public sealed class RoomLevelArtworks
{
    /// <summary>Validated level selection for all installed room-level visual planes.</summary>
    private readonly Dictionary<RoomLevelId, RoomLevelArtwork> layouts;

    /// <summary>Copies a level-keyed installation and requires each of <paramref name="requiredLevels"/> to have a matching non-null layout.</summary>
    /// <param name="layouts">Initial visual layouts keyed by their own <see cref="RoomLevelArtwork.Level"/>.</param>
    /// <param name="requiredLevels">Every room level the installation must cover.</param>
    /// <exception cref="InvalidDataException">A value is null, its level differs from its key, or a required level is absent.</exception>
    public RoomLevelArtworks(IReadOnlyDictionary<RoomLevelId, RoomLevelArtwork> layouts,
        IEnumerable<RoomLevelId> requiredLevels)
    {
        Ensure.NotNull(layouts);
        this.layouts = new Dictionary<RoomLevelId, RoomLevelArtwork>(layouts);
        foreach ((RoomLevelId level, RoomLevelArtwork layout) in this.layouts)
            if (layout is null || layout.Level != level)
                throw new InvalidDataException(
                    $"Room visual layout key {level} must contain its matching nonnull layout.");
        foreach (RoomLevelId level in Ensure.NotNull(requiredLevels))
            if (!this.layouts.ContainsKey(level))
                throw new InvalidDataException(
                    $"Installed room visual layouts lack required level {level}.");
    }

    /// <summary>SHA-256 of selected room geometry and both ordered planes of appearances.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomLevelArtworks), content =>
    {
        foreach ((RoomLevelId level, RoomLevelArtwork layout) in this.layouts.OrderBy(pair => pair.Key.Name, StringComparer.Ordinal))
        {
            content.AppendName("level", level.Name);
            content.Append("width", layout.Size.X);
            content.Append("height", layout.Size.Y);
            content.AppendAppearances("foreground", layout.Foreground.Span);
            content.AppendAppearances("background", layout.Background.Span);
        }
    });

    /// <summary>Resolves installed initial artwork by the room state's level, without selecting room state or reading cartridge bytes.</summary>
    /// <param name="level">The level the room state names, used as the installation key.</param>
    /// <returns>The matching validated visual layout.</returns>
    /// <exception cref="InvalidDataException">The requested level is not installed.</exception>
    public RoomLevelArtwork Get(RoomLevelId level) =>
        layouts.TryGetValue(level, out RoomLevelArtwork? layout)
            ? layout
            : throw new InvalidDataException(
                $"Installed room visual layouts lack level {level}.");
}
