using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>One component of an extended enemy frame: its displacement and its packed sprite parts, import data only the import composes pictures from (#1638).</summary>
public sealed partial record EnemyExtendedVisualComponent
{
    /// <summary>Gets the signed whole-pixel horizontal displacement from the enemy anchor.</summary>
    public required int OffsetX { get; init; }
    /// <summary>Gets the signed whole-pixel vertical displacement from the enemy anchor.</summary>
    public required int OffsetY { get; init; }
    /// <summary>Gets the ordered visual-only OAM parts drawn for this component.</summary>
    public required SpriteVisualPart[] Parts { get; init; }
}
