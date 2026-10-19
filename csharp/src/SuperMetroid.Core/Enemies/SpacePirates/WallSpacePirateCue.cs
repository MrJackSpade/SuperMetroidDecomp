namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the wall Space Pirates' programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="ClimbPixels">For a climb cue: the whole pixels the Pirate moves down the wall; negative climbs up.</param>
/// <param name="Function">The cue's Function operand.</param>
internal readonly record struct WallSpacePirateCue(WallSpacePirateCueKind Kind, short ClimbPixels = 0, WallSpacePirateFunction Function = default);
