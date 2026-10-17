namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One cue of the Chozo statue's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="CarryRecord">The carry-motion record, 0 through 31, a <see cref="ChozoStatueCueKind.Move"/> cue moves by.</param>
/// <param name="FootstepOffset">Where a <see cref="ChozoStatueCueKind.Footstep"/> cue probes for spikes, relative to the statue; only its X is meaningful.</param>
/// <param name="PreInstruction">The cue's PreInstruction operand.</param>
internal readonly record struct ChozoStatueCue(
    ChozoStatueCueKind Kind,
    int CarryRecord = 0,
    Offset16<RoomPixels> FootstepOffset = default,
    ChozoStatuePreInstruction PreInstruction = default);
