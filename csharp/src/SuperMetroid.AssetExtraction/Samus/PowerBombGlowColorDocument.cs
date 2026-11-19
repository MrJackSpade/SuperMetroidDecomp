namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The installed colours of the explosion's glow, as a player edits them in place: the colour the glow shows at each
/// size, each written as <c>#rrggbb</c> (#2564).
/// </summary>
public sealed record PowerBombGlowColorDocument
{
    /// <summary>Schema revision; loading requires <see cref="PowerBombGlowColorFile.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>The sixteen colours the glow shows before the explosion, by the glow's radius, smallest first.</summary>
    public required string[] PreExplosion { get; init; }
    /// <summary>The thirty-two colours the glow shows while the explosion grows, by the explosion's radius, smallest first.</summary>
    public required string[] Explosion { get; init; }
}
