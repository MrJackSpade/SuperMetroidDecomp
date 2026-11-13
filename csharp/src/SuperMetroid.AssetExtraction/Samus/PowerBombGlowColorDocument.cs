using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Editable five-bit component sequences of the explosion's glow colours, imported from the bank-$88 fixed-colour tables; entries are not CGRAM palette words or COLDATA command bytes.</summary>
public sealed record PowerBombGlowColorDocument
{
    /// <summary>Schema revision; loading requires <see cref="PowerBombGlowColorFile.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen nonnull RGB5 triplets corresponding to $88:9079, indexed by the 16-bit pre-explosion radius shifted right eleven bits and masked to 0..15.</summary>
    public required PaletteRgb5[] PreExplosion { get; init; }
    /// <summary>Thirty-two nonnull RGB5 triplets corresponding to $88:8D85, indexed by the 16-bit explosion radius shifted right eleven bits, not by elapsed gameplay updates.</summary>
    public required PaletteRgb5[] Explosion { get; init; }
}
