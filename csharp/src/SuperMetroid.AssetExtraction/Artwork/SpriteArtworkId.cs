using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Owned identity of one sprite artwork sheet. It names installed artwork by what it is, never by the
/// storage location or upload slot it was once transferred from. A family of one sheet is named by its kind alone; a
/// family of several names each sheet by a typed variant of what distinguishes it (a beam combination, a Grapple tile
/// selection, the half pictures a Samus sheet composes), never by a number.
/// </summary>
public readonly partial record struct SpriteArtworkId
{
    /// <summary>Names the one sheet of <paramref name="kind"/>.</summary>
    /// <param name="kind">The artwork family.</param>
    public SpriteArtworkId(SpriteArtworkKind kind) => Kind = kind;

    /// <summary>Names the sheet of <paramref name="kind"/> that <paramref name="variant"/> distinguishes.</summary>
    /// <param name="kind">The artwork family.</param>
    /// <param name="variant">What distinguishes the sheet within its family.</param>
    public SpriteArtworkId(SpriteArtworkKind kind, SpriteArtworkVariant variant)
    {
        Kind = kind;
        Variant = Ensure.NotNull(variant);
    }

    /// <summary>The artwork family.</summary>
    public SpriteArtworkKind Kind { get; }

    /// <summary>What distinguishes the sheet within a family of several, or none for a family of one sheet.</summary>
    private SpriteArtworkVariant? Variant { get; }

    /// <summary>The same sheet's identity in <paramref name="kind"/>, as a page composed over another keeps its variant.</summary>
    /// <param name="kind">The family of the composed sheet.</param>
    public SpriteArtworkId InKind(SpriteArtworkKind kind) =>
        Variant is { } variant ? new SpriteArtworkId(kind, variant) : new SpriteArtworkId(kind);
}
