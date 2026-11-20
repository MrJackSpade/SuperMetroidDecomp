namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Editable appearance-only schema for trail timed records; its frame dictionary remains caller-mutable and contains no movement commands or frame durations.</summary>
public sealed record ProjectileTrailDocument
{
    /// <summary>Schema revision; loading requires <see cref="ProjectileTrailVisualDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>One nonnull appearance for each trail frame the trail records show, keyed by the frame's installed key.</summary>
    public required Dictionary<string, ProjectileTrailAppearance> Frames { get; init; }
}
