using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms;

/// <summary>Complete installed room-layout artwork selected by the room state's level.</summary>
public sealed class RoomVisualLayoutCatalog
{
    /// <summary>Validated level selection for all installed room-level visual planes.</summary>
    private readonly Dictionary<RoomLevelId, RoomVisualLayout> layouts;

    /// <summary>Copies a level-keyed installation and requires each of <paramref name="requiredLevels"/> to have a matching non-null layout.</summary>
    /// <param name="layouts">Initial visual layouts keyed by their own <see cref="RoomVisualLayout.Level"/>.</param>
    /// <param name="requiredLevels">Every room level the installation must cover.</param>
    /// <exception cref="InvalidDataException">A value is null, its level differs from its key, or a required level is absent.</exception>
    public RoomVisualLayoutCatalog(IReadOnlyDictionary<RoomLevelId, RoomVisualLayout> layouts,
        IEnumerable<RoomLevelId> requiredLevels)
    {
        Ensure.NotNull(layouts);
        this.layouts = new Dictionary<RoomLevelId, RoomVisualLayout>(layouts);
        foreach ((RoomLevelId level, RoomVisualLayout layout) in this.layouts)
            if (layout is null || layout.Level != level)
                throw new InvalidDataException(
                    $"Room visual layout key {level} must contain its matching nonnull layout.");
        foreach (RoomLevelId level in Ensure.NotNull(requiredLevels))
            if (!this.layouts.ContainsKey(level))
                throw new InvalidDataException(
                    $"Installed room visual layouts lack required level {level}.");
    }

    /// <summary>SHA-256 of selected room geometry and both ordered visual planes.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomVisualLayoutCatalog), content =>
    {
        foreach ((RoomLevelId level, RoomVisualLayout layout) in this.layouts.OrderBy(pair => pair.Key.Name, StringComparer.Ordinal))
        {
            content.AppendName("level", level.Name);
            content.Append("width", layout.Size.X);
            content.Append("height", layout.Size.Y);
            content.AppendWords("foreground", layout.ForegroundVisualWords.Span);
            content.AppendWords("background", layout.BackgroundVisualWords.Span);
        }
    });

    /// <summary>Resolves installed initial artwork by the room state's level, without selecting room state or reading cartridge bytes.</summary>
    /// <param name="level">The level the room state names, used as the installation key.</param>
    /// <returns>The matching validated visual layout.</returns>
    /// <exception cref="InvalidDataException">The requested level is not installed.</exception>
    public RoomVisualLayout Get(RoomLevelId level) =>
        layouts.TryGetValue(level, out RoomVisualLayout? layout)
            ? layout
            : throw new InvalidDataException(
                $"Installed room visual layouts lack level {level}.");
}
