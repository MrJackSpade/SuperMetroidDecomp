using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Editable five-bit fixed-color component sequences for bank-$88 explosion color math; entries are not CGRAM palette words or COLDATA command bytes.</summary>
public sealed record PowerBombFixedColorDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="PowerBombFixedColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen nonnull RGB5 triplets corresponding to $88:9079, indexed by the 16-bit pre-explosion radius shifted right eleven bits and masked to 0..15.</summary>
    public required PaletteRgb5[] PreExplosion { get; init; }
    /// <summary>Thirty-two nonnull RGB5 triplets corresponding to $88:8D85, indexed by the 16-bit explosion radius shifted right eleven bits, not by elapsed gameplay updates.</summary>
    public required PaletteRgb5[] Explosion { get; init; }
}
