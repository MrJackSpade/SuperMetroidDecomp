using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Editable JSON schema for the complete Ceres approach palette, also reused by the destruction scene; PNG transport palettes do not supply these rendered colors.</summary>
public sealed record CeresFlightPaletteDocument
{
    /// <summary>Schema revision, which must equal <see cref="CeresFlightPaletteFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 256 non-null colors with channels in 0..31, ordered by CGRAM index and matching native source words at <c>$8C:E5E9 + 2 * index</c>.</summary>
    public required PaletteRgb5[] Colors { get; init; }
}
