namespace SuperMetroid.Core.Assets;

/// <summary>Presentation-only enemy-projectile JSON schema: named ordered OAM compositions and program-frame bindings, with no executable instruction or gameplay fields.</summary>
public sealed record EnemyProjectileSpritemapDocument
{
    /// <summary>Schema generation determining the required composition and program-frame name sets; older generations require a current-stock fallback at load time.</summary>
    public required int Version { get; init; }
    /// <summary>Named bank-$8D Ceres elevator and Skree/Metaree debris compositions, each represented by ordered sprite parts; version 1 contains only the three Ceres frames.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
    /// <summary>Named visual frames keyed by the schema's bank-$86 spritemap-operand identities, required from version 3 onward; editing these parts does not replace the timed instruction program.</summary>
    public Dictionary<string, SpriteVisualPart[]>? ProgramFrames { get; init; }
}
