namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the walking Space Pirates' programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="LaserHeight">For a laser cue: how many pixels above the Pirate's origin the laser is emitted; negative is below.</param>
/// <param name="Function">The cue's Function operand.</param>
internal readonly record struct WalkingSpacePirateCue(WalkingSpacePirateCueKind Kind, short LaserHeight = 0, WalkingSpacePirateFunction Function = default);
