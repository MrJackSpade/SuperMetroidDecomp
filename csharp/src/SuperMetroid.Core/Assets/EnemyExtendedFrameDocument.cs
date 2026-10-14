namespace SuperMetroid.Core.Assets;

/// <summary>Versioned editable extended-enemy-frame compositions.</summary>
public sealed record EnemyExtendedFrameDocument
{
    /// <summary>Gets the extended-frame schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets every named visual frame composition defined by this schema revision.</summary>
    public required Dictionary<string, EnemyExtendedVisualComponent[]> Frames { get; init; }
    /// <summary>Visual-only frame selection; native timers and hitboxes remain fixed.</summary>
    public Dictionary<string, string>? DisplayFrames { get; init; }
}
