using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Editable RGB5 color document for one externally selected ending palette role; the color array remains caller-mutable until compilation.</summary>
public sealed record EndingPaletteDocument
{
    /// <summary>Schema revision; loading requires <see cref="EndingPaletteDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Ordered, nonnull RGB5 entries with each channel in 0..31; the role requires 256, 16 or 512 colors. Logo crossfade order is sixteen steps of 16 BG colors followed by 16 OBJ colors.</summary>
    public required PaletteRgb5[] Colors { get; init; }
}
