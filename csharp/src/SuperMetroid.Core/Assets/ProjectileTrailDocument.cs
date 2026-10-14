namespace SuperMetroid.Core.Assets;

/// <summary>Editable appearance-only schema for trail timed records; its frame dictionary remains caller-mutable and contains no movement commands or frame durations.</summary>
public sealed record ProjectileTrailDocument
{
    /// <summary>Schema revision; loading requires <see cref="ProjectileTrailVisualDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 42 nonnull appearances keyed as trail_XXXX with uppercase hexadecimal bank-$90 timed-record addresses; left/right ice each contribute 17, and wave/missile each contribute four.</summary>
    public required Dictionary<string, ProjectileTrailAppearance> Frames { get; init; }
}
