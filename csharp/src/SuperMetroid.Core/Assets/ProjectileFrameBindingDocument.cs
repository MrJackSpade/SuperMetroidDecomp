namespace SuperMetroid.Core.Assets;

/// <summary>Editable mapping from bank-$93 timed projectile records to presentation-only sprite identities, leaving native instruction mechanics unchanged.</summary>
public sealed record ProjectileFrameBindingDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="ProjectileFrameBindingFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 805 required frame_XXXX record keys from <see cref="ProjectileFrameBindingFormat.FrameName"/>, each assigned a sprite_XXXX identity from <see cref="ProjectileSpriteDefinitions.NativePointers"/>; multiple records may share one sprite.</summary>
    public required Dictionary<string, string> Frames { get; init; }
}
