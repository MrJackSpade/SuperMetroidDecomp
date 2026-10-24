using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>JSON schema for Phantoon and Wrecked Ship power-transition colors.</summary>
public sealed record PhantoonColorDocument
{
    /// <summary>Gets the schema version required by <see cref="PhantoonColorFormat.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets eight health-band rows of sixteen editable RGB5 colors.</summary>
    public required PaletteRgb5[][] HealthBands { get; init; }

    /// <summary>Gets the sixteen-color target used while Phantoon fades out.</summary>
    public required PaletteRgb5[] FadeOut { get; init; }

    /// <summary>Gets the 112 colors forming seven powered Wrecked Ship palette rows.</summary>
    public required PaletteRgb5[] PowerOn { get; init; }
}
