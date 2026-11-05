using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Presentation-only enemy-projectile JSON schema: named ordered OAM compositions and program-frame bindings, with no executable instruction or gameplay fields.</summary>
public sealed record EnemyProjectileSpritemapDocument
{
    /// <summary>Schema revision; the reader accepts only <see cref="EnemyProjectileSpritemapDocuments.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Named bank-$8D Ceres elevator and Skree/Metaree debris compositions, each represented by ordered sprite parts.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
    /// <summary>Named visual frames keyed by the schema's bank-$86 spritemap-operand identities; editing these parts does not replace the timed instruction program.</summary>
    public Dictionary<string, SpriteVisualPart[]>? ProgramFrames { get; init; }
}
