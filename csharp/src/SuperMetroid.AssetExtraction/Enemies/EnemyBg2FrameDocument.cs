namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Named visual frames; no collision or instruction-program data.</summary>
public sealed record EnemyBg2FrameDocument
{
    /// <summary>Gets the owning Phantoon or Draygon frame schema version.</summary>
    public required int Version { get; init; }

    /// <summary>Gets every required named frame as an ordered sequence of horizontal BG2 writes.</summary>
    public required Dictionary<string, EnemyBg2WriteDocument[]> Frames { get; init; }
}
