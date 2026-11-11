using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>Serializable item-icon anchor and tile cells.</summary>
public sealed record GameplayHudIconDocument
{
    /// <summary>Gets the upper-left coordinate of the icon.</summary>
    public required Position16<CharacterCells> Anchor { get; init; }
    /// <summary>Gets the icon cells in row-major order.</summary>
    public required GameplayHudCell[] Cells { get; init; }
}
