namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Editable mapping from timed projectile records to presentation-only compositions, leaving native instruction mechanics unchanged.</summary>
public sealed record ProjectileFrameBindingDocument
{
    /// <summary>Schema revision; loading requires <see cref="ProjectileFrameBindingFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 805 required record keys from <see cref="ProjectileFrameBindingFormat.FrameName"/>, each assigned a composition name from <see cref="ProjectileSpriteDefinitions.All"/>; multiple records may share one composition.</summary>
    public required Dictionary<string, string> Frames { get; init; }
}
