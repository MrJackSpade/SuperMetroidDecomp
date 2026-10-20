using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>Complete installed room-layout artwork selected by the room state's source.</summary>
public sealed class RoomVisualLayoutCatalog
{
    /// <summary>Validated source-address selection for all installed room-level visual planes.</summary>
    private readonly Dictionary<int, RoomVisualLayout> layouts;

    /// <summary>Copies a source-keyed installation and requires every supported room-level source to have a matching non-null layout.</summary>
    /// <param name="layouts">Initial visual layouts keyed by their own full 24-bit <see cref="RoomVisualLayout.SourceAddress"/>.</param>
    /// <exception cref="InvalidDataException">A value is null, its source differs from its key, or a required source is absent.</exception>
    public RoomVisualLayoutCatalog(IReadOnlyDictionary<int, RoomVisualLayout> layouts)
        : this(layouts, requiredSources: RoomVisualLayoutSourceDefinitions.All)
    {
    }

    /// <summary>Copies layouts and requires each of <paramref name="requiredSources"/> to be present.</summary>
    internal RoomVisualLayoutCatalog(IReadOnlyDictionary<int, RoomVisualLayout> layouts,
        IEnumerable<int> requiredSources)
    {
        Ensure.NotNull(layouts);
        this.layouts = new Dictionary<int, RoomVisualLayout>(layouts);
        foreach ((int source, RoomVisualLayout layout) in this.layouts)
            if (layout is null || layout.SourceAddress != source)
                throw new InvalidDataException(
                    $"Room visual layout key ${source:X6} must contain its matching nonnull layout.");
        foreach (int source in Ensure.NotNull(requiredSources))
            if (!this.layouts.ContainsKey(source))
                throw new InvalidDataException(
                    $"Installed room visual layouts lack required source ${source:X6}.");
    }

    /// <summary>SHA-256 of selected room geometry and both ordered visual planes.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomVisualLayoutCatalog), content =>
    {
        foreach ((int source, RoomVisualLayout layout) in this.layouts.OrderBy(pair => pair.Key))
        {
            content.Append("source", source);
            content.Append("width", layout.Size.X);
            content.Append("height", layout.Size.Y);
            content.AppendWords("foreground", layout.ForegroundVisualWords.Span);
            content.AppendWords("background", layout.BackgroundVisualWords.Span);
        }
    });

    /// <summary>Resolves installed initial artwork by the room state's level-data source, without selecting room state or reading cartridge bytes.</summary>
    /// <param name="sourceAddress">Full 24-bit source identity used as the installation key.</param>
    /// <returns>The matching validated visual layout.</returns>
    /// <exception cref="InvalidDataException">The requested source is not installed.</exception>
    public RoomVisualLayout Get(int sourceAddress) =>
        layouts.TryGetValue(sourceAddress, out RoomVisualLayout? layout)
            ? layout
            : throw new InvalidDataException(
                $"Installed room visual layouts lack source ${sourceAddress:X6}.");
}
