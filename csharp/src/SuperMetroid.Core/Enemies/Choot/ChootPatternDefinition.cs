namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>One Choot falling stream and its per-loop vertical advance.</summary>
internal readonly record struct ChootPatternDefinition(
    ChootFallingPath FallingPatternPointer,
    ushort FallingPatternYDistance);
