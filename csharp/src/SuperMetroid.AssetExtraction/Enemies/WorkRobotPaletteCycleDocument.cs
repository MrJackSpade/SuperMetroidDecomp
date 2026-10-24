using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>JSON schema for the six ordered Work Robot RGB5 palette records.</summary>
public sealed record WorkRobotPaletteCycleDocument
{
    /// <summary>Schema revision; loading requires <see cref="WorkRobotPaletteCycleFormat.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Six ordered frames of four editable RGB5 colors, independent of native frame timing.</summary>
    public required PaletteRgb5[][] Frames { get; init; }
}
