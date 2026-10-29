using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Editable, visual-only coordinates and OAM parts of one component.</summary>
[SpatialComponents("External format: the extended-frame JSON stores each component displacement as offsetX/offsetY.")]
public sealed record EnemyExtendedVisualComponent
{
    /// <summary>Gets the signed whole-pixel horizontal displacement from the enemy anchor.</summary>
    public required int OffsetX { get; init; }
    /// <summary>Gets the signed whole-pixel vertical displacement from the enemy anchor.</summary>
    public required int OffsetY { get; init; }
    /// <summary>Gets the ordered visual-only OAM parts drawn for this component.</summary>
    public required SpriteVisualPart[] Parts { get; init; }
}
