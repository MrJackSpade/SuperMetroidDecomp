using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>The horizontal speeds of a Zoa's launch, by speed stage.</summary>
public static class ZoaSpeedDefinitions
{
    /// <summary>
    /// The launch's speed at each stage, in eighths of a pixel per frame: at rest before the shooting animation's first
    /// cue, then half, five eighths and two pixels per frame as its cues advance the stage.
    /// </summary>
    public static ImmutableArray<int> StageSpeedEighths { get; } = [0, 4, 5, 16];
}
